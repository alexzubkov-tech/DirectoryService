import type { Metadata, Viewport } from "next";
import { Geist, Geist_Mono, Inter } from "next/font/google";
import "./globals.css";

import { cn } from "@/shared/lib/utils";
import Header from "@/features/header/header";
import { AppSidebar } from "@/features/sidebar/app.sidebar";
import { SidebarInset, SidebarProvider } from "@/shared/components/ui/sidebar";

const inter = Inter({
  subsets: ["latin"],
  variable: "--font-sans",
});

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Справочник компании",
  description:
    "Приложение для просмотра подразделений, локаций и должностей компании.",
};

export const viewport: Viewport = {
  width: "device-width",
  initialScale: 1,
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="ru" className="dark">
      <body
        className={cn(
          inter.variable,
          geistSans.variable,
          geistMono.variable,
          "min-h-screen bg-[#0a0d0c] text-stone-100 antialiased"
        )}
      >
        <SidebarProvider defaultOpen={true}>
          <AppSidebar />

          <SidebarInset className="min-w-0 bg-[#0a0d0c]">
            <Header />

            <main className="flex-1 overflow-x-hidden px-4 py-4 sm:px-6 sm:py-6 lg:px-8 lg:py-8 xl:px-10 xl:py-10">
              <div className="mx-auto w-full max-w-7xl">{children}</div>
            </main>
          </SidebarInset>
        </SidebarProvider>
      </body>
    </html>
  );
}
