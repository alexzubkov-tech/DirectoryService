"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import axios from "axios";
import {
  Building2,
  CalendarDays,
  CheckCircle2,
  Clock,
  MapPinned,
  Navigation,
  XCircle,
} from "lucide-react";

import { locationsApi, type ApiError } from "@/entities/locations/api";
import type { Location } from "@/entities/locations/types";
import { Spinner } from "@/shared/components/ui/spinner";

const PAGE_SIZE = 10;
const IS_ACTIVE_FILTER = true;

function getFullAddressText(location: Location) {
  const { country, city, street, buildingNumber } = location.address;
  const streetWithBuilding = [street, buildingNumber].filter(Boolean).join(" ");

  return (
    [country, city, streetWithBuilding].filter(Boolean).join(", ") ||
    "Адрес не указан"
  );
}

function getCreatedAtText(createdAt: string) {
  if (!createdAt) {
    return "Не указано";
  }

  const date = new Date(createdAt);

  if (Number.isNaN(date.getTime())) {
    return "Не указано";
  }

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
    const responseMessage =
      error.response?.data?.errorList?.[0]?.messages?.[0]?.message ??
      error.response?.data?.message;

    return responseMessage ?? error.message ?? "Ошибка запроса к серверу";
  }

  if (error instanceof Error) {
    return error.message;
  }

  return "Не удалось загрузить локации";
}

export default function LocationsPage() {
  const [page, setPage] = useState(1);
  const [locations, setLocations] = useState<Location[] | null>(null);

  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let isActual = true;

    locationsApi
      .getLocations({
        departmentIds: [],
        search: "",
        isActive: IS_ACTIVE_FILTER,
        paginationRequest: {
          page,
          pageSize: PAGE_SIZE,
        },
      })
      .then((data) => {
        if (!isActual) {
          return;
        }

        setLocations(data);
      })
      .catch((error: unknown) => {
        if (!isActual) {
          return;
        }

        setError(getErrorMessage(error));
      })
      .finally(() => {
        if (!isActual) {
          return;
        }

        setIsLoading(false);
      });

    return () => {
      isActual = false;
    };
  }, [page]);

  const handlePrevPage = () => {
    setIsLoading(true);
    setError(null);
    setPage((current) => Math.max(1, current - 1));
  };

  const handleNextPage = () => {
    setIsLoading(true);
    setError(null);
    setPage((current) => current + 1);
  };

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

  if (error) {
    return (
      <section className="rounded-3xl border border-red-950/70 bg-[#111816] p-6 sm:p-8 lg:p-10">
        <span className="inline-flex rounded-full border border-red-900/60 bg-red-950/30 px-3 py-1 text-sm uppercase tracking-[0.2em] text-red-300">
          ошибка загрузки
        </span>

        <h1 className="mt-4 text-3xl font-semibold text-stone-50 sm:text-4xl">
          Не удалось получить локации
        </h1>

        <p className="mt-4 max-w-3xl text-base leading-7 text-stone-300 sm:text-lg">
          {error}
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
              В этом разделе собрана информация о рабочих локациях компании:
              офисах, зданиях, филиалах и других площадках.
            </p>

            <p className="mt-4 max-w-4xl text-base leading-7 text-stone-400 sm:text-lg">
              Здесь можно быстро посмотреть, какие объекты есть в компании и где
              именно они расположены.
            </p>
          </div>

          <div className="rounded-2xl border border-[#2f281f] bg-[#0d1210] px-5 py-4">
            <div className="text-sm uppercase tracking-[0.2em] text-stone-500">
              получено
            </div>

            <div className="mt-2 text-3xl font-semibold text-stone-100">
              {locations?.length ?? 0}
            </div>
          </div>
        </div>
      </section>

      {!locations || locations.length === 0 ? (
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
                Список локаций пустой. Проверь данные в базе или текущие
                параметры запроса.
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

                {IS_ACTIVE_FILTER ? (
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

      <section className="flex flex-col gap-3 rounded-3xl border border-[#2f281f] bg-[#111816] p-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="text-base text-stone-400">
          Страница <span className="text-stone-200">{page}</span>
        </div>

        <div className="flex gap-3">
          <button
            type="button"
            disabled={page === 1}
            onClick={handlePrevPage}
            className="rounded-2xl border border-[#2f281f] bg-[#0d1210] px-4 py-2 text-base text-stone-200 transition hover:bg-emerald-950/40 hover:text-white disabled:cursor-not-allowed disabled:opacity-40"
          >
            Назад
          </button>

          <button
            type="button"
            disabled={!locations || locations.length < PAGE_SIZE}
            onClick={handleNextPage}
            className="rounded-2xl border border-[#2f281f] bg-[#0d1210] px-4 py-2 text-base text-stone-200 transition hover:bg-emerald-950/40 hover:text-white disabled:cursor-not-allowed disabled:opacity-40"
          >
            Вперед
          </button>
        </div>
      </section>
    </div>
  );
}