<script setup lang="ts">
import { computed } from "vue";
import { useRouter } from "vue-router";
import { toast } from "vue-sonner";

import { Button } from "@/components/ui/button";
import { ROUTES } from "@/shared/config/routes";
import { useAuthStore } from "@/features/auth";

import { useRoomStore } from "../model/roomStore";
import ShareRoomButton from "./ShareRoomButton.vue";

const router = useRouter();
const roomStore = useRoomStore();
const authStore = useAuthStore();

const isOwner = computed(() => roomStore.currentRoom?.ownerId === authStore.userId);

async function endSession(id: string) {
    if (!window.confirm("Завершить сеанс и удалить комнату для всех?")) {
        return;
    }
    try {
        await roomStore.deleteRoomById(id);
    } catch (err) {
        toast.error(err instanceof Error ? err.message : "Не удалось удалить комнату");
    }
}
</script>

<template>
    <div
        v-if="roomStore.currentRoom"
        class="flex items-center gap-2 border-t bg-card p-2"
    >
        <span class="min-w-0 flex-1 truncate text-sm">{{ roomStore.currentRoom.name }}</span>

        <ShareRoomButton :room-id="roomStore.currentRoom.id" />

        <Button variant="ghost" size="sm" @click="router.push(ROUTES.rooms)">
            Выйти
        </Button>

        <Button
            v-if="isOwner"
            variant="destructive"
            size="sm"
            @click="endSession(roomStore.currentRoom.id)"
        >
            Завершить сеанс
        </Button>
    </div>
</template>
