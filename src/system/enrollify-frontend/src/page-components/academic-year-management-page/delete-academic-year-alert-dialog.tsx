import { deleteAcademicYearOptions } from "@/api/collections/academic-year-collection";
import type { AcademicYear } from "@/api/models/academic-year";
import { DeleteAlertDialog } from "@/components/delete-alert-dialog";

interface DeleteAcademicYearAlertDialogProps {
  yearToDelete?: AcademicYear;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function DeleteAcademicYearAlertDialog({
  yearToDelete,
  isOpen,
  onOpenChange,
}: DeleteAcademicYearAlertDialogProps) {
  return (
    <DeleteAlertDialog
      entityToDelete={yearToDelete}
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      entityLabel="Academic Year"
      getEntityName={(y) =>
        y.academicYearTitle ?? `AY ${y.startYear}–${y.endYear}`
      }
      deleteMutationOptions={deleteAcademicYearOptions(yearToDelete?.id ?? 0)}
    />
  );
}
