import { OverlayLoader } from "@/components/app-loading-overlay";

export default function CollegesPage() {
  return (
    <main>
      <OverlayLoader isLoading={false} text="Loading" size="sm" />

      <div className="p-4 md:p-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-foreground mb-2">
            College Management
          </h1>
          <p className="text-muted-foreground">Manage college resources</p>
        </div>
      </div>
    </main>
  );
}
