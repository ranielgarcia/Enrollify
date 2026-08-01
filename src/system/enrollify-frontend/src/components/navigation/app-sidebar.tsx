import * as React from "react";
import { GraduationCap } from "lucide-react";

import { NavMain } from "@/components/navigation/nav-main";
import { NavUser } from "@/components/navigation/nav-user";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarMenuButton,
  SidebarRail,
  SidebarSeparator,
} from "@/components/ui/sidebar";
import { useAuthenticationContext } from "@/infrastructure/authentication/authentication-context";
import { navigationItems } from "@/components/navigation/navigation-config";
import type { NavMainItemProp } from "@/components/navigation/nav-main";

const navMain = navigationItems as NavMainItemProp[];

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  const userContext = useAuthenticationContext();

  return (
    <Sidebar collapsible="icon" {...props}>
      <SidebarHeader className="pb-0">
        <SidebarMenuButton
          size="lg"
          className="data-[state=open]:bg-sidebar-accent data-[state=open]:text-sidebar-accent-foreground hover:bg-transparent cursor-default"
        >
          <div className="bg-sidebar-primary text-sidebar-primary-foreground flex aspect-square size-9 items-center justify-center rounded-lg">
            <GraduationCap className="size-5" />
          </div>
          <div className="grid flex-1 text-left leading-tight">
            <span className="truncate text-[15px] font-bold tracking-[-0.01em]">
              Enrollify
            </span>
          </div>
        </SidebarMenuButton>
      </SidebarHeader>
      <SidebarContent className="overflow-y-auto scrollbar-thin pt-1">
        <NavMain items={navMain} />
      </SidebarContent>
      <SidebarSeparator className="mx-4 opacity-40" />
      <SidebarFooter>
        <NavUser
          user={{
            name: userContext.user?.fullName ?? "Guest User",
            email: userContext.user?.email ?? "",
            avatar: "/avatars/default-avatar.png",
          }}
        />
      </SidebarFooter>
      <SidebarRail />
    </Sidebar>
  );
}
