import { useQuery } from "@tanstack/react-query";
import { isEnvelopeError } from "@/shared/api/errors";
import {
    GetLocationsQueryParams,
    locationsQueryOptions,
} from "@/entities/locations/api";

export function useLocationsList({
    page,
    search,
    isActive,
    departmentIds,
}: GetLocationsQueryParams) {
    const { data, isLoading, isFetching, error, isError, refetch } = useQuery(
        locationsQueryOptions.getLocationsOptions({
            page,
            search,
            isActive,
            departmentIds,
        }),
    );

    return {
        locations: data?.items ?? [],
        totalPages: data?.totalPages ?? 1,
        totalCount: data?.totalCount ?? 0,
        currentPage: data?.page ?? page,
        pageSize: data?.pageSize ?? 2,
        isError,
        error: isEnvelopeError(error) ? error : undefined,
        isLoading,
        isFetching,
        refetch,
    };
}
