import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

import { locationsApi, locationsQueryOptions } from "@/entities/locations/api";
import { isEnvelopeError } from "@/shared/api/errors";

export function useRestoreLocation() {
    const queryClient = useQueryClient();

    const mutation = useMutation({
        mutationFn: async (id: string) => locationsApi.restoreLocation(id),
        onSettled: () => {
            queryClient.invalidateQueries({
                queryKey: locationsQueryOptions.baseKey,
            });
        },
        onSuccess: () => {
            toast.success("Локация восстановлена");
        },
        onError: () => {
            toast.error("Ошибка при восстановлении локации");
        },
    });

    return {
        restoreLocation: mutation.mutate,
        isError: mutation.isError,
        error: isEnvelopeError(mutation.error) ? mutation.error : undefined,
        isPending: mutation.isPending,
    };
}
