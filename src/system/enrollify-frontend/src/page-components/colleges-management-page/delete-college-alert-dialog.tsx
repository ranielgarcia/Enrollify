import type { College } from "@/api/models/college";
import { deleteCollegeOptions } from "@/api/collections/college-collection";
import { DeleteAlertDialog } from "@/components/delete-alert-dialog";

interface DeleteCollegeAlertDialogProps {
  collegeToDelete?: College;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function DeleteCollegeAlertDialog({
  collegeToDelete,
  isOpen,
  onOpenChange,
}: DeleteCollegeAlertDialogProps) {
  return (
    <DeleteAlertDialog
      entityToDelete={collegeToDelete}
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      entityLabel="College"
      getEntityName={(c) => c.name}
      deleteMutationOptions={deleteCollegeOptions(collegeToDelete?.id ?? 0)}
    />
  );
}
