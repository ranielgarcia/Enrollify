import { OverlayLoader } from "@/components/app-loading-overlay";

interface ManagementPageLayoutProps {
  title: string;
  description: string;
  isLoading?: boolean;
  children: React.ReactNode;
}

export function ManagementPageLayout({
  title,
  description,
  isLoading = false,
  children,
}: ManagementPageLayoutProps) {
  return (
    <main>
      <OverlayLoader isLoading={isLoading} text="Loading" size="sm" />
      <div className="p-4 md:p-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-foreground mb-2">{title}</h1>
          <p className="text-muted-foreground">{description}</p>
        </div>
        {children}
      </div>
    </main>
  );
}
