<script setup lang="ts">
import { useRoute, useRouter } from "vue-router"
import { Button } from "@/components/ui/button"
import { ROUTES } from "@/shared/config/routes"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"

const route = useRoute()
const router = useRouter()

const message = (route.query.message as string) || "Не удалось подключиться к доске."
</script>

<template>
    <main class="flex min-h-screen items-center justify-center bg-background">
        <Card class="w-full max-w-sm">
            <CardHeader>
                <CardTitle class="text-xl">Не удалось подключиться</CardTitle>
            </CardHeader>

            <CardContent class="space-y-4">
                <p class="text-sm text-destructive">{{ message }}</p>
                <Button class="w-full" @click="router.push(ROUTES.rooms)">Вернуться к комнатам</Button>
            </CardContent>
        </Card>
    </main>
</template>
