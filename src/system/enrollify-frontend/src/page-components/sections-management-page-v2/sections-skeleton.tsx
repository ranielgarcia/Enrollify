import { Skeleton } from "@/components/ui/skeleton";
import { Card, CardContent } from "@/components/ui/card";

function StatPillSkeleton() {
  return (
    <div className="flex items-center gap-2.5 rounded-lg border bg-card px-4 py-3">
      <Skeleton className="size-8 rounded-md" />
      <div className="space-y-1.5">
        <Skeleton className="h-3 w-16" />
        <Skeleton className="h-5 w-8" />
      </div>
    </div>
  );
}

function SectionCardSkeleton() {
  return (
    <Card className="border-l-4 border-l-transparent">
      <CardContent className="p-4 space-y-3">
        {/* Header */}
        <div className="flex items-start justify-between gap-3">
          <div className="flex items-center gap-2.5 flex-1">
            <Skeleton className="size-4 rounded-sm shrink-0" />
            <div className="space-y-1.5 flex-1">
              <div className="flex items-center gap-2">
                <Skeleton className="h-4 w-8" />
                <Skeleton className="h-4 w-24" />
              </div>
              <Skeleton className="h-3 w-48" />
            </div>
          </div>
          <div className="flex items-center gap-2">
            <Skeleton className="h-5 w-16 rounded-full" />
            <Skeleton className="h-5 w-14 rounded-md" />
          </div>
        </div>

        <Skeleton className="h-px w-full" />

        {/* Details */}
        <div className="space-y-2">
          <div className="flex items-center gap-2">
            <Skeleton className="size-3.5 rounded-full" />
            <Skeleton className="h-3 w-12" />
            <Skeleton className="h-3 w-28" />
          </div>
          <div className="space-y-1">
            <div className="flex items-center justify-between">
              <Skeleton className="h-3 w-24" />
              <Skeleton className="h-3 w-20" />
            </div>
            <Skeleton className="h-1.5 w-full rounded-full" />
          </div>
        </div>

        <Skeleton className="h-px w-full" />

        {/* Actions */}
        <div className="flex items-center gap-2">
          <Skeleton className="h-7 w-28 rounded-md" />
          <Skeleton className="h-7 w-16 rounded-md" />
          <Skeleton className="h-7 w-16 rounded-md" />
        </div>
      </CardContent>
    </Card>
  );
}

function CourseGroupSkeleton() {
  return (
    <div className="space-y-2">
      {/* Group header */}
      <div className="flex items-center gap-3 rounded-lg bg-muted/60 px-4 py-3">
        <Skeleton className="size-4 rounded-sm" />
        <div className="flex-1 space-y-2">
          <div className="flex items-center gap-2">
            <Skeleton className="h-4 w-48" />
            <Skeleton className="h-4 w-16 rounded-full" />
          </div>
          <Skeleton className="h-1.5 w-full rounded-full" />
        </div>
      </div>

      {/* Cards grid */}
      <div className="pl-4 grid gap-3 grid-cols-1 xl:grid-cols-2">
        <SectionCardSkeleton />
        <SectionCardSkeleton />
      </div>
    </div>
  );
}

export function SectionsSkeleton() {
  return (
    <div className="flex flex-col gap-6 p-1">
      {/* College selector skeleton */}
      <div className="flex items-center gap-2">
        <Skeleton className="h-4 w-12" />
        <Skeleton className="h-9 w-64 rounded-md" />
      </div>

      {/* Stats bar skeleton */}
      <div className="grid grid-cols-2 gap-2 sm:grid-cols-3 lg:grid-cols-6">
        {Array.from({ length: 6 }).map((_, i) => (
          <StatPillSkeleton key={i} />
        ))}
      </div>

      {/* Toolbar skeleton */}
      <div className="flex items-center justify-between gap-4">
        {/* View toggle */}
        <div className="flex items-center gap-1">
          <Skeleton className="h-8 w-20 rounded-md" />
          <Skeleton className="h-8 w-20 rounded-md" />
        </div>
        {/* Quick filters */}
        <div className="flex items-center gap-1.5">
          <Skeleton className="h-4 w-10" />
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-7 w-16 rounded-md" />
          ))}
        </div>
      </div>

      {/* Course groups */}
      <div className="space-y-3">
        <CourseGroupSkeleton />
        <CourseGroupSkeleton />
      </div>
    </div>
  );
}
