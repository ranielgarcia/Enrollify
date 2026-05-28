import { useState } from "react";
import { getSectionWithOfferingsOptions } from "@/api/collections/class-section-collection";
import {
  openClassSectionOptions,
  lockClassSectionOptions,
  activateClassSectionOptions,
  completeClassSectionOptions,
  cancelClassSectionOptions,
} from "@/api/collections/class-section-collection";
import { useSuspenseQuery, useMutation } from "@tanstack/react-query";
import { ManagementPageLayout } from "@/components/page-layouts/management-page-layout";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ModuleIcons } from "@/config/module-icons";
import { LayoutGrid, CalendarClock, AlertCircle } from "lucide-react";
import { OfferingsTab } from "./offerings-tab";
import { WeeklyGridTab } from "./weekly-grid-tab";
import { ConflictsTab } from "./conflicts-tab";
import type { ConflictResult } from "@/api/models/offering";
import type { ClassSectionWithOfferings } from "@/api/models/class-section";
import { ClassSectionStatusEnum } from "@/api/models/class-section";
import { SectionStatusBadge } from "../sections-management-page/section-status-badge";

interface SectionDetailPageProps {
  sectionId: number;
}

export default function SectionDetailPage({
  sectionId,
}: SectionDetailPageProps) {
  const storageKey = `section-${sectionId}-tab`;
  const [activeTab, setActiveTab] = useState(
    () => sessionStorage.getItem(storageKey) ?? "offerings",
  );

  const { data: sectionData } = useSuspenseQuery(
    getSectionWithOfferingsOptions(sectionId),
  );
  const section = sectionData as unknown as ClassSectionWithOfferings;

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

  const allConflicts: ConflictResult[] = section.offerings.flatMap(
    (o) => o.conflicts ?? [],
  );
  const hardConflictCount = allConflicts.filter(
    (c) => c.severity === "error",
  ).length;

  const statusValue = section.statusId?.value;

  const transitionButtons = (
    <div className="flex gap-2 flex-wrap">
      {statusValue === ClassSectionStatusEnum.Draft && (
        <Button
          size="sm"
          variant="default"
          onClick={() => openSection({})}
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
            onClick={() => lockSection({})}
            disabled={isLocking}
          >
            Lock Enrollment
          </Button>
          <Button
            size="sm"
            variant="destructive"
            onClick={() => cancelSection({})}
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
          onClick={() => activateSection({})}
          disabled={isActivating}
        >
          Activate
        </Button>
      )}
      {statusValue === ClassSectionStatusEnum.Active && (
        <Button
          size="sm"
          variant="outline"
          onClick={() => completeSection({})}
          disabled={isCompleting}
        >
          Complete
        </Button>
      )}
    </div>
  );

  return (
    <ManagementPageLayout
      title={
        <span className="flex items-center gap-2">
          {section.name}
          {section.statusId && (
            <SectionStatusBadge status={section.statusId} />
          )}
        </span>
      }
      description={`${section.course.name} · ${section.academicTerm.termName} · Year ${section.intendedYearLevel}`}
      icon={<ModuleIcons.sections />}
      createNewItemButton={transitionButtons}
    >
      <Tabs
        value={activeTab}
        onValueChange={handleTabChange}
        className="min-h-0 flex-1 space-y-6"
      >
        <TabsList variant="line">
          <TabsTrigger value="offerings" className="gap-2">
            <LayoutGrid className="size-4" />
            Offerings
            {section.offerings.length > 0 && (
              <Badge variant="secondary" className="text-xs h-4 px-1">
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
            {allConflicts.length > 0 && (
              <Badge
                variant={hardConflictCount > 0 ? "destructive" : "secondary"}
                className="text-xs h-4 px-1"
              >
                {allConflicts.length}
              </Badge>
            )}
          </TabsTrigger>
        </TabsList>

        <TabsContent
          value="offerings"
          className="flex min-h-0 flex-col space-y-4"
        >
          <OfferingsTab sectionId={sectionId} offerings={section.offerings} />
        </TabsContent>

        <TabsContent value="grid" className="flex min-h-0 flex-col space-y-4">
          <WeeklyGridTab offerings={section.offerings} />
        </TabsContent>

        <TabsContent
          value="conflicts"
          className="flex min-h-0 flex-col space-y-4"
        >
          <ConflictsTab conflicts={allConflicts} />
        </TabsContent>
      </Tabs>
    </ManagementPageLayout>
  );
}
