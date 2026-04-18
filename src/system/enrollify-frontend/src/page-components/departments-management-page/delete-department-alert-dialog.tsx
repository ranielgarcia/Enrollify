import { deleteDepartmentOptions } from "@/api/collections/department-collection";
import type { Department } from "@/api/models/department";
import { DeleteAlertDialog } from "@/components/delete-alert-dialog";

interface DeleteDepartmentAlertDialogProps {
  departmentToDelete?: Department;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function DeleteDepartmentAlertDialog({
  departmentToDelete,
  isOpen,
  onOpenChange,
}: DeleteDepartmentAlertDialogProps) {
  return (
    <DeleteAlertDialog
      entityToDelete={departmentToDelete}
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      entityLabel="Department"
      getEntityName={(d) => d.name}
      deleteMutationOptions={deleteDepartmentOptions(
        departmentToDelete?.id ?? 0
      )}
    />
  );
}
