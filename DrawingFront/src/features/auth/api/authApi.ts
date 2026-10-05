import { env } from "@/shared/config/env"
import { extractErrorMessage } from "@/shared/api/httpClient"

export async function login(username: string, password: string): Promise<string> {
    const res = await fetch(`${env.apiUrl}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password }),
    })

    if (!res.ok) {
        throw new Error(await extractErrorMessage(res, 'Неверный логин или пароль'))
    }

    return await res.text()
}

export async function register(username: string, email: string, password: string): Promise<void> {
    const res = await fetch(`${env.apiUrl}/api/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, email, password }),
    })

    if (!res.ok) {
        throw new Error(await extractErrorMessage(res, 'Ошибка регистрации'))
    }
}
