import { OverlayLoader } from "@/components/app-loading-overlay";
import React from "react";
import { Separator } from "@/components/ui/separator";

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

      <div className="flex items-start justify-between py-2">
        <div className="flex items-start gap-3">
          <div className="flex h-12 w-12 items-center justify-center">
            {icon}
          </div>
          <div className="flex flex-col">
            <h2 className="text-2xl font-bold text-slate-900">{title}</h2>
            <span className="text-sm font-medium text-slate-500">
              {description}
            </span>
          </div>
        </div>
        <div className="py-2">{createNewItemButton}</div>
      </div>
      <Separator />

      <div className="flex min-h-0 flex-1 flex-col py-5">{children}</div>
    </main>
  );
}
