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
import { type ClassSectionMinimal } from "@/api/models/class-scheduling/class-section";
import { cancelClassSectionOptions } from "@/api/collections/class-section-collection";
import { useMutation } from "@tanstack/react-query";
import { Loader2 } from "lucide-react";

interface CancelSectionAlertDialogProps {
  sectionToCancel: ClassSectionMinimal | null | undefined;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function CancelSectionAlertDialog({
  sectionToCancel,
  isOpen,
  onOpenChange,
}: CancelSectionAlertDialogProps) {
  const { mutateAsync, isPending } = useMutation(
    cancelClassSectionOptions(sectionToCancel?.id ?? 0),
  );

  const handleConfirm = async () => {
    if (!sectionToCancel) return;
    await mutateAsync({});
    onOpenChange(false);
  };

  return (
    <AlertDialog open={isOpen} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Cancel Section</AlertDialogTitle>
          <AlertDialogDescription asChild>
            <div className="space-y-2">
              <p>
                Are you sure you want to cancel{" "}
                <span className="font-semibold text-foreground">
                  {sectionToCancel?.fullName}
                </span>
                ?
              </p>
              <p className="text-sm">
                Cancelling a section will release all teacher and room
                assignments from its offerings. This action removes the section
                from conflict detection. This cannot be undone.
              </p>
            </div>
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel disabled={isPending}>Keep Section</AlertDialogCancel>
          <AlertDialogAction
            disabled={isPending}
            onClick={(e) => {
              e.preventDefault();
              handleConfirm();
            }}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
          >
            {isPending && <Loader2 className="mr-2 size-4 animate-spin" />}
            Cancel Section
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
