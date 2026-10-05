import { defineStore } from "pinia"
import { computed, ref } from "vue"

import { login as loginRequest, register as registerRequest } from "../api/authApi"
import { getUserIdFromJwt } from "../lib/jwt"
import {
    clearStoredToken,
    isTokenExpired,
    loadStoredToken,
    loadStoredUserId,
    loadStoredUsername,
    storeToken,
} from "@/shared/auth/tokenStorage"

export const useAuthStore = defineStore("auth", () => {
    const token = ref<string | null>(loadStoredToken())
    const userId = ref<string | null>(token.value ? loadStoredUserId() : null)
    const username = ref<string | null>(token.value ? loadStoredUsername() : null)

    const isAuthenticated = computed(() => !!token.value && !isTokenExpired(token.value))

    async function login(usernameInput: string, password: string): Promise<void> {
        const jwt = await loginRequest(usernameInput, password)
        const userIdFromToken = getUserIdFromJwt(jwt)
        if (!userIdFromToken) {
            throw new Error("Некорректный токен авторизации")
        }

        token.value = jwt
        userId.value = userIdFromToken
        username.value = usernameInput
        storeToken(jwt, usernameInput, userIdFromToken)
    }

    async function register(usernameInput: string, email: string, password: string): Promise<void> {
        await registerRequest(usernameInput, email, password)
    }

    function logout(): void {
        token.value = null
        username.value = null
        userId.value = null
        clearStoredToken()
    }

    function validateSession(): void {
        if (token.value && isTokenExpired(token.value)) {
            logout()
        }
    }

    return { token, username, userId, isAuthenticated, login, register, logout, validateSession }
})
