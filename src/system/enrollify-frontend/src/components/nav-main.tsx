import { ChevronRight, type LucideIcon } from "lucide-react";
import { useEffect, useState, useMemo, useCallback } from "react";

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

interface SubItemProps {
  title: string;
  url: FileRouteTypes["to"] & {};
  params?: Record<string, string>;
  viewAuthorizationPolicies: PolicyName[];
}

export interface NavMainItemProp {
  title: string;
  url: FileRouteTypes["to"] & {};
  icon?: LucideIcon;
  isActive?: boolean;
  items?: SubItemProps[];
}

export interface NavMainProps {
  items: NavMainItemProp[];
}

// Component to handle async authorization check for a single sub-item
function NavSubItem({
  subItem,
  isActive,
}: {
  subItem: SubItemProps;
  isActive: boolean;
}) {
  const { checkPolicy } = useAuthorization();
  const [canView, setCanView] = useState<boolean | null>(null);

  useEffect(() => {
    let isMounted = true;

    const checkAuthorization = async () => {
      if (!subItem.viewAuthorizationPolicies?.length) {
        if (isMounted) setCanView(false);
        return;
      }

      const results = await Promise.all(
        subItem.viewAuthorizationPolicies.map((policy) => checkPolicy(policy))
      );
      if (isMounted) {
        setCanView(results.every((res) => res));
      }
    };

    checkAuthorization();

    return () => {
      isMounted = false;
    };
  }, [subItem.viewAuthorizationPolicies, checkPolicy]);

  // Don't render until authorization check is complete
  if (canView === null || !canView) return null;

  return (
    <SidebarMenuSubItem>
      <SidebarMenuSubButton
        asChild
        className={cn(
          "transition-colors",
          isActive
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
}

export function NavMain({ items }: NavMainProps) {
  const location = useLocation();
  const currentPath = location.pathname;

  // Memoize the normalize function to prevent recreation on each render
  const normalizePath = useCallback(
    (path: string) =>
      path.length > 1 && path.endsWith("/") ? path.slice(0, -1) : path,
    []
  );

  // Memoize normalized current path
  const normalizedCurrentPath = useMemo(
    () => normalizePath(currentPath),
    [currentPath, normalizePath]
  );

  // Check if any sub-item matches the current path (for expanding parent)
  const isParentActive = useCallback(
    (item: NavMainItemProp) =>
      item.items?.some(
        (subItem) => normalizedCurrentPath === normalizePath(subItem.url)
      ) ?? false,
    [normalizedCurrentPath, normalizePath]
  );

  // Check if a sub-item is the current active route
  const isSubItemActive = useCallback(
    (url: string) => normalizedCurrentPath === normalizePath(url),
    [normalizedCurrentPath, normalizePath]
  );

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
                  {item.items?.map((subItem) => (
                    <NavSubItem
                      key={subItem.title}
                      subItem={subItem}
                      isActive={isSubItemActive(subItem.url)}
                    />
                  ))}
                </SidebarMenuSub>
              </CollapsibleContent>
            </SidebarMenuItem>
          </Collapsible>
        ))}
      </SidebarMenu>
    </SidebarGroup>
  );
}
