<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRouter, useRoute } from "vue-router";

import { Button } from "@/components/ui/button";
import { LogOut, Loader2 } from "lucide-vue-next";
import { ROUTES, roomPath } from "@/shared/config/routes";
import { useAuthStore } from "@/features/auth";
import { useRoomStore, CreateRoomDialog, ShareRoomButton } from "@/features/rooms";
import { OnlineUsers } from "@/features/presence";

const router = useRouter();
const route = useRoute();

const roomStore = useRoomStore();
const authStore = useAuthStore();

const isLoadingRooms = ref(false);

onMounted(async () => {
    if (roomStore.rooms.length === 0) {
        isLoadingRooms.value = true;
        try {
            await roomStore.loadRooms();
        } finally {
            isLoadingRooms.value = false;
        }
    }
});

function openRoom(id: string) {
    if (route.params.id === id) {
        return;
    }
    router.push(roomPath(id));
}

function handleLogout() {
    authStore.logout();
    router.push(ROUTES.login);
}
</script>

<template>
    <div class="flex w-64 flex-col border-r p-4">
        <div class="flex-1 space-y-4 overflow-x-hidden overflow-y-auto">
            <OnlineUsers :owner-id="roomStore.currentRoom?.ownerId" />

            <h2 class="text-lg font-semibold">
                Ваши комнаты
            </h2>

            <CreateRoomDialog />

            <div v-if="isLoadingRooms" class="flex items-center gap-2 text-sm text-muted-foreground">
                <Loader2 class="h-4 w-4 animate-spin" />
                Загрузка...
            </div>

            <p v-else-if="roomStore.rooms.length === 0" class="text-sm text-muted-foreground">
                Пока нет комнат — создайте первую.
            </p>

            <div
                v-for="room in roomStore.rooms"
                :key="room.id"
                class="group flex items-center rounded transition-colors hover:bg-accent hover:text-accent-foreground"
                :class="{ 'bg-accent': route.params.id === room.id }"
            >
                <button
                    type="button"
                    class="min-w-0 flex-1 truncate p-2 text-left"
                    @click="openRoom(room.id)"
                >
                    {{ room.name }}
                </button>

                <ShareRoomButton
                    :room-id="room.id"
                    class="mr-1 opacity-0 transition-opacity group-hover:opacity-100"
                />
            </div>
        </div>

        <Button variant="ghost" class="w-full justify-start gap-2 text-muted-foreground" @click="handleLogout">
            <LogOut class="h-4 w-4" />
            Выйти
        </Button>
    </div>
</template>
