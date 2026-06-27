import { Suspense, useState, useCallback } from "react";
import {
  getClassSectionsStatsOptions,
  getCollegeCoursesWithClassSectionsForSchedulingOptions,
  openClassSectionOptions,
  bulkOpenSectionsMutationOptions,
  bulkCancelSectionsMutationOptions,
} from "@/api/collections/class-section-collection";
import { useSuspenseQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useQueryStates } from "nuqs";
import { ManagementPageLayout } from "@/components/page-layouts/management-page-layout";
import { searchParams } from "./searchParams";
import type { QuickFilter } from "./searchParams";
import { ModuleIcons } from "@/config/module-icons";
import { CollegeSelector } from "./college-selector";
import { StatsBar } from "./stats-bar";
import { QuickFilters } from "./quick-filters";
import { SectionsCardView } from "./sections-card-view";
import { SectionsTable } from "./sections-table";
import { SectionFormDrawer } from "./section-form-drawer";
import { CancelSectionAlertDialog } from "./cancel-section-alert-dialog";
import { BulkStatusTransitionDialog } from "./bulk-status-transition-dialog";
import { BulkAdviserAssignDrawer } from "./bulk-adviser-assign-drawer";
import { ConflictPreviewDrawer } from "./conflict-preview-drawer";
import { SectionsSkeleton } from "./sections-skeleton";
import { EmptyState } from "./empty-state";
import { Button } from "@/components/ui/button";
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/toggle-group";
import {
  LayoutGrid,
  TableIcon,
  Plus,
} from "lucide-react";
import type { ClassSectionMinimal } from "@/api/models/class-scheduling/class-section";
import { useEnrollmentContext } from "@/contexts/enrollment-context/enrollment-context";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { AlertCircle } from "lucide-react";
import { useCrudState } from "@/hooks/use-crud-state";

// ─── Dialog/Drawer state types ───────────────────────────────────────────────

interface BulkTransitionState {
  action: "open" | "cancel";
  sectionIds: number[];
}

interface ConflictPreviewState {
  sectionId: number;
  sectionName: string;
}

// ─── Page Content (inside Suspense) ──────────────────────────────────────────

