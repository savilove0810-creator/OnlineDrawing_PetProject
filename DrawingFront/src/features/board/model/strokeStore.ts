import {  defineStore } from "pinia";
import { ref } from "vue";

import type { Stroke } from "../types";

export const useStrokeStore = defineStore("stroke", () => {
    const strokes = ref<Stroke[]>([]);

    function startStroke(stroke: Stroke) {
        strokes.value.push(stroke);
    }

    function addPointToStroke(strokeId: string, x: number, y: number) {
        const stroke = strokes.value.find(s => s.id === strokeId);
        if (stroke) {
            stroke.points.push({ x, y });
        }
    }

    function clearStrokes() {
        strokes.value = [];
    }

    return {
        strokes,
        startStroke,
        addPointToStroke,
        clearStrokes
    };
});