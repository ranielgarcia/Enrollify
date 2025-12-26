import { getAllDepartmentsOptions } from "@/api/collections/department-collection";
import type { Department } from "@/api/models/department";
import { useSuspenseQuery } from "@tanstack/react-query";
import { useState } from "react";
import { DepartmentsTable } from "./departments-table";
import { getAllCollegesOptions } from "@/api/collections/college-collection";
import { DepartmentFormDrawer } from "./department-form-drawer";
import { DeleteDepartmentAlertDialog } from "./delete-department-alert-dialog";

export default function DepartmentPage() {
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [departmentToEdit, setDepartmentToEdit] = useState<
    Department | undefined
  >();
  const [departmentToDelete, setDepartmentToDelete] = useState<
    Department | undefined
  >();

  const { data: departments } = useSuspenseQuery(getAllDepartmentsOptions());

  const { data: colleges } = useSuspenseQuery(getAllCollegesOptions());

  const handleEdit = (department: Department) => {
    setDepartmentToEdit(department);
    setIsFormOpen(true);
  };

  const handleDelete = (department: Department) => {
    setDepartmentToDelete(department);
  };

  const handleDrawerOnOpenChange = (open: boolean) => {
    setDepartmentToEdit(undefined);
    setIsFormOpen(open);
  };

  const handleDeleteDepartmentAlertDialogOnOpenChange = (open: boolean) => {
    if (!open) {
      setDepartmentToDelete(undefined);
    }
  };

  return (
    <main>
      <div className="p-4 md:p-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-foreground mb-2">
            College Management
          </h1>
          <p className="text-muted-foreground">Manage college resources</p>
        </div>

        <div className="flex justify-end">
          <DepartmentFormDrawer
            colleges={colleges}
            onOpenChange={handleDrawerOnOpenChange}
            departmentToUpdate={departmentToEdit}
            isOpen={isFormOpen}
            setIsOpen={setIsFormOpen}
          />
        </div>

        <DepartmentsTable
          departments={departments}
          onEdit={handleEdit}
          onDelete={handleDelete}
        />

        <DeleteDepartmentAlertDialog
          isOpen={!!departmentToDelete}
          onOpenChange={handleDeleteDepartmentAlertDialogOnOpenChange}
          departmentToDelete={departmentToDelete}
        />
      </div>
    </main>
  );
}
