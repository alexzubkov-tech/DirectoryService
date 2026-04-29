"use client";

import Link from "next/link";
import { FolderTree } from "lucide-react";
import { routes } from "@/shared/routes";
import { SidebarTrigger, useSidebar } from "@/shared/components/ui/sidebar";

export default function Header() {
  const { isMobile } = useSidebar();

  return (
    <header className="border-b border-[#2f281f] bg-[#0a0d0c]">
      <div className="mx-auto w-full max-w-7xl px-4 sm:px-6 lg:px-8 xl:px-10">
        <div className="flex h-16 min-w-0 items-center">
          <div className="flex min-w-0 items-center gap-3">
            {isMobile ? (
              <SidebarTrigger className="shrink-0 text-stone-300 hover:bg-emerald-950/40 hover:text-white" />
            ) : null}

            <Link
              href={routes.home}
              className="flex min-w-0 items-center gap-3 rounded-md py-1 transition-colors hover:bg-emerald-950/40"
            >
              <FolderTree className="h-5 w-5 shrink-0 text-stone-300" />

              <div className="min-w-0 leading-tight">
                <div className="truncate text-[11px] uppercase tracking-[0.22em] text-stone-400">
                  единый корпоративный справочник
                </div>
                <div className="truncate text-[17px] font-semibold text-stone-100">
                  Справочник компании
                </div>
              </div>
            </Link>
          </div>
        </div>
      </div>
    </header>
  );
}
