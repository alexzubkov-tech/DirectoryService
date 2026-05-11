"use client";

import Link from "next/link";
import { ArrowLeft, MapPinned } from "lucide-react";

import { CreateLocationForm } from "@/features/locations/create-location-form";

export default function CreateLocationPage() {
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

                <div className="mt-6 flex items-start gap-4">
                    <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-emerald-950/40 text-emerald-300">
                        <MapPinned className="h-6 w-6" />
                    </div>

                    <div>
                        <span className="inline-flex rounded-full border border-emerald-900/60 bg-emerald-950/40 px-3 py-1 text-sm uppercase tracking-[0.2em] text-emerald-300">
                            создание
                        </span>

                        <h1 className="mt-3 text-3xl font-semibold tracking-tight text-stone-50 sm:text-4xl">
                            Новая локация
                        </h1>

                        <p className="mt-2 max-w-2xl text-base leading-7 text-stone-400">
                            Заполните данные чтобы добавить новую рабочую
                            локацию в справочник.
                        </p>
                    </div>
                </div>
            </section>

            <section className="rounded-3xl border border-[#2f281f] bg-[#111816] p-6 sm:p-8 lg:p-10">
                <CreateLocationForm />
            </section>
        </div>
    );
}
