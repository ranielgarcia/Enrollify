import { deleteCourseOptions } from "@/api/collections/course-collection";
import type { Course } from "@/api/models/course";
import { DeleteAlertDialog } from "@/components/delete-alert-dialog";

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
  return (
    <DeleteAlertDialog
      entityToDelete={courseToDelete}
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      entityLabel="Course"
      getEntityName={(c) => c.name}
      deleteMutationOptions={deleteCourseOptions(courseToDelete?.id ?? 0)}
    />
  );
}
