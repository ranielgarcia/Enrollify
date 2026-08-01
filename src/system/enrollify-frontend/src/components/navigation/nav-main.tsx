import { ChevronRight, type LucideIcon } from "lucide-react";
import { useEffect, useState, useMemo, useCallback, useRef } from "react";

import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import {
  SidebarGroup,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSub,
  SidebarMenuSubButton,
  SidebarMenuSubItem,
  useSidebar,
} from "@/components/ui/sidebar";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import type { FileRouteTypes } from "@/routeTree.gen";
import { Link, useLocation } from "@tanstack/react-router";
import { cn } from "@/lib/utils";
import type { PolicyName } from "@/infrastructure/authorization/models/PolicyNames";
import { useAuthorization } from "@/infrastructure/authorization/components/useAuthorization";
import { useEnrollmentContext } from "@/contexts/enrollment-context/enrollment-context";

interface SubItemProps {
  title: string;
  url: FileRouteTypes["to"] & {};
  params?: Record<string, string>;
  viewAuthorizationPolicies: PolicyName[];
  icon?: LucideIcon;
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
  const { selectedAcademicYearSlug } = useEnrollmentContext();

  useEffect(() => {
    let isMounted = true;

    const checkAuthorization = async () => {
      if (!subItem.viewAuthorizationPolicies?.length) {
        if (isMounted) setCanView(false);
        return;
      }

      const results = await Promise.all(
        subItem.viewAuthorizationPolicies.map((policy) => checkPolicy(policy)),
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
          "transition-all duration-200 rounded-md",
          isActive
            ? "bg-sidebar-primary/10 text-sidebar-primary font-medium border-l-2 border-sidebar-primary/70"
            : "text-sidebar-foreground/65 hover:text-sidebar-foreground hover:bg-sidebar-accent/50",
        )}
      >
        <Link
          to={subItem.url}
          params={subItem.params}
          search={{ academicYear: selectedAcademicYearSlug }}
        >
          {subItem.icon && (
            <subItem.icon
              className={cn(
                "size-4 shrink-0 transition-colors duration-200",
                isActive
                  ? "text-sidebar-primary"
                  : "text-sidebar-foreground/50",
              )}
            />
          )}
          <span className="text-[13px] tracking-wide">{subItem.title}</span>
        </Link>
      </SidebarMenuSubButton>
    </SidebarMenuSubItem>
  );
}

// Sub-item rendered inside the collapsed flyout popover
function NavFlyoutSubItem({
  subItem,
  isActive,
  onNavigate,
}: {
  subItem: SubItemProps;
  isActive: boolean;
  onNavigate: () => void;
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
        subItem.viewAuthorizationPolicies.map((policy) => checkPolicy(policy)),
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

  if (canView === null || !canView) return null;

  return (
    <Link
      to={subItem.url}
      params={subItem.params}
      onClick={onNavigate}
      className={cn(
        "flex items-center gap-2.5 rounded-md px-3 py-2 text-[13px] transition-colors duration-150",
        isActive
          ? "bg-sidebar-primary/10 text-sidebar-primary font-medium"
          : "text-popover-foreground/70 hover:bg-accent hover:text-accent-foreground",
      )}
    >
      {subItem.icon && (
        <subItem.icon
          className={cn(
            "size-4 shrink-0",
            isActive ? "text-sidebar-primary" : "text-muted-foreground",
          )}
        />
      )}
      <span>{subItem.title}</span>
    </Link>
  );
}

// Flyout menu shown when hovering an icon in collapsed sidebar
function NavCollapsedFlyout({
  item,
  parentActive,
  isSubItemActive,
}: {
  item: NavMainItemProp;
  parentActive: boolean;
  isSubItemActive: (url: string) => boolean;
}) {
  const [isOpen, setIsOpen] = useState(false);
  const timeoutRef = useRef<ReturnType<typeof setTimeout>>(undefined);

  const handleOpen = useCallback(() => {
    clearTimeout(timeoutRef.current);
    setIsOpen(true);
  }, []);

  const handleClose = useCallback(() => {
    timeoutRef.current = setTimeout(() => setIsOpen(false), 150);
  }, []);

  useEffect(() => () => clearTimeout(timeoutRef.current), []);

  return (
    <SidebarMenuItem onMouseEnter={handleOpen} onMouseLeave={handleClose}>
      <Popover
        open={isOpen}
        onOpenChange={(open) => {
          if (!open) setIsOpen(false);
        }}
      >
        <PopoverTrigger asChild>
          <SidebarMenuButton
            className={cn(
              "rounded-lg transition-all duration-200 h-9",
              parentActive
                ? "bg-sidebar-primary/10 text-sidebar-primary font-medium"
                : "hover:bg-sidebar-accent/50",
            )}
          >
            {item.icon && (
              <item.icon
                className={cn(
                  "size-[18px] shrink-0 transition-colors duration-200",
                  parentActive
                    ? "text-sidebar-primary"
                    : "text-sidebar-foreground/60",
                )}
              />
            )}
            <span className="text-[13.5px] font-medium tracking-wide">
              {item.title}
            </span>
          </SidebarMenuButton>
        </PopoverTrigger>
        <PopoverContent
          side="right"
          align="start"
          sideOffset={6}
          className="w-56 p-1.5 rounded-xl shadow-xl"
          onMouseEnter={handleOpen}
          onMouseLeave={handleClose}
          onOpenAutoFocus={(e) => e.preventDefault()}
          onCloseAutoFocus={(e) => e.preventDefault()}
        >
          <p className="px-3 pt-1.5 pb-1 text-[11px] font-semibold uppercase tracking-widest text-muted-foreground">
            {item.title}
          </p>
          <div className="space-y-0.5">
            {item.items?.map((subItem) => (
              <NavFlyoutSubItem
                key={subItem.title}
                subItem={subItem}
                isActive={isSubItemActive(subItem.url)}
                onNavigate={() => setIsOpen(false)}
              />
            ))}
          </div>
        </PopoverContent>
      </Popover>
    </SidebarMenuItem>
  );
}

export function NavMain({ items }: NavMainProps) {
  const { state } = useSidebar();
  const isCollapsed = state === "collapsed";
  const location = useLocation();
  const currentPath = location.pathname;

  // Memoize the normalize function to prevent recreation on each render
  const normalizePath = useCallback(
    (path: string) =>
      path.length > 1 && path.endsWith("/") ? path.slice(0, -1) : path,
    [],
  );

  // Get base path without parameter placeholders (e.g., "/path/{-$param}" -> "/path")
  const getBasePath = useCallback((path: string) => {
    const paramIndex = path.indexOf("/{-$");
    return paramIndex !== -1 ? path.slice(0, paramIndex) : path;
  }, []);

  // Memoize normalized current path
  const normalizedCurrentPath = useMemo(
    () => normalizePath(currentPath),
    [currentPath, normalizePath],
  );

  // Check if a path matches (exact match or parameterized route match)
  const pathMatches = useCallback(
    (routeUrl: string) => {
      const normalizedRoute = normalizePath(routeUrl);
      // Exact match
      if (normalizedCurrentPath === normalizedRoute) return true;
      // For parameterized routes, check if current path starts with base path
      const basePath = getBasePath(normalizedRoute);
      if (basePath !== normalizedRoute) {
        return normalizedCurrentPath.startsWith(basePath);
      }
      return false;
    },
    [normalizedCurrentPath, normalizePath, getBasePath],
  );

  // Check if any sub-item matches the current path (for expanding parent)
  const isParentActive = useCallback(
    (item: NavMainItemProp) =>
      item.items?.some((subItem) => pathMatches(subItem.url)) ?? false,
    [pathMatches],
  );

  // Check if a sub-item is the current active route
  const isSubItemActive = useCallback(
    (url: string) => pathMatches(url),
    [pathMatches],
  );

  return (
    <SidebarGroup className="px-3 py-2">
      <SidebarMenu className="gap-1.5">
        {items.map((item) => {
          const parentActive = isParentActive(item);

          if (isCollapsed) {
            return (
              <NavCollapsedFlyout
                key={item.title}
                item={item}
                parentActive={parentActive}
                isSubItemActive={isSubItemActive}
              />
            );
          }

          return (
            <Collapsible
              key={item.title}
              asChild
              defaultOpen={item.isActive || parentActive}
              className="group/collapsible"
            >
              <SidebarMenuItem>
                <CollapsibleTrigger asChild>
                  <SidebarMenuButton
                    tooltip={item.title}
                    className={cn(
                      "rounded-lg transition-all duration-200 h-9",
                      parentActive
                        ? "bg-sidebar-primary/10 text-sidebar-primary font-medium"
                        : "hover:bg-sidebar-accent/50",
                    )}
                  >
                    {item.icon && (
                      <item.icon
                        className={cn(
                          "size-[18px] shrink-0 transition-colors duration-200",
                          parentActive
                            ? "text-sidebar-primary"
                            : "text-sidebar-foreground/60",
                        )}
                      />
                    )}
                    <span className="text-[13.5px] font-medium tracking-wide">
                      {item.title}
                    </span>
                    <ChevronRight className="ml-auto size-4 text-sidebar-foreground/40 transition-transform duration-200 group-data-[state=open]/collapsible:rotate-90" />
                  </SidebarMenuButton>
                </CollapsibleTrigger>
                <CollapsibleContent>
                  <SidebarMenuSub className="ml-4 border-l border-sidebar-border/30 pl-3 py-1 mt-0.5">
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
          );
        })}
      </SidebarMenu>
    </SidebarGroup>
  );
}
