import { queryOptions, useQuery } from "@tanstack/react-query";
import { isEnvelopeError } from "@/shared/api/errors";
import { locationsApi } from "@/entities/locations/api";

const PAGE_SIZE = 2;
const MAX_SEARCH_LENGTH = 100;

type UseLocationsListParams = {
    page: number;
    search?: string;
    isActive?: boolean;
    departmentIds: string[];
};

export function getLocationsQueryOptions({
    page,
    search,
    isActive,
    departmentIds,
}: UseLocationsListParams) {
    const trimmedSearch = search
        ? search.trim().slice(0, MAX_SEARCH_LENGTH)
        : undefined;

    return queryOptions({
        queryKey: [
            "locations",
            {
                page,
                pageSize: PAGE_SIZE,
                search: trimmedSearch,
                isActive,
                departmentIds,
            },
        ],
        queryFn: ({ signal }) => {
            const request: Parameters<typeof locationsApi.getLocations>[0] = {
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
}

export function useLocationsList({
    page,
    search,
    isActive,
    departmentIds,
}: UseLocationsListParams) {
    const { data, isLoading, isFetching, error, isError, refetch } = useQuery(
        getLocationsQueryOptions({
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
        pageSize: data?.pageSize ?? PAGE_SIZE,
        isError,
        error: isEnvelopeError(error) ? error : undefined,
        isLoading,
        isFetching,
        refetch,
    };
}
