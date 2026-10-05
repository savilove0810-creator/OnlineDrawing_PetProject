<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from "vue"
import { useStrokeSync } from "../model/useStrokeSync"
import type { Point } from "../types"

const props = defineProps<{ roomId: string }>()

const canvasRef = ref<HTMLCanvasElement | null>(null)
const containerRef = ref<HTMLDivElement | null>(null)

let ctx: CanvasRenderingContext2D | null = null
let isDrawing = false
let lastX = 0
let lastY = 0
let currentStrokeId: string | null = null
let resizeObserver: ResizeObserver | null = null
let hasSizedOnce = false
let strokeSync: ReturnType<typeof useStrokeSync> | null = null

function applyBrushSettings() {
    if (!ctx) return
    ctx.lineCap = "round"
    ctx.lineJoin = "round"
    ctx.lineWidth = 4
    ctx.strokeStyle = "#000000"
}

function drawSegment(from: Point, to: Point) {
    if (!ctx) return
    ctx.beginPath()
    ctx.moveTo(from.x, from.y)
    ctx.lineTo(to.x, to.y)
    ctx.stroke()
}

function resize(width: number, height: number) {
    const canvas = canvasRef.value
    if (!canvas || !ctx || width === 0 || height === 0) return

    const snapshot = hasSizedOnce
        ? ctx.getImageData(0, 0, canvas.width, canvas.height)
        : null

    canvas.width = width
    canvas.height = height
    ctx.fillStyle = "#ffffff"
    ctx.fillRect(0, 0, canvas.width, canvas.height)
    applyBrushSettings()

    if (snapshot) {
        ctx.putImageData(snapshot, 0, 0)
    }

    hasSizedOnce = true
}

function getPoint(event: PointerEvent) {
    const canvas = canvasRef.value!
    const rect = canvas.getBoundingClientRect()
    return {
        x: event.clientX - rect.left,
        y: event.clientY - rect.top
    }
}

function onPointerDown(event: PointerEvent) {
    const canvas = canvasRef.value
    if (!canvas) return

    canvas.setPointerCapture(event.pointerId)

    const point = getPoint(event)
    isDrawing = true
    lastX = point.x
    lastY = point.y

    currentStrokeId = crypto.randomUUID()
    strokeSync?.startStroke(currentStrokeId, point.x, point.y, "#000000", 4)
}

function onPointerMove(event: PointerEvent) {
    if (!isDrawing || !ctx || !currentStrokeId) return

    const point = getPoint(event)
    drawSegment({ x: lastX, y: lastY }, point)
    strokeSync?.addPoint(currentStrokeId, point.x, point.y)

    lastX = point.x
    lastY = point.y
}

function onPointerUp(event: PointerEvent) {
    const canvas = canvasRef.value
    if (canvas?.hasPointerCapture(event.pointerId)) {
        canvas.releasePointerCapture(event.pointerId)
    }
    isDrawing = false

    if (currentStrokeId) {
        strokeSync?.endStroke(currentStrokeId)
        currentStrokeId = null
    }
}

onMounted(() => {
    const canvas = canvasRef.value
    const container = containerRef.value
    if (!canvas || !container) return

    ctx = canvas.getContext("2d")

    resizeObserver = new ResizeObserver((entries) => {
        const entry = entries[0]
        if (!entry) return
        const { width, height } = entry.contentRect
        resize(Math.round(width), Math.round(height))
    })
    resizeObserver.observe(container)

    strokeSync = useStrokeSync(props.roomId, drawSegment)
})

onBeforeUnmount(() => {
    resizeObserver?.disconnect()
    strokeSync?.dispose()
})
</script>

<template>
    <div
        ref="containerRef"
        class="h-full min-h-0 w-full min-w-0 overflow-hidden rounded-lg border bg-card shadow-sm"
    >
        <canvas
            ref="canvasRef"
            class="block h-full w-full touch-none cursor-crosshair"
            @pointerdown="onPointerDown"
            @pointermove="onPointerMove"
            @pointerup="onPointerUp"
            @pointerleave="onPointerUp"
        />
    </div>
</template>
