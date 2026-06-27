import {
  getClassSectionsStatsOptions,
  filterClassSectionsPaginatedOptions,
} from "@/api/collections/class-section-collection";
import { Button } from "@/components/ui/button";
import { useSuspenseQuery } from "@tanstack/react-query";
import { Suspense } from "react";
import { Plus } from "lucide-react";
import { useQueryStates } from "nuqs";
import { ManagementPageLayout } from "@/components/page-layouts/management-page-layout";
import { searchParams } from "./searchParams";
import { useCrudState } from "@/hooks/use-crud-state";
import { ModuleIcons } from "@/config/module-icons";
import { StatsPanel } from "./stats-panel";
import { CollegeSelector } from "./college-selector";
import { SectionsTable } from "./sections-table";
import { SectionFormDrawer } from "./section-form-drawer";
import { DeleteSectionAlertDialog } from "./delete-section-alert-dialog";
import { SectionsSkeleton } from "./sections-skeleton";
import type { ClassSection } from "@/api/models/class-scheduling/class-section";

function SectionsPageContent() {
  const [{ collegeId, page, perPage, filters, sort, joinOperator }] =
    useQueryStates(searchParams);

  const {
    isFormOpen,
    entityToEdit,
    entityToDelete,
    handleEdit,
    handleDelete,
    handleFormOpenChange,
    handleDeleteDialogOpenChange,
  } = useCrudState<ClassSection>();

  const currentPage = page ? Number(page) : 1;
  const currentPageSize = perPage ? Number(perPage) : 10;

  // Stats query (for selected college only)
  const { data: stats } = useSuspenseQuery(
    getClassSectionsStatsOptions(undefined),
  );

  // Sections list query (paginated, with filters)
  const { data: pagedSections } = useSuspenseQuery(
    filterClassSectionsPaginatedOptions(
      0,
      currentPage,
      currentPageSize,
      filters,
      sort,
      joinOperator,
    ),
  );

  return (
    <ManagementPageLayout
      title="Class Sections (v2)"
      description="Manage class sections and their scheduling"
      icon={<ModuleIcons.sections />}
      createNewItemButton={
        <Button onClick={() => handleFormOpenChange(true)}>
          <Plus className="size-4 mr-2" />
          New Section
        </Button>
      }
    >
      <div className="flex flex-col space-y-6">
        {/* College Selector */}
        <CollegeSelector selectedCollegeId={collegeId} />

        {/* Stats Panel */}
        <StatsPanel stats={stats} />

        {/* Sections Table */}
        <SectionsTable
          sections={pagedSections.data}
          totalRecords={pagedSections.totalRecords}
          currentPage={currentPage}
          pageSize={currentPageSize}
          onEdit={handleEdit}
          onDelete={handleDelete}
        />

        {/* Form Drawer */}
        <SectionFormDrawer
          key={entityToEdit?.id ?? "new"}
          onOpenChange={handleFormOpenChange}
          sectionToUpdate={entityToEdit}
          isOpen={isFormOpen}
          setIsOpen={handleFormOpenChange}
        />

        {/* Delete Dialog */}
        <DeleteSectionAlertDialog
          isOpen={!!entityToDelete}
          onOpenChange={handleDeleteDialogOpenChange}
          sectionToDelete={entityToDelete}
        />
      </div>
    </ManagementPageLayout>
  );
}

export default function SectionsManagementPageV2() {
  return (
    <Suspense fallback={<SectionsSkeleton />}>
      <SectionsPageContent />
    </Suspense>
  );
}
