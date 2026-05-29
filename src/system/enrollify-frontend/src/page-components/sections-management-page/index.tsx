import { getAllCoursesOptions } from "@/api/collections/course-collection";
import { filterClassSectionsPaginatedOptions } from "@/api/collections/class-section-collection";
import type { ClassSection } from "@/api/models/class-section";
import { ManagementPageLayout } from "@/components/page-layouts/management-page-layout";
import { ModuleIcons } from "@/config/module-icons";
import { useCrudState } from "@/hooks/use-crud-state";
import { useDebounce } from "@/hooks/use-debounce";
import { useSuspenseQuery } from "@tanstack/react-query";
import { useQueryStates } from "nuqs";
import { SectionFormDrawer } from "./section-form-drawer";
import { BulkInitializeSectionsDrawer } from "./bulk-initialize-sections-drawer";
import { DeleteSectionAlertDialog } from "./delete-section-alert-dialog";
import { SectionsTable } from "./sections-table";
import { searchParams } from "./searchParams";
import { useState } from "react";
import { useEnrollmentContext } from "@/contexts/enrollment-context/enrollment-context";

export default function SectionsManagementPage() {
  const [{ page, perPage, filters, sort, joinOperator }] =
    useQueryStates(searchParams);

  const { selectedAcademicYear } = useEnrollmentContext();

  const {
    isFormOpen,
    entityToEdit: sectionToEdit,
    entityToDelete: sectionToDelete,
    handleEdit,
    handleFormOpenChange,
    handleDeleteDialogOpenChange,
  } = useCrudState<ClassSection>();

  const [isBulkInitializeOpen, setIsBulkInitializeOpen] = useState(false);

  const currentPage = page ? Number(page) : 1;
  const currentPageSize = perPage ? Number(perPage) : 10;

  const debouncedFilters = useDebounce(filters, 600);
  const debouncedSort = useDebounce(sort, 600);

  const academicYearId = selectedAcademicYear?.id;

  const { data: pagedSections } = useSuspenseQuery(
    filterClassSectionsPaginatedOptions(
      currentPage,
      currentPageSize,
      academicYearId ?? 0,
      debouncedFilters,
      debouncedSort,
      joinOperator,
    ),
  );

  const { data: courses } = useSuspenseQuery(getAllCoursesOptions());

  return (
    <ManagementPageLayout
      title={`Class Sections of ${selectedAcademicYear?.academicYearTitle ?? "<invalid academic year...>"}`}
      description="Manage class sections, assign subjects, and build weekly schedules"
      icon={<ModuleIcons.sections />}
      createNewItemButton={
        <div className="flex gap-2">
          <BulkInitializeSectionsDrawer
            isOpen={isBulkInitializeOpen}
            setIsOpen={setIsBulkInitializeOpen}
            onOpenChange={setIsBulkInitializeOpen}
            courses={courses ?? []}
          />
          <SectionFormDrawer
            key={sectionToEdit?.id ?? "new"}
            courses={courses ?? []}
            onOpenChange={handleFormOpenChange}
            sectionToUpdate={sectionToEdit}
            isOpen={isFormOpen}
            setIsOpen={handleFormOpenChange}
          />
        </div>
      }
    >
      <SectionsTable
        pagedSections={pagedSections}
        onEdit={handleEdit}
        onDelete={handleDeleteDialogOpenChange}
      />

      <DeleteSectionAlertDialog
        sectionToDelete={sectionToDelete}
        isOpen={!!sectionToDelete}
        onOpenChange={handleDeleteDialogOpenChange}
      />
    </ManagementPageLayout>
  );
}
