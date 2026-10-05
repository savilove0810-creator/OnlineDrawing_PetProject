import { connection } from "./connection";

const HubMethod = {
    JoinRoom: "JoinRoom",
    LeaveRoom: "LeaveRoom",
    StartStroke: "StartStroke",
    AddPoint: "AddPoint",
    EndStroke: "EndStroke",
} as const;

let connectPromise: Promise<void> | null = null;
let disconnectPromise: Promise<void> | null = null;
let activeRoomId: string | null = null;

export async function connect(): Promise<void> {
    if (connection.state === "Connected") {
        return;
    }

    if (!connectPromise) {
        connectPromise = connection.start()
            .finally(() => {
                connectPromise = null;
            });
    }

    await connectPromise;
}

export async function disconnect(): Promise<void> {
    if (connection.state === "Disconnected") {
        return;
    }

    if (!disconnectPromise) {
        disconnectPromise = connection.stop()
            .finally(() => {
                disconnectPromise = null;
            });
    }

    await disconnectPromise;
    activeRoomId = null;
}

export async function switchRoom(newRoomId: string): Promise<void> {
    if (connection.state !== "Connected") return;
    if (activeRoomId === newRoomId) return;

    if (activeRoomId) {
        await connection.invoke(HubMethod.LeaveRoom, activeRoomId);
        activeRoomId = null;
    }

    await connection.invoke(HubMethod.JoinRoom, newRoomId);
    activeRoomId = newRoomId;
}

export async function leaveCurrentRoom(): Promise<void> {
    if (connection.state !== "Connected") return;
    if (!activeRoomId) return;

    const roomId = activeRoomId;
    await connection.invoke(HubMethod.LeaveRoom, roomId);
    activeRoomId = null;
}

function fireAndForget(method: string, ...args: unknown[]): void {
    connection.invoke(method, ...args).catch((err) => console.error(`Hub call ${method} failed`, err));
}

export function startStroke(roomId: string, strokeId: string, x: number, y: number, color: string, width: number): void {
    fireAndForget(HubMethod.StartStroke, roomId, strokeId, x, y, color, width);
}

export function addPoint(roomId: string, strokeId: string, x: number, y: number): void {
    fireAndForget(HubMethod.AddPoint, roomId, strokeId, x, y);
}

export function endStroke(roomId: string, strokeId: string): void {
    fireAndForget(HubMethod.EndStroke, roomId, strokeId);
}
