import { Suspense, useCallback, useEffect, useState } from "react";
import {
  QueryErrorResetBoundary,
  useMutation,
  useQuery,
} from "@tanstack/react-query";
import { useQueryStates } from "nuqs";

import {
  bulkCancelSectionsMutationOptions,
  bulkOpenSectionsMutationOptions,
  getClassSectionsStatsOptions,
  getCollegeCoursesWithClassSectionsForSchedulingOptions,
} from "@/api/collections/class-section-collection";
import type { ClassSectionMinimal } from "@/api/models/class-scheduling/class-section";

import { ManagementPageLayout } from "@/components/page-layouts/management-page-layout";
import { ModuleIcons } from "@/config/module-icons";
import { useEnrollmentContext } from "@/contexts/enrollment-context/enrollment-context";
import { useCrudState } from "@/hooks/use-crud-state";
import type { Course } from "@/api/models/course";

import { BulkAdviserAssignDrawer } from "./bulk-adviser-assign-drawer";
import { BulkStatusTransitionDialog } from "./bulk-status-transition-dialog";
import { CancelSectionAlertDialog } from "./cancel-section-alert-dialog";
import { SectionsErrorBoundary } from "./components/sections-error-boundary";
import { ConflictPreviewDrawer } from "./validation-issues-preview-drawer";
import { EmptyState } from "./empty-state";
import { SectionFormDrawer } from "./section-form-drawer";
import { SectionsCardView } from "./sections-card-view";
import { SectionsContextBar } from "./sections-context-bar";
import { SectionsSkeleton } from "./sections-skeleton";
import { SectionsTable } from "./sections-table";
import { SectionStatsStrip } from "./stats-bar";
import { useSectionsDialogs } from "./hooks/use-sections-dialogs";
import { useSectionSelection } from "./hooks/use-section-selection";
import { searchParams, type QuickFilter } from "./searchParams";
import { BulkInitializeSectionsDrawer } from "./bulk-initialize-sections-drawer";
import { FloatingSelectionToolbar } from "./floating-selection-toolbar";
import { ClassSectionOfferingsDetailDrawer } from "./class-section-offerings-detail-drawer";

export default function SectionsManagementPageV2() {
  const [isBulkInitializeOpen, setIsBulkInitializeOpen] = useState(false);
  const [courses, setCourses] = useState<Course[]>([]);

  return (
    <ManagementPageLayout
      title="Class Sections"
      description="Plan, schedule, and open class sections for the active term."
      icon={<ModuleIcons.sections className="size-6 text-primary" />}
      createNewItemButton={
        <div className="flex gap-2">
          <BulkInitializeSectionsDrawer
            isOpen={isBulkInitializeOpen}
            setIsOpen={setIsBulkInitializeOpen}
            onOpenChange={setIsBulkInitializeOpen}
            courses={courses ?? []}
          />
        </div>
      }
    >
      <QueryErrorResetBoundary>
        {({ reset }) => (
          <SectionsErrorBoundary onReset={reset}>
            <Suspense fallback={<SectionsSkeleton />}>
              <SectionsPageContent setCourses={setCourses} />
            </Suspense>
          </SectionsErrorBoundary>
        )}
      </QueryErrorResetBoundary>
    </ManagementPageLayout>
  );
}

