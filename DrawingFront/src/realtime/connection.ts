import * as signalR from "@microsoft/signalr";

import { env } from "@/shared/config/env";
import { loadStoredToken } from "@/shared/auth/tokenStorage";

export const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${env.apiUrl}/boardhub`, {
        accessTokenFactory: () => loadStoredToken() ?? ""
    })
    .build();

if (import.meta.hot) {
    import.meta.hot.dispose(() => {
        connection.stop();
    });
}
