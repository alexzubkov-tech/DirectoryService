"use client";

import { SidebarProvider, SidebarInset } from "@/shared/components/ui/sidebar";
import Header from "../header/header";
import { AppSidebar } from "../sidebar/app.sidebar";
import { Toaster } from "sonner";

export default function Layout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <SidebarProvider defaultOpen={true}>
      <AppSidebar />
      <SidebarInset className="min-w-0 bg-[#0a0d0c]">
        <Header />
        <main className="flex-1 overflow-x-hidden px-4 py-4 sm:px-6 sm:py-6 lg:px-8 lg:py-8 xl:px-10 xl:py-10">
          <Toaster position="top-center" duration={3000} richColors={true} />
          <div className="mx-auto w-full max-w-7xl">{children}</div>
        </main>
      </SidebarInset>
    </SidebarProvider>
  );
}