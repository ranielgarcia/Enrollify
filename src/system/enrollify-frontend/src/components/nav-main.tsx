import { ChevronRight, type LucideIcon } from "lucide-react";

import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import {
  SidebarGroup,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSub,
  SidebarMenuSubButton,
  SidebarMenuSubItem,
} from "@/components/ui/sidebar";
import type { FileRouteTypes } from "@/routeTree.gen";
import { Link, useLocation } from "@tanstack/react-router";
import { cn } from "@/lib/utils";
import type { PolicyName } from "@/infrastructure/authorization/models/PolicyNames";
import { useAuthorization } from "@/infrastructure/authorization/components/useAuthorization";

export interface NavMainItemProp {
  title: string;
  url: FileRouteTypes["to"] & {};
  icon?: LucideIcon;
  isActive?: boolean;
  items?: {
    title: string;
    url: FileRouteTypes["to"] & {};
    params?: Record<string, string>;
    viewAuthorizationPolicies: PolicyName[];
  }[];
}

export interface NavMainProps {
  items: NavMainItemProp[];
}

export function NavMain({ items }: NavMainProps) {
  const location = useLocation();
  const currentPath = location.pathname;
  const { checkPolicy } = useAuthorization();

  // Normalize path by removing trailing slash (except for root "/")
  const normalizePath = (path: string) =>
    path.length > 1 && path.endsWith("/") ? path.slice(0, -1) : path;

  // Check if any sub-item matches the current path (for expanding parent)
  const isParentActive = (item: NavMainItemProp) =>
    item.items?.some(
      (subItem) => normalizePath(currentPath) === normalizePath(subItem.url)
    ) ?? false;

  // Check if a sub-item is the current active route
  const isSubItemActive = (url: string) =>
    normalizePath(currentPath) === normalizePath(url);

  return (
    <SidebarGroup>
      <SidebarGroupLabel>Platform</SidebarGroupLabel>
      <SidebarMenu>
        {items.map((item) => (
          <Collapsible
            key={item.title}
            asChild
            defaultOpen={item.isActive || isParentActive(item)}
            className="group/collapsible"
          >
            <SidebarMenuItem>
              <CollapsibleTrigger asChild>
                <SidebarMenuButton tooltip={item.title}>
                  {item.icon && <item.icon />}
                  <span>{item.title}</span>
                  <ChevronRight className="ml-auto transition-transform duration-200 group-data-[state=open]/collapsible:rotate-90" />
                </SidebarMenuButton>
              </CollapsibleTrigger>
              <CollapsibleContent>
                <SidebarMenuSub>
                  {item.items?.map(async (subItem) => {
                    const canView = !subItem.viewAuthorizationPolicies
                      ? false
                      : await Promise.all(
                          subItem.viewAuthorizationPolicies?.map((policy) =>
                            checkPolicy(policy)
                          )
                        ).then((results) => results.every((res) => res));
                    if (!canView) return null;

                    return (
                      <SidebarMenuSubItem key={subItem.title}>
                        <SidebarMenuSubButton
                          asChild
                          className={cn(
                            "transition-colors",
                            isSubItemActive(subItem.url)
                              ? "bg-sidebar-primary text-sidebar-primary-foreground"
                              : "text-sidebar-foreground hover:bg-sidebar-accent/10"
                          )}
                        >
                          <Link to={subItem.url} params={subItem.params}>
                            <span>{subItem.title}</span>
                          </Link>
                        </SidebarMenuSubButton>
                      </SidebarMenuSubItem>
                    );
                  })}
                </SidebarMenuSub>
              </CollapsibleContent>
            </SidebarMenuItem>
          </Collapsible>
        ))}
      </SidebarMenu>
    </SidebarGroup>
  );
}
