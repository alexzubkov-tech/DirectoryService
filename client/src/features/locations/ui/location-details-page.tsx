"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import {
  ArrowLeft,
  Building2,
  CalendarDays,
  CheckCircle2,
  Clock,
  Navigation,
  Pencil,
  RefreshCcw,
  Trash2,
  XCircle,
} from "lucide-react";

import { locationsQueryOptions } from "@/entities/locations/api";
import type { Location } from "@/entities/locations/types";
import { useDeleteLocation } from "../model/use-delete-location";
import { useRestoreLocation } from "../model/use-restore-location";
import { LocationFormDialog } from "./location-form-dialog";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/shared/components/ui/alert-dialog";
import { Spinner } from "@/shared/components/ui/spinner";
import { isEnvelopeError } from "@/shared/api/errors";

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

export function LocationDetailsPage() {
  const params = useParams<{ id: string }>();
  const locationId = params.id;
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [isDeactivateOpen, setIsDeactivateOpen] = useState(false);

  const {
    data: location,
    isLoading,
    isError,
    error,
    refetch,
  } = useQuery(locationsQueryOptions.getLocationByIdOptions({ id: locationId }));

  const { deleteLocation, isPending: isDeletePending } = useDeleteLocation();
  const { restoreLocation, isPending: isRestorePending } = useRestoreLocation();

  const handleAction = () => {
    if (!location) return;

    if (location.isActive) {
      deleteLocation(location.id, {
        onSuccess: () => {
          refetch();
        },
        onSettled: () => {
          setIsDeactivateOpen(false);
        },
      });
    } else {
      restoreLocation(location.id, {
        onSuccess: () => {
          refetch();
        },
        onSettled: () => {
          setIsDeactivateOpen(false);
        },
      });
    }
  };

  if (isLoading) {
    return (
      <div className="flex min-h-[420px] items-center justify-center rounded-3xl border border-[#2f281f] bg-[#111816]">
        <div className="flex flex-col items-center gap-4 text-stone-300">
          <Spinner />
          <p className="text-base">Загрузка локации...</p>
        </div>
      </div>
    );
  }

  if (isError || !location) {
    return (
      <section className="rounded-3xl border border-red-950/70 bg-[#111816] p-6 sm:p-8 lg:p-10">
        <Link
          href="/locations"
          className="inline-flex items-center gap-2 text-base text-stone-400 transition hover:text-white"
        >
          <ArrowLeft className="h-4 w-4" />
          Назад к списку локаций
        </Link>

        <span className="mt-6 inline-flex rounded-full border border-red-900/60 bg-red-950/30 px-3 py-1 text-sm uppercase tracking-[0.2em] text-red-300">
          ошибка загрузки
        </span>

        <h1 className="mt-4 text-3xl font-semibold text-stone-50 sm:text-4xl">
          Не удалось получить локацию
        </h1>

        <p className="mt-4 max-w-3xl text-base leading-7 text-stone-300 sm:text-lg">
          {isEnvelopeError(error)
            ? error.firstMessage
            : error instanceof Error
              ? error.message
              : "Не удалось загрузить локацию"}
        </p>
      </section>
    );
  }

  return (
    <div className="flex w-full flex-col gap-6">
      <section className="rounded-3xl border border-[#2f281f] bg-[#111816] p-6 sm:p-8 lg:p-10">
        <Link
          href="/locations"
          className="inline-flex items-center gap-2 text-base text-stone-400 transition hover:text-white"
        >
          <ArrowLeft className="h-4 w-4" />
          Назад к списку локаций
        </Link>

        <div className="mt-6 flex flex-col gap-5 lg:flex-row lg:items-start lg:justify-between">
          <div>
            <span className="inline-flex rounded-full border border-emerald-900/60 bg-emerald-950/40 px-3 py-1 text-sm uppercase tracking-[0.2em] text-emerald-300">
              карточка локации
            </span>

            <h1 className="mt-4 max-w-4xl text-3xl font-semibold tracking-tight text-stone-50 sm:text-4xl lg:text-5xl">
              {location.name}
            </h1>

            <p className="mt-4 max-w-4xl text-base leading-7 text-stone-300 sm:text-lg">
              {getFullAddressText(location)}
            </p>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            <button
              type="button"
              onClick={() => setIsEditOpen(true)}
              className="inline-flex items-center gap-2 rounded-2xl border border-[#2f281f] bg-[#0d1210] px-4 py-2 text-base text-stone-200 transition hover:border-emerald-900/70 hover:text-emerald-300"
            >
              <Pencil className="h-4 w-4" />
              Редактировать
            </button>

            <button
              type="button"
              onClick={() => setIsDeactivateOpen(true)}
              disabled={isDeletePending || isRestorePending}
              className="inline-flex items-center gap-2 rounded-2xl border border-red-950/70 bg-red-950/20 px-4 py-2 text-base text-red-300 transition hover:bg-red-950/35 disabled:pointer-events-none disabled:opacity-50"
            >
              {location.isActive ? (
                <>
                  <Trash2 className="h-4 w-4" />
                  Удалить
                </>
              ) : (
                <>
                  <RefreshCcw className="h-4 w-4" />
                  Восстановить
                </>
              )}
            </button>
          </div>
        </div>
      </section>

      <section className="grid grid-cols-1 gap-6 xl:grid-cols-[1.2fr_0.8fr]">
        <div className="flex flex-col gap-4">
          <article className="rounded-3xl border border-[#2f281f] bg-[#111816] p-6 sm:p-8">
            <h2 className="text-2xl font-semibold text-stone-100">
              Адрес локации
            </h2>

            <div className="mt-5 grid grid-cols-1 gap-3 sm:grid-cols-2">
              <div className="rounded-2xl border border-[#2f281f] bg-[#0d1210] px-4 py-3">
                <div className="text-sm uppercase tracking-[0.16em] text-stone-500">
                  страна
                </div>

                <div className="mt-2 text-base text-stone-200">
                  {location.address.country || "Не указана"}
                </div>
              </div>

              <div className="rounded-2xl border border-[#2f281f] bg-[#0d1210] px-4 py-3">
                <div className="text-sm uppercase tracking-[0.16em] text-stone-500">
                  город
                </div>

                <div className="mt-2 text-base text-stone-200">
                  {location.address.city || "Не указан"}
                </div>
              </div>

              <div className="rounded-2xl border border-[#2f281f] bg-[#0d1210] px-4 py-3">
                <div className="text-sm uppercase tracking-[0.16em] text-stone-500">
                  улица
                </div>

                <div className="mt-2 text-base text-stone-200">
                  {location.address.street || "Не указана"}
                </div>
              </div>

              <div className="rounded-2xl border border-[#2f281f] bg-[#0d1210] px-4 py-3">
                <div className="text-sm uppercase tracking-[0.16em] text-stone-500">
                  дом
                </div>

                <div className="mt-2 text-base text-stone-200">
                  {location.address.buildingNumber || "Не указан"}
                </div>
              </div>

              <div className="rounded-2xl border border-[#2f281f] bg-[#0d1210] px-4 py-3 sm:col-span-2">
                <div className="flex items-center gap-2 text-sm uppercase tracking-[0.16em] text-stone-500">
                  <Navigation className="h-4 w-4" />
                  полный адрес
                </div>

                <div className="mt-2 text-base leading-7 text-stone-200">
                  {getFullAddressText(location)}
                </div>
              </div>
            </div>
          </article>

          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            <article className="rounded-2xl border border-[#2f281f] bg-[#111816] p-5">
              <div className="flex items-center gap-2 text-sm uppercase tracking-[0.16em] text-stone-500">
                <Clock className="h-4 w-4" />
                часовой пояс
              </div>

              <div className="mt-3 text-xl font-semibold text-stone-100">
                {location.timeZone || "Не указан"}
              </div>
            </article>

            <article className="rounded-2xl border border-[#2f281f] bg-[#111816] p-5">
              <div className="flex items-center gap-2 text-sm uppercase tracking-[0.16em] text-stone-500">
                <CalendarDays className="h-4 w-4" />
                дата создания
              </div>

              <div className="mt-3 text-xl font-semibold text-stone-100">
                {getCreatedAtText(location.createdAt)}
              </div>
            </article>
          </div>
        </div>

        <article className="rounded-3xl border border-[#2f281f] bg-[#111816] p-6 sm:p-8">
          <div className="flex items-center justify-between gap-4">
            <h2 className="text-2xl font-semibold text-stone-100">
              Подразделения
            </h2>

            <span className="rounded-full border border-[#2f281f] bg-[#0d1210] px-3 py-1 text-base text-stone-300">
              {location.departments?.length ?? 0}
            </span>
          </div>

          {location.departments && location.departments.length > 0 ? (
            <div className="mt-5 flex flex-col gap-3">
              {location.departments.map((department, index) => (
                <div
                  key={department.id}
                  className="rounded-2xl border border-[#2f281f] bg-[#0d1210] px-4 py-3"
                >
                  <div className="flex items-center gap-2 text-sm uppercase tracking-[0.16em] text-stone-500">
                    <Building2 className="h-4 w-4" />
                    подразделение {index + 1}
                  </div>

                  <div className="mt-2 text-base font-medium text-stone-100">
                    {department.identificator}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="mt-5 rounded-2xl border border-[#2f281f] bg-[#0d1210] px-4 py-3">
              <div className="flex items-center gap-2 text-sm uppercase tracking-[0.16em] text-stone-500">
                <Building2 className="h-4 w-4" />
                подразделения
              </div>

              <p className="mt-2 text-base leading-7 text-stone-400">
                Для этой локации нет привязанных подразделений.
              </p>
            </div>
          )}
        </article>
      </section>

      <LocationFormDialog
        open={isEditOpen}
        onOpenChange={setIsEditOpen}
        location={location}
      />

      <AlertDialog open={isDeactivateOpen} onOpenChange={setIsDeactivateOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              {location.isActive ? "Удалить локацию?" : "Восстановить локацию?"}
            </AlertDialogTitle>
            <AlertDialogDescription>
              {location.isActive
                ? `Удалить локацию "${location.name}"?`
                : `Восстановить локацию "${location.name}"?`}
            </AlertDialogDescription>
          </AlertDialogHeader>

          <AlertDialogFooter>
            <AlertDialogCancel disabled={isDeletePending || isRestorePending}>
              Отмена
            </AlertDialogCancel>
            <AlertDialogAction
              disabled={isDeletePending || isRestorePending}
              onClick={handleAction}
            >
              {location.isActive ? "Удалить" : "Восстановить"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
