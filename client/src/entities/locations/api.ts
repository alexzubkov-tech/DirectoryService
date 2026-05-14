import { queryOptions } from "@tanstack/react-query";

import type { Location } from "@/entities/locations/types";
import { apiClient } from "@/shared/api/axios-instance";
import type { Envelope } from "@/shared/api/envelope";
import type { PaginationResponse } from "@/shared/api/types";

export type CreateLocationRequest = {
    name: string;
    addressDto: {
        country: string;
        city: string;
        street: string;
        buildingNumber: string;
    };
    timezone: string;
};

export type UpdateLocationRequest = CreateLocationRequest & {
    isActive: boolean;
};

export type GetLocationsRequest = {
    departmentIds?: string[];
    search?: string;
    isActive?: boolean;
    page: number;
    pageSize: number;
};

export type GetLocationByIdRequest = {
    id: string;
};

export type GetLocationsQueryParams = {
    page: number;
    search?: string;
    isActive?: boolean;
    departmentIds: string[];
};

const PAGE_SIZE = 2;
const MAX_SEARCH_LENGTH = 100;

export const locationsQueryOptions = {
    baseKey: ["locations"] as const,

    getLocationByIdOptions: ({ id }: GetLocationByIdRequest) =>
        queryOptions({
            queryKey: [...locationsQueryOptions.baseKey, "detail", id],
            queryFn: ({ signal }) => locationsApi.getLocationById({ id }, signal),
            enabled: Boolean(id),
        }),

    getLocationsOptions: ({
        page,
        search,
        isActive,
        departmentIds,
    }: GetLocationsQueryParams) => {
        const trimmedSearch = search
            ? search.trim().slice(0, MAX_SEARCH_LENGTH)
            : undefined;

        return queryOptions({
            queryKey: [
                ...locationsQueryOptions.baseKey,
                {
                    page,
                    pageSize: PAGE_SIZE,
                    search: trimmedSearch,
                    isActive,
                    departmentIds,
                },
            ],
            queryFn: ({ signal }) => {
                const request: GetLocationsRequest = {
                    departmentIds,
                    page,
                    pageSize: PAGE_SIZE,
                };
                if (trimmedSearch) request.search = trimmedSearch;
                if (isActive !== undefined) request.isActive = isActive;
                return locationsApi.getLocations(request, signal);
            },
            placeholderData: (previousData) => previousData,
        });
    },
};

export const locationsApi = {
    getLocations: async (
        request: GetLocationsRequest,
        signal?: AbortSignal,
    ): Promise<PaginationResponse<Location>> => {
        const response = await apiClient.get<
            Envelope<PaginationResponse<Location>>
        >("/locations", {
            params: request,
            signal,
        });

        return (
            response.data.result ?? {
                items: [],
                totalCount: 0,
                page: request.page,
                pageSize: request.pageSize,
                totalPages: 1,
            }
        );
    },

    getLocationById: async (
        request: GetLocationByIdRequest,
        signal?: AbortSignal,
    ): Promise<Location> => {
        const response = await apiClient.get<Envelope<Location>>(
            `/locations/${request.id}`,
            { signal },
        );

        if (!response.data.result) {
            throw new Error(`Локация с ID ${request.id} не найдена`);
        }

        return response.data.result;
    },

    createLocation: async (
        request: CreateLocationRequest,
    ): Promise<Location> => {
        const response = await apiClient.post<Envelope<Location>>(
            "/locations",
            request,
        );

        if (!response.data.result) {
            throw new Error("Не удалось создать локацию: пустой ответ");
        }

        return response.data.result;
    },

    updateLocation: async (
        id: string,
        request: UpdateLocationRequest,
    ): Promise<string> => {
        const response = await apiClient.put<Envelope<string>>(
            `/locations/${id}`,
            request,
        );

        if (!response.data.result) {
            throw new Error("Не удалось обновить локацию: пустой ответ");
        }

        return response.data.result;
    },

    deleteLocation: async (id: string): Promise<void> => {
        await apiClient.delete<Envelope<null>>(`/locations/${id}`);
    },

    restoreLocation: async (id: string): Promise<void> => {
        await apiClient.post<Envelope<null>>(`/locations/${id}/restore`);
    },
};