function SectionsPageContent() {
  const [{ collegeId, view, quickFilter }, setParams] =
    useQueryStates(searchParams);

  const { selectedAcademicTerm } = useEnrollmentContext();
  const queryClient = useQueryClient();

  // ── Data queries ────────────────────────────────────────────────────────────
  const { data: stats } = useSuspenseQuery(
    getClassSectionsStatsOptions(
      collegeId ?? undefined,
      selectedAcademicTerm?.id,
    ),
  );

  const { data: collegeData } = useSuspenseQuery(
    getCollegeCoursesWithClassSectionsForSchedulingOptions(
      collegeId ?? undefined,
      selectedAcademicTerm?.id,
    ),
  );

  // ── CRUD state (edit / delete) ────────────────────────────────────────────
  const {
    isFormOpen,
    entityToEdit,
    entityToDelete: sectionToCancel,
    handleEdit,
    handleDelete: handleCancelSection,
    handleFormOpenChange,
    handleDeleteDialogOpenChange: handleCancelDialogOpenChange,
  } = useCrudState<ClassSectionMinimal>();

  // ── Single-section open mutation ──────────────────────────────────────────
  const [openPendingId, setOpenPendingId] = useState<number | null>(null);
  const openMutation = useMutation(
    openClassSectionOptions(openPendingId ?? 0),
  );

  // ── Bulk mutations ────────────────────────────────────────────────────────
  const { mutateAsync: bulkOpen, isPending: isBulkOpenPending } = useMutation(
    bulkOpenSectionsMutationOptions(),
  );
  const { mutateAsync: bulkCancel, isPending: isBulkCancelPending } =
    useMutation(bulkCancelSectionsMutationOptions());

  // ── Bulk transition dialog ────────────────────────────────────────────────
  const [bulkTransitionState, setBulkTransitionState] =
    useState<BulkTransitionState | null>(null);

  // ── Bulk adviser drawer ───────────────────────────────────────────────────
  const [adviserAssignState, setAdviserAssignState] = useState<{
    sectionIds: number[];
  } | null>(null);

  // ── Conflict preview drawer ────────────────────────────────────────────────
  const [conflictPreview, setConflictPreview] =
    useState<ConflictPreviewState | null>(null);

  // ── Handlers ──────────────────────────────────────────────────────────────

  const handleOpenSection = useCallback(
    async (section: ClassSectionMinimal) => {
      setOpenPendingId(section.id);
      try {
        await openMutation.mutateAsync({});
        await queryClient.invalidateQueries({ queryKey: ["sections"] });
      } finally {
        setOpenPendingId(null);
      }
    },
    [openMutation, queryClient],
  );

  const handleViewDetails = useCallback((section: ClassSectionMinimal) => {
    // Navigate to section detail page — placeholder for now
    // In a real app: router.navigate({ to: "/scheduling/sections/$sectionId", params: { sectionId: section.id.toString() } });
    console.info("Navigate to section detail:", section.id);
  }, []);

  const handleViewConflicts = useCallback((section: ClassSectionMinimal) => {
    setConflictPreview({ sectionId: section.id, sectionName: section.name });
  }, []);

  const handleBulkOpen = useCallback((sectionIds: number[]) => {
    setBulkTransitionState({ action: "open", sectionIds });
  }, []);

  const handleBulkCancel = useCallback((sectionIds: number[]) => {
    setBulkTransitionState({ action: "cancel", sectionIds });
  }, []);

  const handleBulkAssignAdviser = useCallback((sectionIds: number[]) => {
    setAdviserAssignState({ sectionIds });
  }, []);

  const handleBulkTransitionConfirm = async () => {
    if (!bulkTransitionState) return;
    const { action, sectionIds } = bulkTransitionState;
    if (action === "open") {
      await bulkOpen({ sectionIds });
    } else {
      await bulkCancel({ sectionIds });
    }
    setBulkTransitionState(null);
  };

  const handleQuickFilterChange = (filter: QuickFilter) => {
    setParams({ quickFilter: filter });
  };

  const handleClearFilters = () => {
    setParams({ quickFilter: "all" });
  };

  const handleViewChange = (v: string) => {
    if (v === "card" || v === "table") {
      setParams({ view: v });
    }
  };

  const coursesWithSections = collegeData?.coursesWithClassSections ?? [];
  const collegeName = collegeData?.name ?? "";
  const noCollegeSelected = !collegeId;

  return (
    <div className="flex flex-col gap-5">
      {/* ── Top bar: College selector ── */}
      <div className="flex items-center gap-4 flex-wrap">
        <CollegeSelector />
        {selectedAcademicTerm && (
          <span className="text-sm text-muted-foreground">
            {selectedAcademicTerm.termName}
          </span>
        )}
      </div>

      {/* ── No academic term warning ── */}
      {!selectedAcademicTerm && (
        <Alert variant="destructive">
          <AlertCircle className="size-4" />
          <AlertTitle>Academic Term Not Selected</AlertTitle>
          <AlertDescription>
            Please select an academic year and term to view class sections.
          </AlertDescription>
        </Alert>
      )}

      {/* ── Stats bar ── */}
      {!noCollegeSelected && selectedAcademicTerm && (
        <StatsBar
          stats={stats ?? {}}
          activeFilter={quickFilter as QuickFilter}
          onFilterChange={handleQuickFilterChange}
        />
      )}

      {/* ── No college selected ── */}
      {noCollegeSelected && <EmptyState variant="no-college" />}

      {/* ── Content area (college selected) ── */}
      {!noCollegeSelected && selectedAcademicTerm && (
        <>
          {/* Toolbar row: view toggle + quick filters */}
          <div className="flex items-center justify-between gap-4 flex-wrap">
            {/* View toggle */}
            <ToggleGroup
              type="single"
              value={view}
              onValueChange={handleViewChange}
              className="h-8"
            >
              <ToggleGroupItem
                value="card"
                aria-label="Card view"
                className="h-8 gap-1.5 text-xs px-3"
              >
                <LayoutGrid className="size-3.5" />
                Cards
              </ToggleGroupItem>
              <ToggleGroupItem
                value="table"
                aria-label="Table view"
                className="h-8 gap-1.5 text-xs px-3"
              >
                <TableIcon className="size-3.5" />
                Table
              </ToggleGroupItem>
            </ToggleGroup>

            {/* Quick filters */}
            <QuickFilters
              activeFilter={quickFilter as QuickFilter}
              onFilterChange={handleQuickFilterChange}
            />
          </div>

          {/* ── Card view ── */}
          {view === "card" && (
            <SectionsCardView
              coursesWithSections={coursesWithSections}
              collegeName={collegeName}
              quickFilter={quickFilter as QuickFilter}
              onClearFilters={handleClearFilters}
              onOpenSection={handleOpenSection}
              onCancelSection={handleCancelSection}
              onViewDetails={handleViewDetails}
              onChangeAdviser={handleEdit}
              onViewConflicts={handleViewConflicts}
              onBulkOpen={handleBulkOpen}
              onBulkCancel={handleBulkCancel}
              onBulkAssignAdviser={handleBulkAssignAdviser}
              openSectionPendingId={openPendingId}
            />
          )}

          {/* ── Table view ── */}
          {view === "table" && (
            <SectionsTable
              coursesWithSections={coursesWithSections}
              quickFilter={quickFilter as QuickFilter}
              onClearFilters={handleClearFilters}
              onOpenSection={handleOpenSection}
              onCancelSection={handleCancelSection}
              onViewDetails={handleViewDetails}
              onViewConflicts={handleViewConflicts}
              onBulkOpen={handleBulkOpen}
              onBulkCancel={handleBulkCancel}
              onBulkAssignAdviser={handleBulkAssignAdviser}
              openSectionPendingId={openPendingId}
            />
          )}
        </>
      )}

      {/* ── Dialogs & Drawers ── */}

      {/* Edit section drawer */}
      <SectionFormDrawer
        key={entityToEdit?.id ?? "new"}
        isOpen={isFormOpen}
        onOpenChange={handleFormOpenChange}
        sectionToUpdate={entityToEdit}
      />

      {/* Cancel single section */}
      <CancelSectionAlertDialog
        sectionToCancel={sectionToCancel}
        isOpen={!!sectionToCancel}
        onOpenChange={handleCancelDialogOpenChange}
      />

      {/* Bulk transition dialog */}
      {bulkTransitionState && (
        <BulkStatusTransitionDialog
          isOpen={!!bulkTransitionState}
          onOpenChange={(open) => {
            if (!open) setBulkTransitionState(null);
          }}
          action={bulkTransitionState.action}
          sectionCount={bulkTransitionState.sectionIds.length}
          onConfirm={handleBulkTransitionConfirm}
          isPending={isBulkOpenPending || isBulkCancelPending}
        />
      )}

      {/* Bulk adviser assignment */}
      {adviserAssignState && (
        <BulkAdviserAssignDrawer
          isOpen={!!adviserAssignState}
          onOpenChange={(open) => {
            if (!open) setAdviserAssignState(null);
          }}
          sectionIds={adviserAssignState.sectionIds}
          onSuccess={() => setAdviserAssignState(null)}
        />
      )}

      {/* Conflict preview */}
      <ConflictPreviewDrawer
        sectionId={conflictPreview?.sectionId ?? null}
        sectionName={conflictPreview?.sectionName ?? ""}
        isOpen={!!conflictPreview}
        onOpenChange={(open) => {
          if (!open) setConflictPreview(null);
        }}
      />
    </div>
  );
}

// ─── Page Root ────────────────────────────────────────────────────────────────

export default function SectionsManagementPageV2() {
  return (
    <ManagementPageLayout
      title="Class Sections"
      description="Manage scheduling for class sections by college and course"
      icon={<ModuleIcons.sections />}
      createNewItemButton={
        <Button variant="outline" size="sm" disabled className="gap-1.5">
          <Plus className="size-4" />
          Bulk Initialize
        </Button>
      }
    >
      <Suspense fallback={<SectionsSkeleton />}>
        <SectionsPageContent />
      </Suspense>
    </ManagementPageLayout>
  );
}
