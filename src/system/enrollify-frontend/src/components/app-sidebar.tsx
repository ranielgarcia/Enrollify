import * as React from "react";
import {
  BookOpen,
  Bot,
  GalleryVerticalEnd,
  Settings2,
  DatabaseIcon,
} from "lucide-react";

import { NavMain, type NavMainItemProp } from "@/components/nav-main";
import { NavUser } from "@/components/nav-user";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarMenuButton,
  SidebarRail,
} from "@/components/ui/sidebar";
import { useAuthorizationContext } from "@/infrastructure/auth/authorizationContext";

// This is sample data.
const data = {
  navMain: [
    {
      title: "Master Data Management",
      url: "/login",
      icon: DatabaseIcon,
      isActive: true,
      items: [
        {
          title: "Room Type Management",
          url: "/portal",
        },
        {
          title: "Room Management",
          url: "#",
        },
        {
          title: "College Management",
          url: "#",
        },
      ],
    } as NavMainItemProp,
    {
      title: "Models",
      url: "/",
      icon: Bot,
      items: [
        {
          title: "Genesis",
          url: "/login",
        },
        {
          title: "Explorer",
          url: "#",
        },
        {
          title: "Quantum",
          url: "#",
        },
      ],
    } as NavMainItemProp,
    {
      title: "Documentation",
      url: "/",
      icon: BookOpen,
      items: [
        {
          title: "Introduction",
          url: "/",
        },
        {
          title: "Get Started",
          url: "#",
        },
        {
          title: "Tutorials",
          url: "#",
        },
        {
          title: "Changelog",
          url: "#",
        },
      ],
    } as NavMainItemProp,
    {
      title: "Settings",
      url: "/",
      icon: Settings2,
      items: [
        {
          title: "General",
          url: "/",
        },
        {
          title: "Team",
          url: "#",
        },
        {
          title: "Billing",
          url: "#",
        },
        {
          title: "Limits",
          url: "#",
        },
      ],
    } as NavMainItemProp,
  ],
};

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  const userContext = useAuthorizationContext();

  return (
    <Sidebar collapsible="icon" {...props}>
      <SidebarHeader>
        {/* <TeamSwitcher teams={data.teams} /> */}
        <SidebarMenuButton
          size="lg"
          className="data-[state=open]:bg-sidebar-accent data-[state=open]:text-sidebar-accent-foreground"
        >
          <div className="bg-sidebar-primary text-sidebar-primary-foreground flex aspect-square size-8 items-center justify-center rounded-lg">
            <GalleryVerticalEnd className="size-4" />
          </div>
          <div className="grid flex-1 text-left text-sm leading-tight">
            <span className="truncate font-medium">Enrollify</span>
            <span className="truncate text-xs">Enterprise</span>
          </div>
        </SidebarMenuButton>
      </SidebarHeader>
      <SidebarContent>
        <NavMain items={data.navMain} />
      </SidebarContent>
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
