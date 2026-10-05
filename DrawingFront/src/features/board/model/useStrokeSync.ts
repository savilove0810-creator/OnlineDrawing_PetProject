import * as hub from "@/realtime"

import type { Point } from "../types"
import { useStrokeStore } from "./strokeStore"

export function useStrokeSync(roomId: string, onRemoteSegment: (from: Point, to: Point) => void) {
    const strokeStore = useStrokeStore()

    const unsubscribers = [
        hub.onStrokeStarted((strokeId, x, y, color, width) => {
            strokeStore.startStroke({ id: strokeId, color, width, points: [{ x, y }] })
        }),
        hub.onPointAdded((strokeId, x, y) => {
            const prevPoint = strokeStore.strokes.find(s => s.id === strokeId)?.points.at(-1)
            strokeStore.addPointToStroke(strokeId, x, y)
            if (prevPoint) onRemoteSegment(prevPoint, { x, y })
        }),
    ]

    function startStroke(strokeId: string, x: number, y: number, color: string, width: number) {
        hub.startStroke(roomId, strokeId, x, y, color, width)
    }

    function addPoint(strokeId: string, x: number, y: number) {
        hub.addPoint(roomId, strokeId, x, y)
    }

    function endStroke(strokeId: string) {
        hub.endStroke(roomId, strokeId)
    }

    function dispose() {
        unsubscribers.forEach(unsubscribe => unsubscribe())
        strokeStore.clearStrokes()
    }

    return { startStroke, addPoint, endStroke, dispose }
}
