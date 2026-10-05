import { roomPath } from "@/shared/config/routes"

export function buildRoomLink(roomId: string): string {
    return `${window.location.origin}${roomPath(roomId)}`
}
