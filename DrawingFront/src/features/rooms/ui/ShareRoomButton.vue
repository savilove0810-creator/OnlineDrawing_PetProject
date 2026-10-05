<script setup lang="ts">
import { Share2 } from "lucide-vue-next"
import { toast } from "vue-sonner"

import { Button } from "@/components/ui/button"

import { buildRoomLink } from "../lib/roomLink"

const props = defineProps<{ roomId: string }>()

async function share() {
    await navigator.clipboard.writeText(buildRoomLink(props.roomId))
    toast.success("Ссылка на комнату скопирована")
}
</script>

<template>
    <Button variant="ghost" size="icon-sm" @click.stop="share">
        <Share2 class="h-4 w-4" />
    </Button>
</template>
