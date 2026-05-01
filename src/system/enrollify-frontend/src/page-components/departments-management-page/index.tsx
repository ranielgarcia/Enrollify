import { getAllDepartmentsOptions } from "@/api/collections/department-collection";
import type { Department } from "@/api/models/department";
import { useSuspenseQuery } from "@tanstack/react-query";
import { DepartmentsTable } from "./departments-table";
import { getAllCollegesOptions } from "@/api/collections/college-collection";
import { DepartmentFormDrawer } from "./department-form-drawer";
import { DeleteDepartmentAlertDialog } from "./delete-department-alert-dialog";
import { useCrudState } from "@/hooks/use-crud-state";
import { ManagementPageLayout } from "@/components/page-layouts/management-page-layout";
import { ModuleIcons } from "@/config/module-icons";

export default function DepartmentPage() {
  const {
    isFormOpen,
    entityToEdit,
    entityToDelete,
    handleEdit,
    handleDelete,
    handleFormOpenChange,
    handleDeleteDialogOpenChange,
  } = useCrudState<Department>();

  const { data: departments } = useSuspenseQuery(getAllDepartmentsOptions());

  const { data: colleges } = useSuspenseQuery(getAllCollegesOptions());

  return (
    <ManagementPageLayout
      title="Department Management"
      description="Manage department resources"
      createNewItemButton={
        <DepartmentFormDrawer
          key={entityToEdit?.id ?? "new"}
          colleges={colleges}
          onOpenChange={handleFormOpenChange}
          departmentToUpdate={entityToEdit}
          isOpen={isFormOpen}
          setIsOpen={handleFormOpenChange}
        />
      }
      icon={<ModuleIcons.departments />}
    >
      <DepartmentsTable
        departments={departments}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />

      <DeleteDepartmentAlertDialog
        isOpen={!!entityToDelete}
        onOpenChange={handleDeleteDialogOpenChange}
        departmentToDelete={entityToDelete}
      />
    </ManagementPageLayout>
  );
}
