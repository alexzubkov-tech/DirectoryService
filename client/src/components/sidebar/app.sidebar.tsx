"use client";

import type { CSSProperties } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  BriefcaseBusiness,
  Building2,
  House,
  MapPinned,
  PanelLeft,
} from "lucide-react";

import { routes } from "@/shared/routes";
import {
  Sidebar,
  SidebarContent,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from "@/components/ui/sidebar";

const menuItems = [
  { href: routes.home, label: "Главная", icon: House },
  { href: routes.locations, label: "Локации", icon: MapPinned },
  { href: routes.departments, label: "Подразделения", icon: Building2 },
  { href: routes.positions, label: "Должности", icon: BriefcaseBusiness },
];

export function AppSidebar() {
  const pathname = usePathname();
  const { isMobile, setOpen, setOpenMobile, toggleSidebar } = useSidebar();

  const handleNavigate = () => {
    if (isMobile) {
      setOpenMobile(false);
      return;
    }

    setOpen(false);
  };

  return (
    <Sidebar
      collapsible="icon"
      className="border-r border-[#2f281f] bg-[#0f1412]"
      style={
        {
          "--sidebar-width": "15.25rem",
          "--sidebar-width-icon": "3.75rem",
        } as CSSProperties
      }
    >
      <SidebarContent className="p-0">
        {!isMobile ? (
          <div className="flex h-16 items-center border-b border-[#2f281f] px-2">
            <SidebarMenu className="w-full">
              <SidebarMenuItem>
                <SidebarMenuButton
                  onClick={toggleSidebar}
                  className="
                    h-12 rounded-xl text-stone-300
                    hover:bg-emerald-950/40 hover:text-white
                    [&_svg]:!h-6 [&_svg]:!w-6
                    group-data-[collapsible=icon]:mx-auto
                    group-data-[collapsible=icon]:!size-11
                    group-data-[collapsible=icon]:!p-0
                    group-data-[collapsible=icon]:justify-center
                    group-data-[collapsible=icon]:rounded-2xl
                  "
                >
                  <PanelLeft className="h-6 w-6 shrink-0" />
                  <span className="group-data-[collapsible=icon]:hidden">
                    Меню
                  </span>
                </SidebarMenuButton>
              </SidebarMenuItem>
            </SidebarMenu>
          </div>
        ) : null}

        <div className={`px-2 ${isMobile ? "pt-3" : "pt-1"}`}>
          <SidebarMenu className="space-y-3">
            {menuItems.map((item) => {
              const Icon = item.icon;
              const isActive = pathname === item.href;

              return (
                <SidebarMenuItem key={item.href}>
                  <SidebarMenuButton
                    asChild
                    isActive={isActive}
                    className="
                      h-12 rounded-xl text-stone-300
                      hover:bg-emerald-950/40 hover:text-white
                      data-[active=true]:bg-white/10 data-[active=true]:text-white
                      [&_svg]:!h-6 [&_svg]:!w-6
                      group-data-[collapsible=icon]:mx-auto
                      group-data-[collapsible=icon]:!size-11
                      group-data-[collapsible=icon]:!p-0
                      group-data-[collapsible=icon]:justify-center
                      group-data-[collapsible=icon]:rounded-2xl
                    "
                  >
                    <Link
                      href={item.href}
                      title={item.label}
                      onClick={handleNavigate}
                    >
                      <Icon className="h-6 w-6 shrink-0" />
                      <span className="group-data-[collapsible=icon]:hidden">
                        {item.label}
                      </span>
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              );
            })}
          </SidebarMenu>
        </div>
      </SidebarContent>
    </Sidebar>
  );
}