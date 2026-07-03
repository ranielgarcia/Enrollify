import { Suspense, useState } from "react";
import { Link } from "@tanstack/react-router";
import {
  useMutation,
  useQueryClient,
  useSuspenseQuery,
} from "@tanstack/react-query";
import { LayoutGrid, CalendarClock, AlertCircle } from "lucide-react";

import {
  getSectionWithOfferingsOptions,
  openClassSectionOptions,
  lockClassSectionOptions,
  activateClassSectionOptions,
  completeClassSectionOptions,
  cancelClassSectionOptions,
} from "@/api/collections/class-section-collection";
import type { ClassSectionWithOfferings } from "@/api/models/class-scheduling/class-section";
import { ClassSectionStatusEnum } from "@/api/models/class-scheduling/class-section";
import type { ValidationIssue } from "@/api/models/class-scheduling/offering";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Drawer,
  DrawerContent,
  DrawerDescription,
  DrawerHeader,
  DrawerTitle,
} from "@/components/ui/drawer";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

import { ConflictsTab } from "../../section-detail-page/conflicts-tab";
import { OfferingsTab } from "../../section-detail-page/offerings-tab";
import { WeeklyGridTab } from "../../section-detail-page/weekly-grid-tab";
import { SectionStatusBadge } from "../section-status-badge";

// ---------------------------------------------------------------------------
// Public drawer shell
// ---------------------------------------------------------------------------

interface ClassSectionOfferingsDetailDrawerProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  sectionId: number | null;
}

export function ClassSectionOfferingsDetailDrawer({
  isOpen,
  onOpenChange,
  sectionId,
}: ClassSectionOfferingsDetailDrawerProps) {
  return (
    <Drawer direction="right" open={isOpen} onOpenChange={onOpenChange}>
      <DrawerContent className="data-[vaul-drawer-direction=right]:w-[85vw] data-[vaul-drawer-direction=right]:sm:max-w-none flex flex-col h-full overflow-hidden">
        {sectionId !== null && (
          <Suspense fallback={<DrawerLoadingSkeleton />}>
            <SectionDetailDrawerContent
              key={sectionId}
              sectionId={sectionId}
              onClose={() => onOpenChange(false)}
            />
          </Suspense>
        )}
      </DrawerContent>
    </Drawer>
  );
}

// ---------------------------------------------------------------------------
// Loading skeleton shown while Suspense resolves
// ---------------------------------------------------------------------------

function DrawerLoadingSkeleton() {
  return (
    <div className="flex flex-col h-full">
      <div className="border-b px-6 py-4 space-y-2">
        <Skeleton className="h-6 w-48" />
        <Skeleton className="h-4 w-64" />
      </div>
      <div className="flex-1 p-6 space-y-4">
        <Skeleton className="h-8 w-64" />
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-24 w-full rounded-lg" />
        ))}
      </div>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Inner content — mounts only when sectionId is known; uses useSuspenseQuery
// ---------------------------------------------------------------------------

interface SectionDetailDrawerContentProps {
  sectionId: number;
  onClose: () => void;
}

