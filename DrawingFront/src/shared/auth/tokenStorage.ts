import { parseJwtPayload } from "./jwt"

const TOKEN_KEY = 'auth_token'
const USERNAME_KEY = 'auth_username'
const USER_ID_KEY = 'auth_user_id'

export function isTokenExpired(jwt: string): boolean {
    const payload = parseJwtPayload(jwt)
    if (!payload) return true
    return typeof payload.exp === 'number' && payload.exp * 1000 < Date.now()
}

export function loadStoredToken(): string | null {
    const jwt = localStorage.getItem(TOKEN_KEY)
    if (!jwt || isTokenExpired(jwt)) {
        clearStoredToken()
        return null
    }
    return jwt
}

export function loadStoredUsername(): string | null {
    return localStorage.getItem(USERNAME_KEY)
}

export function loadStoredUserId(): string | null {
    return localStorage.getItem(USER_ID_KEY)
}

export function storeToken(jwt: string, username: string, userId: string): void {
    localStorage.setItem(TOKEN_KEY, jwt)
    localStorage.setItem(USERNAME_KEY, username)
    localStorage.setItem(USER_ID_KEY, userId)
}

export function clearStoredToken(): void {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USERNAME_KEY)
    localStorage.removeItem(USER_ID_KEY)
}
