import { filterSectionsPaginatedOptions } from "@/api/collections/section-collection";
import { getAllCoursesOptions } from "@/api/collections/course-collection";
import { filterTeachersPaginatedOptions } from "@/api/collections/teacher-collection";
import type { ClassSection } from "@/api/models/class-section";
import { ManagementPageLayout } from "@/components/page-layouts/management-page-layout";
import { ModuleIcons } from "@/config/module-icons";
import { useCrudState } from "@/hooks/use-crud-state";
import { useDebounce } from "@/hooks/use-debounce";
import { useSuspenseQuery } from "@tanstack/react-query";
import { useQueryStates } from "nuqs";
import { SectionFormDrawer } from "./section-form-drawer";
import { SectionsTable } from "./sections-table";
import { DeleteSectionAlertDialog } from "./delete-section-alert-dialog";
import { searchParams } from "./searchParams";

export default function SectionsManagementPage() {
  const [{ page, perPage, filters, sort, joinOperator }] =
    useQueryStates(searchParams);

  const {
    isFormOpen,
    entityToEdit: sectionToEdit,
    entityToDelete: sectionToDelete,
    handleEdit,
    handleDelete,
    handleFormOpenChange,
    handleDeleteDialogOpenChange,
  } = useCrudState<ClassSection>();

  const currentPage = page ? Number(page) : 1;
  const currentPageSize = perPage ? Number(perPage) : 10;

  const debouncedFilters = useDebounce(filters, 600);
  const debouncedSort = useDebounce(sort, 600);

  const { data: pagedSections } = useSuspenseQuery(
    filterSectionsPaginatedOptions(
      currentPage,
      currentPageSize,
      debouncedFilters,
      debouncedSort,
      joinOperator,
    ),
  );

  const { data: courses } = useSuspenseQuery(getAllCoursesOptions());
  const { data: teachers } = useSuspenseQuery(
    filterTeachersPaginatedOptions(1, 10, [], [], "and"),
  );
  // const { data: academicTerms } = useSuspenseQuery(
  //   getAllAcademicTermsOptions(),
  // );

  return (
    <ManagementPageLayout
      title="Class Sections"
      description="Manage class sections, assign subjects, and build weekly schedules"
      icon={<ModuleIcons.sections />}
      createNewItemButton={
        <SectionFormDrawer
          key={sectionToEdit?.id ?? "new"}
          courses={courses ?? []}
          teachers={teachers?.items ?? []}
          academicTerms={[]}
          onOpenChange={handleFormOpenChange}
          sectionToUpdate={sectionToEdit}
          isOpen={isFormOpen}
          setIsOpen={handleFormOpenChange}
        />
      }
    >
      <SectionsTable
        pagedSections={pagedSections}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />

      <DeleteSectionAlertDialog
        sectionToDelete={sectionToDelete}
        isOpen={!!sectionToDelete}
        onOpenChange={handleDeleteDialogOpenChange}
      />
    </ManagementPageLayout>
  );
}
