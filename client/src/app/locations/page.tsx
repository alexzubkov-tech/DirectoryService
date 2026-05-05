"use client";

import { useState, useRef, useCallback } from "react";
import { Search, Plus } from "lucide-react";
import Link from "next/link";

import { useLocationsList } from "@/features/locations/model/use-locations-list";
import { LocationsList } from "@/features/locations/locations-list";
import { LocationsPagination } from "@/features/locations/locations-pagination";

type ActivityFilter = "all" | "active" | "inactive";

const EMPTY_DEPS: string[] = [];
const DEBOUNCE_MS = 400;

function getIsActive(filter: ActivityFilter): boolean | undefined {
  if (filter === "active") return true;
  if (filter === "inactive") return false;
  return undefined;
}

export default function LocationsPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [activityFilter, setActivityFilter] = useState<ActivityFilter>("all");

  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const handleSearchChange = useCallback(
    (value: string) => {
      setSearch(value);

      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }

      timeoutRef.current = setTimeout(() => {
        setDebouncedSearch(value);
        setPage(1);
      }, DEBOUNCE_MS);
    },
    [],
  );

  const handleFilterChange = useCallback((filter: ActivityFilter) => {
    setActivityFilter(filter);
    setPage(1);
  }, []);

  const isActive = getIsActive(activityFilter);

  const {
    locations,
    totalPages,
    totalCount,
    isLoading,
    isError,
    error,
    refetch,
  } = useLocationsList({
    page,
    search: debouncedSearch || undefined,
    isActive,
    departmentIds: EMPTY_DEPS,
  });

  return (
    <div className="flex w-full flex-col gap-6">
      <section className="rounded-3xl border border-[#2f281f] bg-[#111816] p-6 sm:p-8 lg:p-10">
        <span className="inline-flex rounded-full border border-emerald-900/60 bg-emerald-950/40 px-3 py-1 text-sm uppercase tracking-[0.2em] text-emerald-300">
          раздел локаций
        </span>

        <div className="mt-5 flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
          <div>
            <h1 className="max-w-4xl text-3xl font-semibold tracking-tight text-stone-50 sm:text-4xl lg:text-5xl">
              Локации компании
            </h1>

            <p className="mt-4 max-w-4xl text-base leading-7 text-stone-300 sm:text-lg">
              В этом разделе собрана информация о рабочих локациях компании.
            </p>
          </div>

          <div className="flex items-center gap-3">
            <Link
              href="/locations/create"
              className="inline-flex items-center gap-2 rounded-2xl border border-emerald-900/60 bg-emerald-950/40 px-5 py-3 text-base text-emerald-300 transition hover:bg-emerald-950/60"
            >
              <Plus className="h-5 w-5" />
              Создать
            </Link>

            <div className="rounded-2xl border border-[#2f281f] bg-[#0d1210] px-5 py-4">
              <div className="text-sm uppercase tracking-[0.2em] text-stone-500">
                всего
              </div>

              <div className="mt-2 text-3xl font-semibold text-stone-100">
                {totalCount}
              </div>
            </div>
          </div>
        </div>

        <div className="mt-6 grid grid-cols-1 gap-4 lg:grid-cols-[1fr_auto]">
          <div className="relative">
            <Search className="pointer-events-none absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-stone-500" />

            <input
              value={search}
              onChange={(event) => handleSearchChange(event.target.value)}
              maxLength={100}
              placeholder="Поиск по локациям (название, адрес)"
              className="w-full rounded-2xl border border-[#2f281f] bg-[#0d1210] py-3 pl-12 pr-10 text-base text-stone-100 outline-none transition placeholder:text-stone-500 focus:border-emerald-900/70"
            />

            {search && (
              <button
                type="button"
                onClick={() => handleSearchChange("")}
                className="absolute right-4 top-1/2 -translate-y-1/2 text-lg leading-none text-stone-500 hover:text-stone-300"
                aria-label="Очистить поиск"
              >
                ×
              </button>
            )}
          </div>

          <div className="flex gap-2 sm:gap-3">
            {(["all", "active", "inactive"] as ActivityFilter[]).map(
              (filter) => {
                const labels: Record<ActivityFilter, string> = {
                  all: "Все",
                  active: "Активные",
                  inactive: "Неактивные",
                };

                const isSelected = activityFilter === filter;

                return (
                  <button
                    key={filter}
                    type="button"
                    onClick={() => handleFilterChange(filter)}
                    className={`rounded-2xl border px-4 py-3 text-base transition sm:px-5 ${
                      isSelected
                        ? "border-emerald-900/70 bg-emerald-950/40 text-emerald-300"
                        : "border-[#2f281f] bg-[#0d1210] text-stone-200 hover:border-emerald-900/70"
                    }`}
                  >
                    {labels[filter]}
                  </button>
                );
              },
            )}
          </div>
        </div>

        {search.length >= 100 && (
          <p className="mt-2 text-sm text-red-400">Максимум 100 символов</p>
        )}

        {debouncedSearch && (
          <p className="mt-4 text-sm text-stone-400">
            Поиск:{" "}
            <span className="text-stone-200">&quot;{debouncedSearch}&quot;</span>
            {isLoading && " …загрузка"}
          </p>
        )}
      </section>

      <LocationsList
        locations={locations}
        isLoading={isLoading}
        isError={isError}
        error={error || null}
        onRetry={refetch}
      />

      {locations.length > 0 && (
        <LocationsPagination
          currentPage={page}
          totalPages={totalPages}
          onPageChange={setPage}
        />
      )}
    </div>
  );
}
