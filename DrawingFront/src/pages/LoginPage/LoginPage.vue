<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/features/auth'
import { ROUTES } from '@/shared/config/routes'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Field, FieldGroup, FieldLabel, FieldError } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
import { useRouter } from 'vue-router'
import { Loader2 } from 'lucide-vue-next'

const router = useRouter()

const authStore = useAuthStore()

const username = ref('')
const password = ref('')
const error = ref('')
const invalidUsername = ref(false)
const invalidPassword = ref(false)
const isSubmitting = ref(false)

async function handleLogin() {
  error.value = ''
  invalidUsername.value = !username.value
  invalidPassword.value = !password.value

  if (invalidUsername.value || invalidPassword.value) {
    error.value = 'Пожалуйста, заполните все поля.'
    return
  }

  isSubmitting.value = true
  try {
    await authStore.login(username.value, password.value)
    router.push(ROUTES.rooms)
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Неизвестная ошибка'
    invalidUsername.value = true
    invalidPassword.value = true
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <main class="flex min-h-screen items-center justify-center bg-background">
  <Card class="w-full max-w-sm">
    <CardHeader>
      <CardTitle class="text-xl">Вход</CardTitle>
    </CardHeader>

    <CardContent>
      <form id="login-form" novalidate @submit.prevent="handleLogin">
        <FieldGroup>
          <Field :data-invalid="invalidUsername || undefined">
            <FieldLabel for="login-username">Имя пользователя</FieldLabel>
            <Input id="login-username" v-model="username" type="text" placeholder="username" autocomplete="username" :aria-invalid="invalidUsername" />
          </Field>

          <Field :data-invalid="invalidPassword || undefined">
            <FieldLabel for="login-password">Пароль</FieldLabel>
            <Input id="login-password" v-model="password" type="password" placeholder="••••••••" autocomplete="current-password" :aria-invalid="invalidPassword" />
            <FieldError v-if="error">{{ error }}</FieldError>
          </Field>
        </FieldGroup>
      </form>

      <p class="mt-4 text-center text-sm text-muted-foreground">
        Нет аккаунта?
        <Button variant="link" class="h-auto p-0" @click="router.push(ROUTES.register)">
          Зарегистрироваться
        </Button>
      </p>
    </CardContent>

    <CardFooter>
      <Button class="w-full gap-2" type="submit" form="login-form" :disabled="isSubmitting">
        <Loader2 v-if="isSubmitting" class="h-4 w-4 animate-spin" />
        {{ isSubmitting ? 'Входим...' : 'Войти' }}
      </Button>
    </CardFooter>
  </Card>
  </main>
</template>
