import { useState } from "react";
import { getSectionWithOfferingsOptions } from "@/api/collections/class-section-collection";
import { useSuspenseQuery } from "@tanstack/react-query";
import { ManagementPageLayout } from "@/components/page-layouts/management-page-layout";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Badge } from "@/components/ui/badge";
import { ModuleIcons } from "@/config/module-icons";
import { LayoutGrid, CalendarClock, AlertCircle } from "lucide-react";
import { OfferingsTab } from "./offerings-tab";
import { WeeklyGridTab } from "./weekly-grid-tab";
import { ConflictsTab } from "./conflicts-tab";
import type { ConflictResult } from "@/api/models/offering";
import type { ClassSectionWithOfferings } from "@/api/models/class-section";

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

  return (
    <ManagementPageLayout
      title={section.name}
      description={`${section.course.name} · ${section.academicTerm.name} · Year ${section.yearLevel}`}
      icon={<ModuleIcons.sections />}
      createNewItemButton={null}
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
