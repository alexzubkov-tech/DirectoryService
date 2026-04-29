import { apiClient } from "@/shared/api/axios-instance";
import type { Location } from "@/entities/locations/types";
import type { PaginationResponse } from "@/shared/api/types";

export type CreateLocationRequest = {
    name: string;
};

export type GetLocationsRequest = {
    departmentIds?: string[];
    search?: string;
    isActive: boolean;
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

export const locationsApi = {
    getLocations: async (
        request: GetLocationsRequest,
    ): Promise<PaginationResponse<Location>> => {
        const response = await apiClient.get<
            Envelope<PaginationResponse<Location>>
        >("/locations", {
            params: request,
        });

        if (response.data.isError) {
            throw new Error(
                response.data.errorList?.[0]?.messages?.[0]?.message ??
                    "Не удалось получить список локаций",
            );
        }

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

    createLocation: async (
        request: CreateLocationRequest,
    ): Promise<Location> => {
        const response = await apiClient.post<Location>("/locations", request);

        return response.data;
    },
};
