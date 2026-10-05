<script setup lang="ts">
import { LayoutDashboard } from "lucide-vue-next";
import { AppSidebar } from "@/widgets/AppSidebar";
</script>

<template>
    <div class="flex h-screen bg-background">
        <AppSidebar />

        <main class="flex flex-1 flex-col items-center justify-center gap-3 text-center">
            <div class="flex h-12 w-12 items-center justify-center rounded-full border bg-muted">
                <LayoutDashboard class="h-5 w-5 text-muted-foreground" />
            </div>

            <div class="space-y-1">
                <h1 class="text-lg font-semibold">Выберите комнату</h1>
                <p class="text-sm text-muted-foreground">
                    Выберите комнату в списке слева или создайте новую, чтобы начать рисовать.
                </p>
            </div>
        </main>
    </div>
</template>