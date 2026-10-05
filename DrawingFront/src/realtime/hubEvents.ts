import { connection } from "./connection";
import type { HubConnectedUser } from "./types";

function subscribe<Args extends unknown[]>(event: string, handler: (...args: Args) => void): () => void {
    const listener = (...args: unknown[]) => handler(...(args as Args));
    connection.on(event, listener);
    return () => connection.off(event, listener);
}

export function onUsersUpdated(handler: (roomId: string, users: HubConnectedUser[]) => void) {
    return subscribe("UsersUpdated", handler);
}

export function onKicked(handler: (message: string) => void) {
    const unsubscribers = [
        subscribe("RoomDeleted", handler),
        subscribe("ForceDisconnected", handler),
    ];
    return () => unsubscribers.forEach(unsubscribe => unsubscribe());
}

export function onStrokeStarted(handler: (strokeId: string, x: number, y: number, color: string, width: number) => void) {
    return subscribe("StrokeStarted", handler);
}

export function onPointAdded(handler: (strokeId: string, x: number, y: number) => void) {
    return subscribe("PointAdded", handler);
}
