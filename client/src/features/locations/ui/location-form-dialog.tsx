"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";

import type { Location } from "@/entities/locations/types";
import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { useCreateLocation } from "../model/use-create-location";
import { useUpdateLocation } from "../model/use-update-location";

const locationSchema = z.object({
  name: z
    .string()
    .trim()
    .min(3, "Название должно содержать не менее 3 символов")
    .max(120, "Название должно содержать не более 120 символов"),
  country: z.string().trim().min(1, "Страна обязательна"),
  city: z.string().trim().min(1, "Город обязателен"),
  street: z.string().trim().min(1, "Улица обязательна"),
  buildingNumber: z.string().trim().min(1, "Номер дома обязателен"),
  timezone: z.string().trim().min(1, "Часовой пояс обязателен"),
});

type LocationFormData = z.infer<typeof locationSchema>;

type LocationFormDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  location?: Location | null;
  onSaved?: (location: Location | null, nextIsActive: boolean) => void;
};

const emptyLocationData: LocationFormData = {
  name: "",
  country: "",
  city: "",
  street: "",
  buildingNumber: "",
  timezone: "",
};

const timezones = [
  "Europe/Moscow",
  "Europe/Samara",
  "Asia/Yekaterinburg",
  "Asia/Novosibirsk",
  "Asia/Krasnoyarsk",
  "Asia/Irkutsk",
  "Asia/Vladivostok",
  "Asia/Kamchatka",
];

function getLocationFormData(location?: Location | null): LocationFormData {
  if (!location) return emptyLocationData;

  return {
    name: location.name,
    country: location.address.country,
    city: location.address.city,
    street: location.address.street,
    buildingNumber: location.address.buildingNumber,
    timezone: location.timeZone,
  };
}

function LocationForm({
  location,
  onOpenChange,
  onSaved,
}: Omit<LocationFormDialogProps, "open">) {
  const isEditMode = Boolean(location);
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LocationFormData>({
    defaultValues: getLocationFormData(location),
    resolver: zodResolver(locationSchema),
  });

  const {
    createLocation,
    isPending: isCreatePending,
    error: createError,
  } = useCreateLocation();
  const {
    updateLocation,
    isPending: isUpdatePending,
    error: updateError,
  } = useUpdateLocation();

  const isPending = isCreatePending || isUpdatePending;
  const submitError = createError ?? updateError;

  const onSubmit = (data: LocationFormData) => {
    if (location) {
      updateLocation(
        { id: location.id, ...data, isActive: location.isActive },
        {
          onSuccess: () => {
            onSaved?.(location, location.isActive);
            onOpenChange(false);
          },
        },
      );
      return;
    }

    createLocation(data, {
      onSuccess: () => {
        onSaved?.(null, true);
        onOpenChange(false);
      },
    });
  };

  return (
    <>
      <DialogHeader className="p-6 pb-0">
        <DialogTitle className="text-2xl font-semibold text-stone-100">
          {isEditMode ? "Редактировать локацию" : "Новая локация"}
        </DialogTitle>
        <DialogDescription className="text-base text-stone-400">
          {isEditMode
            ? "Измените данные локации."
            : "Заполните данные, чтобы добавить рабочую локацию."}
        </DialogDescription>
      </DialogHeader>

      <form onSubmit={handleSubmit(onSubmit)} className="p-6 pt-4">
        <div className="grid gap-4">
          <div className="grid gap-2">
            <Label htmlFor="name" className="text-stone-200">
              Название локации
            </Label>
            <Input
              id="name"
              placeholder="Например: Главный офис"
              className="w-full border-[#2f281f] bg-[#0d1210] text-stone-100 placeholder:text-stone-500"
              {...register("name")}
            />
            {errors.name && (
              <p className="text-sm text-destructive">{errors.name.message}</p>
            )}
          </div>


          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div className="grid gap-2">
              <Label htmlFor="country" className="text-stone-200">
                Страна
              </Label>
              <Input
                id="country"
                placeholder="Россия"
                className="w-full border-[#2f281f] bg-[#0d1210] text-stone-100 placeholder:text-stone-500"
                {...register("country")}
              />
              {errors.country && (
                <p className="text-sm text-destructive">
                  {errors.country.message}
                </p>
              )}
            </div>

            <div className="grid gap-2">
              <Label htmlFor="city" className="text-stone-200">
                Город
              </Label>
              <Input
                id="city"
                placeholder="Москва"
                className="w-full border-[#2f281f] bg-[#0d1210] text-stone-100 placeholder:text-stone-500"
                {...register("city")}
              />
              {errors.city && (
                <p className="text-sm text-destructive">
                  {errors.city.message}
                </p>
              )}
            </div>
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div className="grid gap-2">
              <Label htmlFor="street" className="text-stone-200">
                Улица
              </Label>
              <Input
                id="street"
                placeholder="Ленина"
                className="w-full border-[#2f281f] bg-[#0d1210] text-stone-100 placeholder:text-stone-500"
                {...register("street")}
              />
              {errors.street && (
                <p className="text-sm text-destructive">
                  {errors.street.message}
                </p>
              )}
            </div>

            <div className="grid gap-2">
              <Label htmlFor="buildingNumber" className="text-stone-200">
                Номер дома
              </Label>
              <Input
                id="buildingNumber"
                placeholder="1"
                className="w-full border-[#2f281f] bg-[#0d1210] text-stone-100 placeholder:text-stone-500"
                {...register("buildingNumber")}
              />
              {errors.buildingNumber && (
                <p className="text-sm text-destructive">
                  {errors.buildingNumber.message}
                </p>
              )}
            </div>
          </div>

          <div className="grid gap-2">
            <Label htmlFor="timezone" className="text-stone-200">
              Часовой пояс
            </Label>
            <select
              id="timezone"
              className="flex h-9 w-full rounded-md border border-[#2f281f] bg-[#0d1210] px-3 py-1 text-base text-stone-100 shadow-xs outline-none focus-visible:ring-2 focus-visible:ring-emerald-900/50"
              {...register("timezone")}
            >
              <option value="" disabled className="bg-[#0d1210]">
                Выберите часовой пояс
              </option>
              {timezones.map((timezone) => (
                <option key={timezone} value={timezone} className="bg-[#0d1210]">
                  {timezone}
                </option>
              ))}
            </select>
            {errors.timezone && (
              <p className="text-sm text-destructive">
                {errors.timezone.message}
              </p>
            )}
          </div>

          {submitError && (
            <p className="rounded-2xl border border-red-950/70 bg-red-950/30 px-4 py-3 text-sm text-red-300">
              {submitError.firstMessage}
            </p>
          )}

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={isPending}
            >
              Отмена
            </Button>
            <Button type="submit" disabled={isPending}>
              {isPending
                ? "Сохранение..."
                : isEditMode
                  ? "Сохранить"
                  : "Создать локацию"}
            </Button>
          </DialogFooter>
        </div>
      </form>
    </>
  );
}

export function LocationFormDialog({
  open,
  onOpenChange,
  location,
  onSaved,
}: LocationFormDialogProps) {
  const formKey = open ? location?.id ?? "create" : "closed";

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[90vh] overflow-y-auto rounded-3xl border border-[#2f281f] bg-[#111816] p-0 sm:max-w-[600px]">
        <LocationForm
          key={formKey}
          location={location}
          onOpenChange={onOpenChange}
          onSaved={onSaved}
        />
      </DialogContent>
    </Dialog>
  );
}
