import { OverlayLoader } from "@/components/app-loading-overlay";
import React from "react";

interface ManagementPageLayoutProps {
  title: string | React.ReactNode;
  description: string;
  icon: React.ReactNode;
  isLoading?: boolean;
  createNewItemButton: React.ReactNode;
  children: React.ReactNode;
}

export function ManagementPageLayout({
  title,
  description,
  icon,
  isLoading = false,
  createNewItemButton,
  children,
}: ManagementPageLayoutProps) {
  return (
    <main className="flex min-h-0 min-w-0 flex-1 flex-col px-4 lg:px-6">
      <OverlayLoader isLoading={isLoading} text="Loading" size="sm" />

      <div className="flex items-start justify-between py-3">
        <div className="flex items-start gap-3">
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-surface-sunken text-primary">
            {icon}
          </div>
          <div className="flex flex-col justify-center gap-0.5">
            <h2 className="text-xl font-bold tracking-tight leading-none">
              {title}
            </h2>
            <span className="text-xs text-muted-foreground">{description}</span>
          </div>
        </div>
        <div className="py-1">{createNewItemButton}</div>
      </div>

      <div className="relative my-2">
        <div className="absolute inset-0 flex items-center">
          <div className="w-full border-t border-dashed" />
        </div>
      </div>

      <div className="flex min-h-0 flex-1 flex-col py-5">{children}</div>
    </main>
  );
}
