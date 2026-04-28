import { apiClient } from "@/shared/api/axios-instance";
import type { Location } from "@/entities/locations/types";

export type CreateLocationRequest = {
    name: string;
};

export type GetLocationsRequest = {
    departmentIds?: string[];
    search?: string;
    isActive: boolean;
    paginationRequest: PaginationRequest;
};

export type PaginationRequest = {
    page: number;
    pageSize: number;
};

export type Envelope<T = unknown> = {
    result: T | null;
    errorList: ApiError[] | null;
    isError: boolean;
    timeGenerated: string;
};

export type ApiError = {
    messages: ErrorMessage[];
    statusCode: ErrorType;
};

export type ErrorMessage = {
    code: string;
    message: string;
    invalidField?: string | null;
};

export type ErrorType = "validation" | "not_found" | "failure" | "conflict";

export type GetLocationsResponse = {
    items: Location[];
    totalCount: number;
};

export const locationsApi = {
    getLocations: async (request: GetLocationsRequest): Promise<Location[]> => {
        const response = await apiClient.get<Envelope<GetLocationsResponse>>(
            "/locations",
            {
                params: request,
            },
        );

        if (response.data.isError) {
            throw new Error("Не удалось получить список локаций");
        }

        return response.data.result?.items ?? [];
    },

    createLocation: async (
        request: CreateLocationRequest,
    ): Promise<Location> => {
        const response = await apiClient.post<Location>("/locations", request);

        return response.data;
    },
};
