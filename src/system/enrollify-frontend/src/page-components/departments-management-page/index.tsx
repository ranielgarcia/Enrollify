import { getAllDepartmentsOptions } from "@/api/collections/department-collection";
import type { Department } from "@/api/models/department";
import { useSuspenseQuery } from "@tanstack/react-query";
import { DepartmentsTable } from "./departments-table";
import { getAllCollegesOptions } from "@/api/collections/college-collection";
import { DepartmentFormDrawer } from "./department-form-drawer";
import { DeleteDepartmentAlertDialog } from "./delete-department-alert-dialog";
import { useCrudState } from "@/hooks/use-crud-state";
import { ManagementPageLayout } from "@/components/management-page-layout";
import { Castle } from "lucide-react";

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
          colleges={colleges}
          onOpenChange={handleFormOpenChange}
          departmentToUpdate={entityToEdit}
          isOpen={isFormOpen}
          setIsOpen={handleFormOpenChange}
        />
      }
      icon={<Castle />}
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
