<script setup lang="ts">
import { useRoute } from "vue-router";

import { AppSidebar } from "@/widgets/AppSidebar";
import { BoardCanvas } from "@/features/board";
import { RoomControlBar } from "@/features/rooms";

import { useRoomSession } from "./useRoomSession";

const route = useRoute();

useRoomSession();
</script>

<template>
    <div class="flex h-screen">
        <AppSidebar />

        <main class="flex-1 flex flex-col min-h-0">
            <div class="flex-1 min-h-0 flex justify-center items-center p-2">
                <BoardCanvas :key="route.params.id as string" :room-id="route.params.id as string" />
            </div>
            <RoomControlBar />
        </main>
    </div>
</template>
