"use client";

import { useState } from "react";
import Link from "next/link";
import axios from "axios";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import {
  Building2,
  CalendarDays,
  CheckCircle2,
  Clock,
  MapPinned,
  Navigation,
  Search,
  XCircle,
} from "lucide-react";

import { locationsApi, type ApiError } from "@/entities/locations/api";
import type { Location } from "@/entities/locations/types";
import { Spinner } from "@/shared/components/ui/spinner";
import {
  Pagination,
  PaginationContent,
  PaginationEllipsis,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/shared/components/ui/pagination";

const PAGE_SIZE = 2;

function getFullAddressText(location: Location) {
  const { country, city, street, buildingNumber } = location.address;
  const streetWithBuilding = [street, buildingNumber].filter(Boolean).join(" ");

  return (
    [country, city, streetWithBuilding].filter(Boolean).join(", ") ||
    "Адрес не указан"
  );
}

function getCreatedAtText(createdAt: string) {
  if (!createdAt) return "Не указано";

  const date = new Date(createdAt);

  if (Number.isNaN(date.getTime())) return "Не указано";

  return date.toLocaleDateString("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
}

function getDepartmentsCount(location: Location) {
  return location.departments?.length ?? 0;
}

function getErrorMessage(error: unknown) {
  if (
    axios.isAxiosError<{
      errorList?: ApiError[] | null;
      message?: string;
    }>(error)
  ) {
    return (
      error.response?.data?.errorList?.[0]?.messages?.[0]?.message ??
      error.response?.data?.message ??
      error.message ??
      "Ошибка запроса к серверу"
    );
  }

  if (error instanceof Error) return error.message;

  return "Не удалось загрузить локации";
}

function getPaginationPages(currentPage: number, totalPages: number) {
  const pages: Array<number | "..."> = [];

  if (totalPages <= 7) {
    for (let page = 1; page <= totalPages; page += 1) {
      pages.push(page);
    }

    return pages;
  }

  pages.push(1);

  if (currentPage > 3) {
    pages.push("...");
  }

  const startPage = Math.max(2, currentPage - 1);
  const endPage = Math.min(totalPages - 1, currentPage + 1);

  for (let page = startPage; page <= endPage; page += 1) {
    pages.push(page);
  }

  if (currentPage < totalPages - 2) {
    pages.push("...");
  }

  pages.push(totalPages);

  return pages;
}

export default function LocationsPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [isActive, setIsActive] = useState(true);

  const { data, isLoading, isFetching, error, isError } = useQuery({
    queryKey: [
      "locations",
      {
        page,
        pageSize: PAGE_SIZE,
        search,
        isActive,
        departmentIds: [],
      },
    ],
   queryFn: () =>
  locationsApi.getLocations({
    departmentIds: [],
    search,
    isActive,
    page,
    pageSize: PAGE_SIZE,
  }),
    placeholderData: keepPreviousData,
  });

  const locations = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;
  const currentPage = data?.page ?? page;
  const currentPageSize = data?.pageSize ?? PAGE_SIZE;
  const totalPages = data?.totalPages ?? 1;
  const paginationPages = getPaginationPages(currentPage, totalPages);

  function handlePageChange(nextPage: number) {
    if (nextPage < 1 || nextPage > totalPages || nextPage === currentPage) {
      return;
    }

    setPage(nextPage);
  }

  if (isLoading) {
    return (
      <div className="flex min-h-[420px] items-center justify-center rounded-3xl border border-[#2f281f] bg-[#111816]">
        <div className="flex flex-col items-center gap-4 text-stone-300">
          <Spinner />
          <p className="text-base">Загрузка локаций...</p>
        </div>
      </div>
    );
  }

  if (isError) {
    return (
      <section className="rounded-3xl border border-red-950/70 bg-[#111816] p-6 sm:p-8 lg:p-10">
        <span className="inline-flex rounded-full border border-red-900/60 bg-red-950/30 px-3 py-1 text-sm uppercase tracking-[0.2em] text-red-300">
          ошибка загрузки
        </span>

        <h1 className="mt-4 text-3xl font-semibold text-stone-50 sm:text-4xl">
          Не удалось получить локации
        </h1>

        <p className="mt-4 max-w-3xl text-base leading-7 text-stone-300 sm:text-lg">
          {getErrorMessage(error)}
        </p>
      </section>
    );
  }

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

          <div className="rounded-2xl border border-[#2f281f] bg-[#0d1210] px-5 py-4">
            <div className="text-sm uppercase tracking-[0.2em] text-stone-500">
              всего
            </div>

            <div className="mt-2 text-3xl font-semibold text-stone-100">
              {totalCount}
            </div>
          </div>
        </div>

        <div className="mt-6 grid grid-cols-1 gap-4 lg:grid-cols-[1fr_auto]">
          <label className="relative block">
            <Search className="pointer-events-none absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-stone-500" />

            <input
              value={search}
              onChange={(event) => {
                setSearch(event.target.value);
                setPage(1);
              }}
              placeholder="Поиск по локациям"
              className="w-full rounded-2xl border border-[#2f281f] bg-[#0d1210] py-3 pl-12 pr-4 text-base text-stone-100 outline-none transition placeholder:text-stone-500 focus:border-emerald-900/70"
            />
          </label>

          <button
            type="button"
            onClick={() => {
              setIsActive((current) => !current);
              setPage(1);
            }}
            className="rounded-2xl border border-[#2f281f] bg-[#0d1210] px-5 py-3 text-base text-stone-200 transition hover:border-emerald-900/70"
          >
            {isActive ? "Активные" : "Неактивные"}
          </button>
        </div>
      </section>

      {locations.length === 0 ? (
        <section className="rounded-3xl border border-[#2f281f] bg-[#111816] p-6 sm:p-8">
          <div className="flex flex-col gap-4 md:flex-row md:items-center">
            <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-emerald-950/40 text-emerald-300">
              <MapPinned className="h-6 w-6" />
            </div>

            <div>
              <h2 className="text-2xl font-semibold text-stone-100">
                Локации не найдены
              </h2>

              <p className="mt-2 text-base leading-7 text-stone-400">
                Список пустой. Проверь данные в базе или параметры фильтрации.
              </p>
            </div>
          </div>
        </section>
      ) : (
        <section className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          {locations.map((location) => (
            <Link
              key={location.id}
              href={`/locations/${location.id}`}
              className="group rounded-2xl border border-[#2f281f] bg-[#111816] p-5 transition hover:border-emerald-900/70 hover:bg-[#141d1a]"
            >
              <div className="flex items-start justify-between gap-3">
                <div className="flex min-w-0 items-start gap-3">
                  <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-2xl bg-emerald-950/40 text-emerald-300">
                    <MapPinned className="h-5 w-5" />
                  </div>

                  <div className="min-w-0">
                    <h2 className="truncate text-xl font-semibold text-stone-100 group-hover:text-white">
                      {location.name}
                    </h2>

                    <p className="mt-2 line-clamp-2 text-base leading-6 text-stone-400">
                      {getFullAddressText(location)}
                    </p>
                  </div>
                </div>

                {isActive ? (
                  <span className="inline-flex shrink-0 items-center gap-1 rounded-full border border-emerald-900/60 bg-emerald-950/40 px-2.5 py-1 text-sm text-emerald-300">
                    <CheckCircle2 className="h-4 w-4" />
                    Активна
                  </span>
                ) : (
                  <span className="inline-flex shrink-0 items-center gap-1 rounded-full border border-red-900/60 bg-red-950/30 px-2.5 py-1 text-sm text-red-300">
                    <XCircle className="h-4 w-4" />
                    Неактивна
                  </span>
                )}
              </div>

              <div className="mt-5 grid grid-cols-1 gap-3">
                <div className="rounded-2xl border border-[#2f281f] bg-[#0d1210] px-4 py-3">
                  <div className="flex items-center gap-2 text-sm uppercase tracking-[0.16em] text-stone-500">
                    <Navigation className="h-4 w-4" />
                    полный адрес
                  </div>

                  <div className="mt-2 text-base leading-6 text-stone-200">
                    {getFullAddressText(location)}
                  </div>
                </div>

                <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
                  <div className="rounded-2xl border border-[#2f281f] bg-[#0d1210] px-4 py-3">
                    <div className="flex items-center gap-2 text-sm uppercase tracking-[0.16em] text-stone-500">
                      <Clock className="h-4 w-4" />
                      пояс
                    </div>

                    <div className="mt-2 truncate text-base text-stone-200">
                      {location.timeZone || "—"}
                    </div>
                  </div>

                  <div className="rounded-2xl border border-[#2f281f] bg-[#0d1210] px-4 py-3">
                    <div className="flex items-center gap-2 text-sm uppercase tracking-[0.16em] text-stone-500">
                      <Building2 className="h-4 w-4" />
                      отделы
                    </div>

                    <div className="mt-2 text-base text-stone-200">
                      {getDepartmentsCount(location)}
                    </div>
                  </div>

                  <div className="rounded-2xl border border-[#2f281f] bg-[#0d1210] px-4 py-3">
                    <div className="flex items-center gap-2 text-sm uppercase tracking-[0.16em] text-stone-500">
                      <CalendarDays className="h-4 w-4" />
                      создана
                    </div>

                    <div className="mt-2 truncate text-base text-stone-200">
                      {getCreatedAtText(location.createdAt)}
                    </div>
                  </div>
                </div>
              </div>
            </Link>
          ))}
        </section>
      )}

      <section className="rounded-2xl border border-[#2f281f] bg-[#111816] p-4">
        <div className="mb-4 text-center text-base text-stone-400">
          Страница {currentPage} из {totalPages}
          <span className="ml-2 text-stone-500">
            · по {currentPageSize} на странице
          </span>
          {isFetching ? " · обновление..." : ""}
        </div>

        <Pagination>
          <PaginationContent>
            <PaginationItem>
              <PaginationPrevious
                href="#"
                text="Назад"
                onClick={(event) => {
                  event.preventDefault();
                  handlePageChange(currentPage - 1);
                }}
                className={
                  currentPage <= 1
                    ? "pointer-events-none opacity-40"
                    : "text-stone-200"
                }
              />
            </PaginationItem>

            {paginationPages.map((paginationPage, index) =>
              paginationPage === "..." ? (
                <PaginationItem key={`ellipsis-${index}`}>
                  <PaginationEllipsis className="text-stone-500" />
                </PaginationItem>
              ) : (
                <PaginationItem key={paginationPage}>
                  <PaginationLink
                    href="#"
                    isActive={paginationPage === currentPage}
                    onClick={(event) => {
                      event.preventDefault();
                      handlePageChange(paginationPage);
                    }}
                    className={
                      paginationPage === currentPage
                        ? "border-emerald-900/70 bg-emerald-950/40 text-emerald-300"
                        : "text-stone-300 hover:text-white"
                    }
                  >
                    {paginationPage}
                  </PaginationLink>
                </PaginationItem>
              )
            )}

            <PaginationItem>
              <PaginationNext
                href="#"
                text="Вперёд"
                onClick={(event) => {
                  event.preventDefault();
                  handlePageChange(currentPage + 1);
                }}
                className={
                  currentPage >= totalPages
                    ? "pointer-events-none opacity-40"
                    : "text-stone-200"
                }
              />
            </PaginationItem>
          </PaginationContent>
        </Pagination>
      </section>
    </div>
  );
}