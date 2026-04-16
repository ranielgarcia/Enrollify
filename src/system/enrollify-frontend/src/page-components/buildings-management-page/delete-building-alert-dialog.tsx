import { deleteBuildingOptions } from "@/api/collections/building-collection";
import type { Building } from "@/api/models/building";
import { DeleteAlertDialog } from "@/components/delete-alert-dialog";

interface DeleteBuildingAlertDialogProps {
  buildingToDelete?: Building;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function DeleteBuildingAlertDialog({
  buildingToDelete,
  isOpen,
  onOpenChange,
}: DeleteBuildingAlertDialogProps) {
  return (
    <DeleteAlertDialog
      entityToDelete={buildingToDelete}
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      entityLabel="Building"
      getEntityName={(b) => b.name}
      deleteMutationOptions={deleteBuildingOptions(
        buildingToDelete?.id ?? 0
      )}
    />
  );
}
