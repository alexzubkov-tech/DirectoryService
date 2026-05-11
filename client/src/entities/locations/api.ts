import { apiClient } from "@/shared/api/axios-instance";
import type { Location } from "@/entities/locations/types";
import type { PaginationResponse } from "@/shared/api/types";
import type { Envelope } from "@/shared/api/envelope";

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
    ): Promise<Location> => {
        const response = await apiClient.get<
            Envelope<PaginationResponse<Location>>
        >("/locations", {
            params: {
                page: 1,
                pageSize: 100,
                search: "",
                isActive: true,
                departmentIds: [],
            },
        });

        const location = response.data.result?.items.find(
            (item) => item.id === request.id,
        );

        if (!location) {
            throw new Error(`Локация с ID ${request.id} не найдена`);
        }

        return location;
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
};
