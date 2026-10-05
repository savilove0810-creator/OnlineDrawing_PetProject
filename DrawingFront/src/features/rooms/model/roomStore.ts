import { defineStore } from "pinia"
import { ref } from "vue"

import { getRooms, createRoom, getRoom, deleteRoom } from "../api/roomApi"
import type { Room } from "../types"

export const useRoomStore = defineStore(
    "room",
    () => {
        const rooms = ref<Room[]>([])
        const currentRoom = ref<Room | null>(null)  

        async function loadRooms(){

            rooms.value = await getRooms()

        }

        async function loadCurrentRoom(roomId: string){
            currentRoom.value = await getRoom(roomId)
        }

        function clearCurrentRoom(){
            currentRoom.value = null
        }

        async function createNewRoom(name: string){

            const newRoom = await createRoom(name)
            rooms.value.push(newRoom)

            return newRoom

        }

        async function deleteRoomById(roomId: string){
            await deleteRoom(roomId)
            rooms.value = rooms.value.filter(r => r.id !== roomId)
        }

        return {
            rooms,
            currentRoom,
            loadRooms,
            loadCurrentRoom,
            clearCurrentRoom,
            createNewRoom,
            deleteRoomById
        }
    }
)