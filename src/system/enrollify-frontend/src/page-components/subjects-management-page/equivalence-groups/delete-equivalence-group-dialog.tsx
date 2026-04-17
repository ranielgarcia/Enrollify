import { deleteSubjectEquivalenceGroupOptions } from "@/api/collections/subject-equivalence-group-collection";
import type { SubjectEquivalenceGroup } from "@/api/models/subject-equivalence";
import { DeleteAlertDialog } from "@/components/delete-alert-dialog";

interface DeleteEquivalenceGroupDialogProps {
  groupToDelete?: SubjectEquivalenceGroup | null;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function DeleteEquivalenceGroupDialog({
  groupToDelete,
  isOpen,
  onOpenChange,
}: DeleteEquivalenceGroupDialogProps) {
  return (
    <>
      <DeleteAlertDialog
        entityToDelete={groupToDelete}
        isOpen={isOpen}
        onOpenChange={onOpenChange}
        entityLabel="Equivalence Group"
        getEntityName={(r) => r?.name ?? "Unknown Group"}
        deleteMutationOptions={deleteSubjectEquivalenceGroupOptions(
          groupToDelete?.id ?? 0,
        )}
        customDialogDescription={
          <>
            Are you sure you want to delete the equivalence group{" "}
            <strong>"{groupToDelete?.name}"</strong>?
            <br />
            <br />
            This will remove all subject associations from this group. The
            subjects themselves will not be deleted.
          </>
        }
      />
    </>
  );
}
