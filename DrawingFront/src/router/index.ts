import { createRouter, createWebHistory } from "vue-router"

import { installAuthGuard } from "./guards"
import { routes } from "./routes"

const router = createRouter({
    history: createWebHistory(),
    routes,
})

installAuthGuard(router)

export default router
