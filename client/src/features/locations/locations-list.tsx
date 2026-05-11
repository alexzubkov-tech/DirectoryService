"use client";

import Link from "next/link";
import {
  Building2,
  CalendarDays,
  CheckCircle2,
  Clock,
  MapPinned,
  Navigation,
} from "lucide-react";

import type { Location } from "@/entities/locations/types";
import { Spinner } from "@/shared/components/ui/spinner";

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

type LocationsListProps = {
  locations: Location[];
  isLoading: boolean;
  isError: boolean;
  error: Error | null;
  onRetry?: () => void;
};

export function LocationsList({
  locations,
  isLoading,
  isError,
  error,
  onRetry,
}: LocationsListProps) {
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
          {error?.message || "Ошибка запроса к серверу"}
        </p>

        {onRetry && (
          <button
            onClick={onRetry}
            className="mt-4 rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-emerald-500"
          >
            Повторить
          </button>
        )}
      </section>
    );
  }

  if (locations.length === 0) {
    return (
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
    );
  }

  return (
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

            <span className="inline-flex shrink-0 items-center gap-1 rounded-full border border-emerald-900/60 bg-emerald-950/40 px-2.5 py-1 text-sm text-emerald-300">
              <CheckCircle2 className="h-4 w-4" />
              Активна
            </span>
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
  );
}