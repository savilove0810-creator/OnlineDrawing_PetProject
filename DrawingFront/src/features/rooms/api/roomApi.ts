import { apiFetch, extractErrorMessage } from "@/shared/api/httpClient"
import { env } from "@/shared/config/env"

import type { Room } from "../types"

export async function getRoom(roomId: string): Promise<Room> {
    const response = await apiFetch(`${env.apiUrl}/api/rooms/${roomId}`)

    if (!response.ok) {
        throw new Error(await extractErrorMessage(response, "Ошибка загрузки комнаты"))
    }

    return await response.json()
}

export async function getRooms(): Promise<Room[]> {
    const response = await apiFetch(`${env.apiUrl}/api/rooms`)

    if (!response.ok) {
        throw new Error(await extractErrorMessage(response, "Ошибка загрузки комнат"))
    }

    return await response.json()
}

export async function deleteRoom(roomId: string): Promise<void> {
    const response = await apiFetch(`${env.apiUrl}/api/rooms/${roomId}`, {
        method: "DELETE"
    })

    if (!response.ok) {
        throw new Error(await extractErrorMessage(response, "Ошибка удаления комнаты"))
    }
}

export async function createRoom(name: string): Promise<Room> {
    const response = await apiFetch(
        `${env.apiUrl}/api/rooms`,
        {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ name })
        }
    )

    if (!response.ok) {
        throw new Error(await extractErrorMessage(response, "Ошибка создания комнаты"))
    }

    return await response.json()
}
