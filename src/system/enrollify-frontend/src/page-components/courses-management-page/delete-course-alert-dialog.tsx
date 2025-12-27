import { deleteCourseOptions } from "@/api/collections/course-collection";
import type { Course } from "@/api/models/course";
import { OverlayLoader } from "@/components/app-loading-overlay";
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
import { useMutation } from "@tanstack/react-query";

interface DeleteCourseAlertDialogProps {
  courseToDelete?: Course;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function DeleteCourseAlertDialog({
  courseToDelete,
  isOpen,
  onOpenChange,
}: DeleteCourseAlertDialogProps) {
  const { mutateAsync: deleteCourseAsync, isPending: isDeletingInProgress } =
    useMutation(deleteCourseOptions(courseToDelete?.id ?? 0));

  const handleContinueDelete = async () => {
    await deleteCourseAsync(undefined);
  };

  return (
    <AlertDialog onOpenChange={onOpenChange} open={isOpen}>
      <AlertDialogContent>
        <OverlayLoader
          isLoading={isDeletingInProgress}
          text={isDeletingInProgress ? "Deleting..." : "Processing.."}
          size="sm"
        />
        <AlertDialogHeader>
          <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>

          <AlertDialogDescription>
            Deleting course <strong>{courseToDelete?.name}</strong> <br />
            This action cannot be undone. This will permanently delete your
            course and remove your data from our servers.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel onClick={() => onOpenChange(false)}>
            Cancel
          </AlertDialogCancel>
          <AlertDialogAction onClick={async () => await handleContinueDelete()}>
            Continue
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
