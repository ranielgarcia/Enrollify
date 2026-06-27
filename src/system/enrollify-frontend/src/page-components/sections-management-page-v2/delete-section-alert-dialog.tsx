import type { ClassSectionMinimal } from "@/api/models/class-scheduling/class-section";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";

interface DeleteSectionAlertDialogProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  sectionToDelete?: ClassSectionMinimal | null;
}

export function DeleteSectionAlertDialog({
  isOpen,
  onOpenChange,
  sectionToDelete,
}: DeleteSectionAlertDialogProps) {
  return (
    <AlertDialog open={isOpen} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Delete Section</AlertDialogTitle>
          <AlertDialogDescription>
            Are you sure you want to delete {sectionToDelete?.fullName}? This
            action cannot be undone. Delete functionality coming in Phase 2.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <div className="flex gap-3">
          <AlertDialogCancel>Cancel</AlertDialogCancel>
          <AlertDialogAction disabled className="bg-destructive text-destructive-foreground">
            Delete
          </AlertDialogAction>
        </div>
      </AlertDialogContent>
    </AlertDialog>
  );
}
