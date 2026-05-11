import { QueryClient } from "@tanstack/react-query";

export function createQueryClient() {
    return new QueryClient({
        defaultOptions: {
            queries: {
                staleTime: 60 * 1000,
                retry: (failureCount, error) => {
                    if (error instanceof Error) {
                        if (
                            error.message.includes("таймаут") ||
                            error.message.includes("соединения") ||
                            error.message.includes("timeout")
                        ) {
                            return false;
                        }
                    }
                    return failureCount < 3;
                },
                retryDelay: (attemptIndex) =>
                    Math.min(1000 * 2 ** attemptIndex, 5000),
            },
        },
    });
}
