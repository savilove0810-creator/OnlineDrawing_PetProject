import type { RouteRecordRaw } from "vue-router"

import { ROUTES } from "@/shared/config/routes"

export const routes: RouteRecordRaw[] = [
    {
        path: "/",
        redirect: ROUTES.login,
    },
    {
        path: ROUTES.login,
        name: "login",
        component: () => import("@/pages/LoginPage/LoginPage.vue"),
    },
    {
        path: ROUTES.register,
        name: "register",
        component: () => import("@/pages/RegisterPage/RegisterPage.vue"),
    },
    {
        path: ROUTES.rooms,
        name: "rooms",
        component: () => import("@/pages/RoomListPage/RoomListPage.vue"),
        meta: { requiresAuth: true },
    },
    {
        path: "/app/room/:id",
        name: "room",
        component: () => import("@/pages/RoomPage/RoomPage.vue"),
        meta: { requiresAuth: true },
    },
    {
        path: ROUTES.connectionError,
        name: "connection-error",
        component: () => import("@/pages/ConnectionErrorPage/ConnectionErrorPage.vue"),
        meta: { requiresAuth: true },
    },
    {
        path: "/:pathMatch(.*)*",
        redirect: ROUTES.rooms,
    },
]
