export const ROUTES = {
    login: "/login",
    register: "/register",
    rooms: "/app",
    connectionError: "/connection-error",
} as const

export function roomPath(roomId: string): string {
    return `/app/room/${roomId}`
}
