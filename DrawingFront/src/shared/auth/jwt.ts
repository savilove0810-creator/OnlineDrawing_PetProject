export function parseJwtPayload(jwt: string): Record<string, unknown> | null {
    try {
        return JSON.parse(atob(jwt.split(".")[1]))
    } catch {
        return null
    }
}