function SectionDetailDrawerContent({
  sectionId,
  onClose,
}: SectionDetailDrawerContentProps) {
  const queryClient = useQueryClient();
  const storageKey = `section-${sectionId}-tab`;

  const [activeTab, setActiveTab] = useState(
    () => sessionStorage.getItem(storageKey) ?? "offerings",
  );

  const { data: sectionData } = useSuspenseQuery(
    getSectionWithOfferingsOptions(sectionId),
  );
  const section = sectionData as unknown as ClassSectionWithOfferings;

  // Re-fetch the card list after any transition so the background view stays fresh
  const invalidateList = () =>
    queryClient.invalidateQueries({ queryKey: ["sections"] });

  const { mutateAsync: openSection, isPending: isOpening } = useMutation(
    openClassSectionOptions(sectionId),
  );
  const { mutateAsync: lockSection, isPending: isLocking } = useMutation(
    lockClassSectionOptions(sectionId),
  );
  const { mutateAsync: activateSection, isPending: isActivating } = useMutation(
    activateClassSectionOptions(sectionId),
  );
  const { mutateAsync: completeSection, isPending: isCompleting } = useMutation(
    completeClassSectionOptions(sectionId),
  );
  const { mutateAsync: cancelSection, isPending: isCancelling } = useMutation(
    cancelClassSectionOptions(sectionId),
  );

  const handleTabChange = (value: string) => {
    setActiveTab(value);
    sessionStorage.setItem(storageKey, value);
  };

  const allValidationIssues: ValidationIssue[] = section.offerings.flatMap(
    (o) => o.validationIssues ?? [],
  );
  const hardValidationIssues = allValidationIssues.filter(
    (c) => c.type.severity === "Error",
  );

  const statusValue = section.status?.value;

  const transitionButtons = (
    <>
      {statusValue === ClassSectionStatusEnum.Draft && (
        <Button
          size="sm"
          variant="default"
          onClick={async () => {
            await openSection({});
            await invalidateList();
          }}
          disabled={isOpening}
        >
          Open for Enrollment
        </Button>
      )}
      {statusValue === ClassSectionStatusEnum.Open && (
        <>
          <Button
            size="sm"
            variant="outline"
            onClick={async () => {
              await lockSection({});
              await invalidateList();
            }}
            disabled={isLocking}
          >
            Lock Enrollment
          </Button>
          <Button
            size="sm"
            variant="destructive"
            onClick={async () => {
              await cancelSection({});
              await invalidateList();
              onClose();
            }}
            disabled={isCancelling}
          >
            Cancel Section
          </Button>
        </>
      )}
      {statusValue === ClassSectionStatusEnum.Locked && (
        <Button
          size="sm"
          variant="default"
          onClick={async () => {
            await activateSection({});
            await invalidateList();
          }}
          disabled={isActivating}
        >
          Activate
        </Button>
      )}
      {statusValue === ClassSectionStatusEnum.Active && (
        <Button
          size="sm"
          variant="outline"
          onClick={async () => {
            await completeSection({});
            await invalidateList();
          }}
          disabled={isCompleting}
        >
          Complete
        </Button>
      )}
    </>
  );

  return (
    <div className="flex flex-col h-full overflow-hidden">
      <DrawerHeader className="border-b pb-4 shrink-0">
        <DrawerTitle className="flex items-center gap-2 flex-wrap">
          {`${section.name}-${section.intendedYearLevel}${section.sectionCode ?? ""}`}
          {section.status && <SectionStatusBadge status={section.status} />}
        </DrawerTitle>
        <DrawerDescription>
          {section.course.name} · {section.academicTerm.termName} · Year{" "}
          {section.intendedYearLevel}
        </DrawerDescription>
        <div className="flex gap-2 flex-wrap">
          {transitionButtons}
          <Link
            to="/portal/curriculum-and-scheduling/sections-details/$sectionId"
            params={{ sectionId: section.id.toString() }}
            target="_blank"
            className="inline-flex items-center justify-center rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 h-9 px-3 text-primary underline-offset-4 hover:underline"
          >
            Open In New Tab
          </Link>
        </div>
      </DrawerHeader>

      <div className="flex-1 overflow-y-auto px-6 py-4">
        <Tabs
          value={activeTab}
          onValueChange={handleTabChange}
          className="flex flex-col space-y-4"
        >
          <TabsList variant="line">
            <TabsTrigger value="offerings" className="gap-2">
              <LayoutGrid className="size-4" />
              Offerings
              {section.offerings.length > 0 && (
                <Badge className="text-[11px] px-1.5 py-0 font-semibold bg-muted text-muted-foreground border hover:bg-muted h-4">
                  {section.offerings.length}
                </Badge>
              )}
            </TabsTrigger>

            <TabsTrigger value="grid" className="gap-2">
              <CalendarClock className="size-4" />
              Weekly Grid
            </TabsTrigger>

            <TabsTrigger value="conflicts" className="gap-2">
              <AlertCircle className="size-4" />
              Conflicts
              {allValidationIssues.length > 0 && (
                <Badge
                  className={
                    hardValidationIssues.length > 0
                      ? "text-[11px] px-1.5 py-0 font-semibold bg-red-500/15 text-red-600 border border-red-500/25 hover:bg-red-500/15 h-4"
                      : "text-[11px] px-1.5 py-0 font-semibold bg-amber-500/15 text-amber-600 border border-amber-500/25 hover:bg-amber-500/15 h-4"
                  }
                >
                  {allValidationIssues.length}
                </Badge>
              )}
            </TabsTrigger>
          </TabsList>

          <TabsContent value="offerings" className="space-y-4">
            <OfferingsTab
              sectionId={sectionId}
              offerings={section.offerings}
              validationMessages={section.validationMessages ?? undefined}
            />
          </TabsContent>

          <TabsContent value="grid" className="space-y-4">
            <WeeklyGridTab offerings={section.offerings} />
          </TabsContent>

          <TabsContent value="conflicts" className="space-y-4">
            <ConflictsTab offerings={section.offerings} />
          </TabsContent>
        </Tabs>
      </div>
    </div>
  );
}
