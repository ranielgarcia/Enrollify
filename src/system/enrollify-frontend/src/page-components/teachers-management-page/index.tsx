import { filterTeachersPaginatedOptions } from "@/api/collections/teacher-collection";
import { getAllDepartmentsOptions } from "@/api/collections/department-collection";
import type { Teacher } from "@/api/models/teacher";
import { useSuspenseQuery } from "@tanstack/react-query";
import { useQueryStates } from "nuqs";
import { ManagementPageLayout } from "@/components/page-layouts/management-page-layout";
import { useCrudState } from "@/hooks/use-crud-state";
import { useDebounce } from "@/hooks/use-debounce";
import { TeacherFormDrawer } from "./teacher-form-drawer";
import { TeachersTable } from "./teachers-table";
import { searchParams } from "./searchParams";
import { ModuleIcons } from "@/config/module-icons";

export default function TeachersManagementPage() {
  const [{ page, perPage, filters, sort, joinOperator }] =
    useQueryStates(searchParams);

  const {
    isFormOpen,
    entityToEdit: teacherToEdit,
    handleEdit,
    handleFormOpenChange,
  } = useCrudState<Teacher>();

  const {
    isFormOpen: isManageSubjectsDialogOpen,
    entityToEdit: teacherToManageSubjects,
    handleEdit: handleTeacherEditSubjects,
    handleFormOpenChange: handleTeacherDialogOpenChange,
  } = useCrudState<Teacher>();

  const currentPage = page ? Number(page) : 1;
  const currentPageSize = perPage ? Number(perPage) : 10;

  const debouncedFilters = useDebounce(filters, 600);
  const debouncedSort = useDebounce(sort, 600);

  const { data: pagedTeachers } = useSuspenseQuery(
    filterTeachersPaginatedOptions(
      currentPage,
      currentPageSize,
      debouncedFilters,
      debouncedSort,
      joinOperator,
    ),
  );

  const { data: departments } = useSuspenseQuery(getAllDepartmentsOptions());

  return (
    <ManagementPageLayout
      title="Teachers Management"
      description="Manage faculty members and their academic profiles"
      icon={<ModuleIcons.teachers />}
      createNewItemButton={
        <TeacherFormDrawer
          departments={departments}
          onOpenChange={handleFormOpenChange}
          teacherToUpdate={teacherToEdit}
          isOpen={isFormOpen}
          setIsOpen={handleFormOpenChange}
        />
      }
    >
      <TeachersTable
        pagedTeachers={pagedTeachers}
        onEdit={handleEdit}
        onManageSubjects={(teacher) => console.log(teacher)}
      />
    </ManagementPageLayout>
  );
}
