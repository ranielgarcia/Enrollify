import { Skeleton } from "@/components/ui/skeleton";
import { Card } from "@/components/ui/card";

export function SectionsSkeleton() {
  return (
    <div className="flex flex-col space-y-6 p-6">
      {/* College Selector Skeleton */}
      <div className="flex items-center gap-2">
        <Skeleton className="h-10 w-32" />
        <Skeleton className="h-10 w-64" />
      </div>

      {/* Stats Panel Skeleton */}
      <Card className="p-6">
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6">
          {Array(6)
            .fill(0)
            .map((_, i) => (
              <div key={i} className="space-y-2">
                <Skeleton className="h-10 w-full" />
                <Skeleton className="h-8 w-12" />
              </div>
            ))}
        </div>
      </Card>

      {/* Table Skeleton */}
      <Card className="overflow-hidden">
        <div className="border-b px-4 py-3">
          <Skeleton className="h-10 w-full" />
        </div>
        {Array(5)
          .fill(0)
          .map((_, i) => (
            <div key={i} className="border-b px-4 py-3">
              <Skeleton className="h-8 w-full" />
            </div>
          ))}
      </Card>
    </div>
  );
}