interface SectionsPageContentProps {
  setCourses: (courses: Course[]) => void;
}
function SectionsPageContent({ setCourses }: SectionsPageContentProps) {
  const [{ collegeId, view, quickFilter }, setParams] =
    useQueryStates(searchParams);
  const { selectedAcademicTerm, selectedAcademicYear } = useEnrollmentContext();
  const dialogs = useSectionsDialogs();
  const selection = useSectionSelection();
  const [collegeSelectorOpen, setCollegeSelectorOpen] = useState(false);

  const noCollegeSelected = !collegeId;
  const hasTerm = !!selectedAcademicTerm;

  const { data: collegeData } = useQuery(
    getCollegeCoursesWithClassSectionsForSchedulingOptions(
      collegeId ?? undefined,
      selectedAcademicTerm?.id,
    ),
  );

  const { data: stats } = useQuery(
    getClassSectionsStatsOptions(
      collegeId ?? undefined,
      selectedAcademicTerm?.id,
    ),
  );

  useEffect(() => {
    if (collegeData?.coursesWithClassSections) {
      const courses = collegeData?.coursesWithClassSections.map((x) => ({
        id: x.id,
        code: x.code,
        name: x.name,
      })) as Course[];

      setCourses(courses);
    }
  }, [collegeData, setCourses]);

  const {
    isFormOpen,
    entityToEdit,
    entityToDelete: sectionToCancel,
    handleEdit,
    handleDelete: handleCancelSection,
    handleFormOpenChange,
    handleDeleteDialogOpenChange: handleCancelDialogOpenChange,
  } = useCrudState<ClassSectionMinimal>();

  const { mutateAsync: bulkOpen, isPending: isBulkOpenPending } = useMutation(
    bulkOpenSectionsMutationOptions(),
  );
  const { mutateAsync: bulkCancel, isPending: isBulkCancelPending } =
    useMutation(bulkCancelSectionsMutationOptions());

  const handleBulkTransitionConfirm = useCallback(async () => {
    if (dialogs.state.type === "bulk-open") {
      await bulkOpen({ sectionIds: [...dialogs.state.sectionIds] });
    } else if (dialogs.state.type === "bulk-cancel") {
      await bulkCancel({ sectionIds: [...dialogs.state.sectionIds] });
    }
    dialogs.close();
  }, [dialogs, bulkOpen, bulkCancel]);

  const handleViewDetails = useCallback(
    (section: ClassSectionMinimal) => {
      dialogs.openViewSectionDetails(section.id);
    },
    [dialogs],
  );

  const handleViewConflicts = useCallback(
    (section: ClassSectionMinimal) =>
      dialogs.openViewConflicts(section.id, section.name),
    [dialogs],
  );

  const handleQuickFilterChange = useCallback(
    (filter: QuickFilter) => {
      setParams({ quickFilter: filter });
    },
    [setParams],
  );

  const handleClearFilters = useCallback(() => {
    setParams({ quickFilter: "all" });
  }, [setParams]);

  const handleViewChange = useCallback(
    (next: "card" | "table") => {
      setParams({ view: next });
    },
    [setParams],
  );

  const handleBulkOpen = useCallback(
    (sectionIds: number[]) => dialogs.openBulkOpen(sectionIds),
    [dialogs],
  );
  const handleBulkCancel = useCallback(
    (sectionIds: number[]) => dialogs.openBulkCancel(sectionIds),
    [dialogs],
  );
  const handleBulkAssignAdviser = useCallback(
    (sectionIds: number[]) => dialogs.openBulkAssignAdviser(sectionIds),
    [dialogs],
  );

  const coursesWithSections = collegeData?.coursesWithClassSections ?? [];
  const collegeName = collegeData?.name ?? "";
  const showContent = !noCollegeSelected && hasTerm;
  const currentView = view === "table" ? "table" : "card";

  const bulkTransitionAction =
    dialogs.state.type === "bulk-open"
      ? "open"
      : dialogs.state.type === "bulk-cancel"
        ? "cancel"
        : null;
  const bulkTransitionCount =
    dialogs.state.type === "bulk-open" || dialogs.state.type === "bulk-cancel"
      ? dialogs.state.sectionIds.length
      : 0;

  return (
    <div className="flex flex-col gap-4">
      <SectionsContextBar
        view={currentView}
        onViewChange={handleViewChange}
        collegeSelectorOpen={collegeSelectorOpen}
        onCollegeSelectorOpenChange={setCollegeSelectorOpen}
        academicTerm={
          selectedAcademicTerm
            ? {
                termName: selectedAcademicTerm.termName,
                termNumber: selectedAcademicTerm.termNumber,
                academicYearTitle: selectedAcademicYear?.academicYearTitle,
              }
            : null
        }
      />

      {noCollegeSelected && (
        <EmptyState
          variant="no-college"
          onOpenCollegeSelector={() => setCollegeSelectorOpen(true)}
        />
      )}

      {!noCollegeSelected && hasTerm && (
        <SectionStatsStrip
          stats={stats ?? {}}
          activeFilter={quickFilter as QuickFilter}
          onFilterChange={handleQuickFilterChange}
        />
      )}

      {showContent && (
        <>
          {currentView === "card" && (
            <SectionsCardView
              coursesWithSections={coursesWithSections}
              collegeName={collegeName}
              quickFilter={quickFilter as QuickFilter}
              selection={selection}
              onClearFilters={handleClearFilters}
              onCancelSection={handleCancelSection}
              onViewDetails={handleViewDetails}
              onChangeAdviser={handleEdit}
              onViewConflicts={handleViewConflicts}
            />
          )}

          {currentView === "table" && (
            <SectionsTable
              coursesWithSections={coursesWithSections}
              quickFilter={quickFilter as QuickFilter}
              selection={selection}
              onClearFilters={handleClearFilters}
              onCancelSection={handleCancelSection}
              onViewDetails={handleViewDetails}
              onViewConflicts={handleViewConflicts}
            />
          )}
        </>
      )}

      <SectionFormDrawer
        key={entityToEdit?.id ?? "new"}
        isOpen={isFormOpen}
        onOpenChange={handleFormOpenChange}
        sectionToUpdate={entityToEdit}
      />

      <CancelSectionAlertDialog
        sectionToCancel={sectionToCancel}
        isOpen={!!sectionToCancel}
        onOpenChange={handleCancelDialogOpenChange}
      />

      {bulkTransitionAction && (
        <BulkStatusTransitionDialog
          isOpen
          onOpenChange={(open) => {
            if (!open) dialogs.close();
          }}
          action={bulkTransitionAction}
          sectionCount={bulkTransitionCount}
          onConfirm={handleBulkTransitionConfirm}
          isPending={isBulkOpenPending || isBulkCancelPending}
        />
      )}

      {dialogs.state.type === "bulk-assign-adviser" && (
        <BulkAdviserAssignDrawer
          isOpen
          onOpenChange={(open) => {
            if (!open) dialogs.close();
          }}
          sectionIds={[...dialogs.state.sectionIds]}
          sectionNames={dialogs.state.sectionIds.map((id) => {
            const section = coursesWithSections
              .flatMap((c) => c.classSections ?? [])
              .find((s) => s.id === id);
            return section?.fullName ?? `Section ${id}`;
          })}
          onSuccess={dialogs.close}
        />
      )}

      <ConflictPreviewDrawer
        sectionId={
          dialogs.state.type === "view-conflicts"
            ? dialogs.state.sectionId
            : null
        }
        sectionName={
          dialogs.state.type === "view-conflicts"
            ? dialogs.state.sectionName
            : ""
        }
        isOpen={dialogs.state.type === "view-conflicts"}
        onOpenChange={(open) => {
          if (!open) dialogs.close();
        }}
      />

      <ClassSectionOfferingsDetailDrawer
        sectionId={
          dialogs.state.type === "view-section-details"
            ? dialogs.state.sectionId
            : null
        }
        isOpen={dialogs.state.type === "view-section-details"}
        onOpenChange={(open) => {
          if (!open) dialogs.close();
        }}
      />

      <FloatingSelectionToolbar
        selection={selection}
        onBulkOpen={handleBulkOpen}
        onBulkCancel={handleBulkCancel}
        onBulkAssignAdviser={handleBulkAssignAdviser}
      />
    </div>
  );
}
