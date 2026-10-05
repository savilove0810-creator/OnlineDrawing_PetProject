import { defineStore } from "pinia";
import { ref } from "vue";

import type { ConnectedUser } from "../types";

export const usePresenceStore = defineStore("presence", () => {
    const users = ref<ConnectedUser[]>([]);
    const currentRoomId = ref<string | null>(null);
    const roomDeletedMessage = ref<string | null>(null);
    function setCurrentRoom(roomId: string | null) {
        currentRoomId.value = roomId;
        users.value = [];
    }

    function setUsers(roomId: string, updatedUsers: ConnectedUser[]) {
        if (roomId !== currentRoomId.value) {
            return;
        }
        users.value = updatedUsers;
    }

    function clearUsers() {
        users.value = [];
        currentRoomId.value = null;
    }

    function setRoomDeletedMessage(message: string | null) {
        roomDeletedMessage.value = message;
    }

    return {
        users,
        currentRoomId,
        roomDeletedMessage,
        setRoomDeletedMessage,
        setCurrentRoom,
        setUsers,
        clearUsers
    };
});