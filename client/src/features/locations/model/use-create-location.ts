import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { locationsApi } from "@/entities/locations/api";
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
    const router = useRouter();

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
        onSuccess: () => {
            // Инвалидируем кэш списка локаций чтобы он обновился
            queryClient.invalidateQueries({ queryKey: ["locations"] });
            router.push("/locations");
        },
    });

    return {
        mutate: mutation.mutate,
        mutateAsync: mutation.mutateAsync,
        isPending: mutation.isPending,
        isError: mutation.isError,
        error: isEnvelopeError(mutation.error) ? mutation.error : undefined,
        isSuccess: mutation.isSuccess,
    };
}
