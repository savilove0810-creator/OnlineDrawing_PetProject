import { parseJwtPayload } from "@/shared/auth/jwt"

const USER_ID_CLAIM = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"

export function getUserIdFromJwt(jwt: string): string | null {
    const value = parseJwtPayload(jwt)?.[USER_ID_CLAIM]
    return typeof value === "string" ? value : null
}
