"use client";

import { useState } from "react";
import { MapPin, Clock, Building } from "lucide-react";

import { useCreateLocation } from "@/features/locations/model/use-create-location";
import { Spinner } from "@/shared/components/ui/spinner";
import Link from "next/link";

const TIMEZONES = [
    "Europe/Moscow",
    "Europe/Samara",
    "Asia/Yekaterinburg",
    "Asia/Novosibirsk",
    "Asia/Krasnoyarsk",
    "Asia/Irkutsk",
    "Asia/Vladivostok",
    "Asia/Kamchatka",
];

type FormField =
    | "name"
    | "country"
    | "city"
    | "street"
    | "buildingNumber"
    | "timezone";

type FormErrors = Partial<Record<FormField, string>>;

function validateForm(data: Record<FormField, string>): FormErrors {
    const errors: FormErrors = {};

    if (!data.name.trim()) errors.name = "Название обязательно";
    if (data.name.trim().length > 120)
        errors.name = "Максимум 120 символов";

    if (!data.country.trim()) errors.country = "Страна обязательна";
    if (!data.city.trim()) errors.city = "Город обязателен";
    if (!data.street.trim()) errors.street = "Улица обязательна";
    if (!data.buildingNumber.trim())
        errors.buildingNumber = "Номер дома обязателен";

    if (!data.timezone) errors.timezone = "Часовой пояс обязателен";

    return errors;
}

