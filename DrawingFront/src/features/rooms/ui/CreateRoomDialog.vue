<script setup lang="ts">
import { ref } from "vue"
import { Plus, Loader2 } from "lucide-vue-next"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"

import { useRouter } from "vue-router"
import { roomPath } from "@/shared/config/routes";

import { useRoomStore } from "../model/roomStore";

const router = useRouter();
const roomStore = useRoomStore();

const open = ref(false);
const roomName = ref("");
const error = ref("");
const isCreating = ref(false);

async function handleCreateRoom() {
  if (!roomName.value.trim()) {
    error.value = "Введите название комнаты.";
    return;
  }

  isCreating.value = true;
  try {
    const room = await roomStore.createNewRoom(roomName.value.trim());
    router.push(roomPath(room.id));
    open.value = false;
    roomName.value = "";
    error.value = "";
  } catch (err) {
    error.value = err instanceof Error ? err.message : "Не удалось создать комнату. Попробуйте ещё раз.";
  } finally {
    isCreating.value = false;
  }
}
</script>

<template>
  <Dialog v-model:open="open">
    <DialogTrigger as-child>
      <Button class="w-full gap-2">
        <Plus class="h-4 w-4" />
        Создать комнату
      </Button>
    </DialogTrigger>

    <DialogContent class="sm:max-w-md">
      <DialogHeader>
        <DialogTitle>Новая комната</DialogTitle>
        <DialogDescription>
          Дайте название доске для рисования.
        </DialogDescription>
      </DialogHeader>

      <div class="flex flex-col gap-3 py-4">
        <Label for="room-name">Название комнаты</Label>
        <Input
          id="room-name"
          v-model="roomName"
          placeholder="Например, «Мозговой штурм»"
          :disabled="isCreating"
          @input="error = ''"
          @keyup.enter="handleCreateRoom"
        />
        <p v-if="error" class="text-sm text-destructive">{{ error }}</p>
      </div>

      <DialogFooter>
        <Button variant="outline" :disabled="isCreating" @click="open = false">
          Отмена
        </Button>
        <Button :disabled="isCreating" class="gap-2" @click="handleCreateRoom">
          <Loader2 v-if="isCreating" class="h-4 w-4 animate-spin" />
          Создать
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>
