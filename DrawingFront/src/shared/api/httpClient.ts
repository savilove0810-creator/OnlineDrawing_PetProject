import { loadStoredToken } from "@/shared/auth/tokenStorage"

export async function apiFetch(url: string, options: RequestInit = {}): Promise<Response> {
    const token = loadStoredToken()

    const headers = new Headers(options.headers)
    if (token) {
        headers.set("Authorization", `Bearer ${token}`)
    }

    return fetch(url, { ...options, headers })
}

export async function extractErrorMessage(res: Response, fallback: string): Promise<string> {
    const text = await res.text()
    try {
        const body = JSON.parse(text)
        if (body?.errors) {
            return Object.values(body.errors as Record<string, string[]>).flat().join(' ')
        }
    } catch {
    }
    return text || fallback
}
