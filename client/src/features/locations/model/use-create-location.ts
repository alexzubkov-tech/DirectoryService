import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

import { locationsApi, locationsQueryOptions } from "@/entities/locations/api";
import type { CreateLocationRequest } from "@/entities/locations/api";
import { isEnvelopeError } from "@/shared/api/errors";

export type CreateLocationFormData = {
    name: string;
    country: string;
    city: string;
    street: string;
    buildingNumber: string;
    timezone: string;
};

export function useCreateLocation() {
    const queryClient = useQueryClient();

    const mutation = useMutation({
        mutationFn: async (formData: CreateLocationFormData) => {
            const request: CreateLocationRequest = {
                name: formData.name.trim(),
                addressDto: {
                    country: formData.country.trim(),
                    city: formData.city.trim(),
                    street: formData.street.trim(),
                    buildingNumber: formData.buildingNumber.trim(),
                },
                timezone: formData.timezone.trim(),
            };

            return locationsApi.createLocation(request);
        },
        onSettled: () => {
            queryClient.invalidateQueries({
                queryKey: locationsQueryOptions.baseKey,
            });
        },
        onSuccess: () => {
            toast.success("Локация успешно создана");
        },
        onError: () => {
            toast.error("Ошибка при создании локации");
        },
    });

    return {
        createLocation: mutation.mutate,
        isError: mutation.isError,
        error: isEnvelopeError(mutation.error) ? mutation.error : undefined,
        isPending: mutation.isPending,
    };
}
