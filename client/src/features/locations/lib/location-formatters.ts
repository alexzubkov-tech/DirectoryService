import type { Location } from "@/entities/locations/types";

export function getFullAddressText(location: Location) {
  const { country, city, street, buildingNumber } = location.address;
  const streetWithBuilding = [street, buildingNumber].filter(Boolean).join(" ");

  return (
    [country, city, streetWithBuilding].filter(Boolean).join(", ") ||
    "Адрес не указан"
  );
}

export function getCreatedAtText(createdAt: string) {
  if (!createdAt) return "Не указано";

  const date = new Date(createdAt);

  if (Number.isNaN(date.getTime())) return "Не указано";

  return date.toLocaleDateString("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
}

export function getDepartmentsCount(location: Location) {
  return location.departments?.length ?? 0;
}
