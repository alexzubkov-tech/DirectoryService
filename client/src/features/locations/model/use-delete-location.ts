import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

import { locationsApi, locationsQueryOptions } from "@/entities/locations/api";
import { isEnvelopeError } from "@/shared/api/errors";

export function useDeleteLocation() {
    const queryClient = useQueryClient();

    const mutation = useMutation({
        mutationFn: async (id: string) => locationsApi.deleteLocation(id),
        onSettled: () => {
            queryClient.invalidateQueries({
                queryKey: locationsQueryOptions.baseKey,
            });
        },
        onSuccess: () => {
            toast.success("Локация удалена");
        },
        onError: () => {
            toast.error("Ошибка при удалении локации");
        },
    });

    return {
        deleteLocation: mutation.mutate,
        isError: mutation.isError,
        error: isEnvelopeError(mutation.error) ? mutation.error : undefined,
        isPending: mutation.isPending,
    };
}
