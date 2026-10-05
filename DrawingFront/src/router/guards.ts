import type { Router } from "vue-router"

import { ROUTES } from "@/shared/config/routes"
import { useAuthStore } from "@/features/auth"

export function installAuthGuard(router: Router) {
    router.beforeEach((to) => {
        const authStore = useAuthStore()
        authStore.validateSession()

        if (to.meta.requiresAuth && !authStore.isAuthenticated) {
            return ROUTES.login
        }

        if (authStore.isAuthenticated && (to.path === ROUTES.login || to.path === ROUTES.register)) {
            return ROUTES.rooms
        }
    })
}
