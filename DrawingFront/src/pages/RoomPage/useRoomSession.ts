import { onBeforeUnmount, onMounted, watch } from "vue"
import { onBeforeRouteUpdate, useRoute, useRouter } from "vue-router"
import { toast } from "vue-sonner"

import * as hub from "@/realtime"
import { ROUTES } from "@/shared/config/routes"
import { usePresenceStore } from "@/features/presence"
import { useRoomStore } from "@/features/rooms"

function extractHubMessage(raw: string): string {
    const marker = "HubException: "
    const idx = raw.indexOf(marker)
    return idx === -1 ? raw : raw.slice(idx + marker.length)
}

function toErrorQuery(err: unknown) {
    const message = err instanceof Error ? extractHubMessage(err.message) : "Не удалось подключиться к доске."
    return { message }
}

export function useRoomSession() {
    const route = useRoute()
    const router = useRouter()
    const presenceStore = usePresenceStore()
    const roomStore = useRoomStore()

    const unsubscribers = [
        hub.onUsersUpdated((roomId, users) => presenceStore.setUsers(roomId, users)),
        hub.onKicked((message) => {
            presenceStore.setRoomDeletedMessage(message)
            presenceStore.clearUsers()
        }),
    ]

    async function enterRoom(roomId: string) {
        roomStore.loadCurrentRoom(roomId).catch(() => {})

        presenceStore.setCurrentRoom(roomId)
        await hub.switchRoom(roomId)
    }

    onMounted(async () => {
        await hub.connect()
        try {
            await enterRoom(route.params.id as string)
        } catch (err) {
            router.push({ path: ROUTES.connectionError, query: toErrorQuery(err) })
        }
    })

    watch(
        () => presenceStore.roomDeletedMessage,
        (message) => {
            if (message) {
                toast.error(message)
                router.push({ path: ROUTES.rooms })
                presenceStore.setRoomDeletedMessage(null)
            }
        }
    )

    onBeforeRouteUpdate(async (to, from) => {
        if (to.params.id === from.params.id) {
            return
        }

        try {
            await enterRoom(to.params.id as string)
        } catch (err) {
            return { path: ROUTES.connectionError, query: toErrorQuery(err) }
        }
    })

    onBeforeUnmount(async () => {
        unsubscribers.forEach(unsubscribe => unsubscribe())
        try {
            await hub.leaveCurrentRoom()
        } catch {
        }
        roomStore.clearCurrentRoom()
        presenceStore.clearUsers()
        await hub.disconnect()
    })
}
