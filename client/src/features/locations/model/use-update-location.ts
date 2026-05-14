import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

import { locationsApi, locationsQueryOptions } from "@/entities/locations/api";
import type { UpdateLocationRequest } from "@/entities/locations/api";
import { isEnvelopeError } from "@/shared/api/errors";

export type UpdateLocationFormData = {
    id: string;
    name: string;
    country: string;
    city: string;
    street: string;
    buildingNumber: string;
    timezone: string;
    isActive?: boolean;
};

export function useUpdateLocation() {
    const queryClient = useQueryClient();

    const mutation = useMutation({
        mutationFn: async (formData: UpdateLocationFormData) => {
            const request: UpdateLocationRequest = {
                name: formData.name.trim(),
                addressDto: {
                    country: formData.country.trim(),
                    city: formData.city.trim(),
                    street: formData.street.trim(),
                    buildingNumber: formData.buildingNumber.trim(),
                },
                timezone: formData.timezone.trim(),
                isActive: formData.isActive ?? true,
            };

            return locationsApi.updateLocation(formData.id, request);
        },
        onSettled: () => {
            queryClient.invalidateQueries({
                queryKey: locationsQueryOptions.baseKey,
            });
        },
        onSuccess: () => {
            toast.success("Локация успешно обновлена");
        },
        onError: () => {
            toast.error("Ошибка при обновлении локации");
        },
    });

    return {
        updateLocation: mutation.mutate,
        isError: mutation.isError,
        error: isEnvelopeError(mutation.error) ? mutation.error : undefined,
        isPending: mutation.isPending,
    };
}

