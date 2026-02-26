import type { SubjectEquivalenceGroup } from "@/api/models/subject-equivalence";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";

interface DeleteEquivalenceGroupDialogProps {
  group?: SubjectEquivalenceGroup | null;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => Promise<void>;
  isDeleting?: boolean;
}

export function DeleteEquivalenceGroupDialog({
  group,
  isOpen,
  onOpenChange,
  onConfirm,
  isDeleting,
}: DeleteEquivalenceGroupDialogProps) {
  const handleConfirm = async () => {
    await onConfirm();
    onOpenChange(false);
  };

  return (
    <AlertDialog open={isOpen} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Delete Equivalence Group?</AlertDialogTitle>
          <AlertDialogDescription>
            Are you sure you want to delete the equivalence group{" "}
            <strong>"{group?.name}"</strong>?
            <br />
            <br />
            This will remove all subject associations from this group. The
            subjects themselves will not be deleted.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel disabled={isDeleting}>Cancel</AlertDialogCancel>
          <AlertDialogAction
            onClick={handleConfirm}
            disabled={isDeleting}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
          >
            {isDeleting ? "Deleting..." : "Delete Group"}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
