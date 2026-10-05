<script setup lang="ts">
import { Badge } from "@/components/ui/badge";

import { usePresenceStore } from "../model/presenceStore";
import { getUserColor, getInitials } from "../lib/avatarColor";

defineProps<{ ownerId?: string }>();

const presenceStore = usePresenceStore();
</script>

<template>
    <div class="space-y-2">
        <div class="flex items-center justify-between">
            <h3 class="text-xs font-medium uppercase tracking-wide text-muted-foreground">
                Участники
            </h3>
            <Badge v-if="presenceStore.users.length" variant="secondary">
                {{ presenceStore.users.length }}
            </Badge>
        </div>

        <ul v-if="presenceStore.users.length" class="space-y-1">
            <li
                v-for="user in presenceStore.users"
                :key="user.userId"
                class="flex items-center gap-2.5 rounded-md px-1 py-1 -mx-1 transition-colors hover:bg-accent"
            >
                <span class="relative flex h-7 w-7 shrink-0 items-center justify-center rounded-full text-[11px] font-medium text-white"
                    :style="{ backgroundColor: getUserColor(user.userId) }"
                >
                    {{ getInitials(user.username) }}
                    <span class="absolute -bottom-0.5 -right-0.5 h-2.5 w-2.5 rounded-full border-2 border-card bg-emerald-500" />
                </span>

                <span class="min-w-0 flex-1 truncate text-sm">{{ user.username }}</span>

                <Badge v-if="user.userId === ownerId" variant="secondary" class="shrink-0 text-[10px]">
                    Владелец
                </Badge>
            </li>
        </ul>

        <p v-else class="text-sm text-muted-foreground">
            Вы ещё не вошли в комнату.
        </p>
    </div>
</template>