export function CreateLocationForm() {
    const [name, setName] = useState("");
    const [country, setCountry] = useState("");
    const [city, setCity] = useState("");
    const [street, setStreet] = useState("");
    const [buildingNumber, setBuildingNumber] = useState("");
    const [timezone, setTimezone] = useState("");
    const [touched, setTouched] = useState<Record<FormField, boolean>>({
        name: false,
        country: false,
        city: false,
        street: false,
        buildingNumber: false,
        timezone: false,
    });

    const { mutate, isPending, isError, error } = useCreateLocation();

    const formData = { name, country, city, street, buildingNumber, timezone };
    const errors = validateForm(formData);
    const hasErrors = Object.keys(errors).length > 0;

    function handleBlur(field: FormField) {
        setTouched((prev) => ({ ...prev, [field]: true }));
    }

    function handleSubmit(event: React.FormEvent) {
        event.preventDefault();

        // Помечаем все поля как touched
        setTouched({
            name: true,
            country: true,
            city: true,
            street: true,
            buildingNumber: true,
            timezone: true,
        });

        if (Object.keys(validateForm(formData)).length > 0) {
            return;
        }

        mutate(formData);
    }

    const inputClass = (field: FormField) =>
        `w-full rounded-2xl border bg-[#0d1210] py-3 px-4 text-base text-stone-100 outline-none transition placeholder:text-stone-500 focus:border-emerald-900/70 ${
            touched[field] && errors[field]
                ? "border-red-900/70 focus:border-red-900/70"
                : "border-[#2f281f]"
        }`;

    const labelClass =
        "block text-sm uppercase tracking-[0.16em] text-stone-500 mb-2";

    return (
        <form onSubmit={handleSubmit} className="flex flex-col gap-6">
            {/* Название */}
            <div>
                <label htmlFor="name" className={labelClass}>
                    <Building className="mb-0.5 mr-1 inline h-4 w-4" />
                    Название локации
                </label>
                <input
                    id="name"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    onBlur={() => handleBlur("name")}
                    maxLength={120}
                    placeholder="Например: Главный офис"
                    className={inputClass("name")}
                />
                {touched.name && errors.name && (
                    <p className="mt-1.5 text-sm text-red-400">{errors.name}</p>
                )}
            </div>

            {/* Адрес */}
            <div className="grid grid-cols-1 gap-5 sm:grid-cols-2">
                <div>
                    <label htmlFor="country" className={labelClass}>
                        <MapPin className="mb-0.5 mr-1 inline h-4 w-4" />
                        Страна
                    </label>
                    <input
                        id="country"
                        value={country}
                        onChange={(e) => setCountry(e.target.value)}
                        onBlur={() => handleBlur("country")}
                        placeholder="Россия"
                        className={inputClass("country")}
                    />
                    {touched.country && errors.country && (
                        <p className="mt-1.5 text-sm text-red-400">
                            {errors.country}
                        </p>
                    )}
                </div>

                <div>
                    <label htmlFor="city" className={labelClass}>
                        Город
                    </label>
                    <input
                        id="city"
                        value={city}
                        onChange={(e) => setCity(e.target.value)}
                        onBlur={() => handleBlur("city")}
                        placeholder="Москва"
                        className={inputClass("city")}
                    />
                    {touched.city && errors.city && (
                        <p className="mt-1.5 text-sm text-red-400">
                            {errors.city}
                        </p>
                    )}
                </div>

                <div>
                    <label htmlFor="street" className={labelClass}>
                        Улица
                    </label>
                    <input
                        id="street"
                        value={street}
                        onChange={(e) => setStreet(e.target.value)}
                        onBlur={() => handleBlur("street")}
                        placeholder="Ленина"
                        className={inputClass("street")}
                    />
                    {touched.street && errors.street && (
                        <p className="mt-1.5 text-sm text-red-400">
                            {errors.street}
                        </p>
                    )}
                </div>

                <div>
                    <label htmlFor="buildingNumber" className={labelClass}>
                        Номер дома
                    </label>
                    <input
                        id="buildingNumber"
                        value={buildingNumber}
                        onChange={(e) => setBuildingNumber(e.target.value)}
                        onBlur={() => handleBlur("buildingNumber")}
                        placeholder="1"
                        className={inputClass("buildingNumber")}
                    />
                    {touched.buildingNumber && errors.buildingNumber && (
                        <p className="mt-1.5 text-sm text-red-400">
                            {errors.buildingNumber}
                        </p>
                    )}
                </div>
            </div>

            {/* Часовой пояс */}
            <div>
                <label htmlFor="timezone" className={labelClass}>
                    <Clock className="mb-0.5 mr-1 inline h-4 w-4" />
                    Часовой пояс
                </label>
                <select
                    id="timezone"
                    value={timezone}
                    onChange={(e) => setTimezone(e.target.value)}
                    onBlur={() => handleBlur("timezone")}
                    className={`${inputClass("timezone")} appearance-none`}
                >
                    <option value="" disabled className="bg-[#0d1210]">
                        Выберите часовой пояс
                    </option>
                    {TIMEZONES.map((tz) => (
                        <option key={tz} value={tz} className="bg-[#0d1210]">
                            {tz}
                        </option>
                    ))}
                </select>
                {touched.timezone && errors.timezone && (
                    <p className="mt-1.5 text-sm text-red-400">
                        {errors.timezone}
                    </p>
                )}
            </div>

            {/* Ошибка от сервера */}
            {isError && (
                <div className="rounded-2xl border border-red-900/60 bg-red-950/20 p-4">
                    <p className="text-sm font-medium text-red-300">
                        Ошибка создания
                    </p>
                    <p className="mt-1 text-sm text-red-400">
                        {error?.firstMessage || "Не удалось создать локацию"}
                    </p>
                </div>
            )}

            {/* Кнопки */}
            <div className="flex flex-col gap-3 sm:flex-row">
                <button
                    type="submit"
                    disabled={isPending || hasErrors}
                    className="inline-flex items-center justify-center gap-2 rounded-2xl border border-emerald-900/60 bg-emerald-950/40 px-6 py-3 text-base text-emerald-300 transition hover:bg-emerald-950/60 disabled:pointer-events-none disabled:opacity-40"
                >
                    {isPending && <Spinner className="h-4 w-4" />}
                    {isPending ? "Создание..." : "Создать локацию"}
                </button>

                <Link
                    href="/locations"
                    className="inline-flex items-center justify-center rounded-2xl border border-[#2f281f] bg-[#0d1210] px-6 py-3 text-base text-stone-300 transition hover:border-emerald-900/70 hover:text-white"
                >
                    Отмена
                </Link>
            </div>
        </form>
    );
}
