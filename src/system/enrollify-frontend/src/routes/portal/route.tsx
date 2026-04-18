import { PageErrorBoundary } from "@/components/page-layouts/page-error-boundary";
import { OverlayLoader } from "@/components/app-loading-overlay";
import { AppSidebar } from "@/components/navigation/app-sidebar";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import {
  SidebarInset,
  SidebarProvider,
  SidebarTrigger,
} from "@/components/ui/sidebar";
import { Toaster } from "@/components/ui/sonner";
import { AuthenticationProvider } from "@/infrastructure/authentication/authentication-provider";
import { AuthorizationProvider } from "@/infrastructure/authorization/AuthorizationProvider";
import { SystemSettingsProvider } from "@/infrastructure/system-settings/system-settings-provider";
import type { RouteLoaderData } from "@/types/route.types";
import { Separator } from "@radix-ui/react-separator";
import {
  createFileRoute,
  Link,
  Outlet,
  redirect,
  useMatches,
} from "@tanstack/react-router";
import React, { Suspense } from "react";

export const Route = createFileRoute("/portal")({
  beforeLoad: async ({ context: { msal }, location }) => {
    const activeAccount = msal?.instance.getActiveAccount();
    if (!activeAccount) {
      throw redirect({
        to: "/login",
        search: {
          redirect: location.href,
        },
      });
    } else if (location.pathname === "/portal") {
      throw redirect({
        to: "/portal/home",
      });
    }
  },
  loader: (): RouteLoaderData => ({
    crumb: undefined,
  }),
  component: RouteComponent,
  errorComponent: PageErrorBoundary,
});

function RouteComponent() {
  const matches = useMatches();

  const items = matches
    .filter(
      (match): match is typeof match & { loaderData: { crumb: string } } =>
        "loaderData" in match &&
        match.loaderData !== null &&
        typeof match.loaderData === "object" &&
        "crumb" in match.loaderData &&
        typeof match.loaderData.crumb === "string",
    )
    .map(({ pathname, loaderData }) => ({
      href: pathname,
      label: loaderData.crumb,
    }));

  return (
    <AuthenticationProvider>
      <AuthorizationProvider>
        <SystemSettingsProvider>
          <SidebarProvider className="h-svh overflow-hidden">
            <AppSidebar />
            <SidebarInset>
              <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12 bg-sidebar">
                <div className="flex items-center gap-2 px-4">
                  <SidebarTrigger className="-ml-1" />
                  <Separator
                    orientation="vertical"
                    className="mr-2 data-[orientation=vertical]:h-4"
                  />
                  <Breadcrumb>
                    <BreadcrumbList>
                      <BreadcrumbItem>
                        <BreadcrumbLink asChild>
                          <Link to="/portal/home">Home</Link>
                        </BreadcrumbLink>
                      </BreadcrumbItem>
                      {items.length > 0 && <BreadcrumbSeparator />}
                      {items.map((item, index) => (
                        <React.Fragment key={index}>
                          <BreadcrumbItem
                            key={index}
                            className="hidden md:block"
                          >
                            <Link to={item.href} className="breadcrumb-link">
                              {item.label}
                            </Link>
                          </BreadcrumbItem>
                          {index < items.length - 1 && (
                            <BreadcrumbSeparator className="hidden md:block" />
                          )}
                        </React.Fragment>
                      ))}
                    </BreadcrumbList>
                  </Breadcrumb>
                </div>
              </header>
              <Suspense
                fallback={
                  <OverlayLoader
                    isLoading={true}
                    text="Loading data..."
                    size="lg"
                  />
                }
              >
                <Outlet />
              </Suspense>

              <Toaster position="bottom-right" />
            </SidebarInset>
          </SidebarProvider>
        </SystemSettingsProvider>
      </AuthorizationProvider>
    </AuthenticationProvider>
  );
}
