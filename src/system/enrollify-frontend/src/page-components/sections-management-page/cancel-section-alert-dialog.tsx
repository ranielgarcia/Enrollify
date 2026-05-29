import type { ClassSection } from "@/api/models/class-section";
import { DeleteAlertDialog } from "@/components/delete-alert-dialog";
import { cancelClassSectionOptions } from "@/api/collections/class-section-collection";

interface CancelSectionAlertDialogProps {
  sectionToCancel?: ClassSection;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function CancelSectionAlertDialog({
  sectionToCancel,
  isOpen,
  onOpenChange,
}: CancelSectionAlertDialogProps) {
  console.log(sectionToCancel);

  return (
    <DeleteAlertDialog
      entityToDelete={sectionToCancel}
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      entityLabel="Class Section"
      getEntityName={(s) => s.name}
      deleteMutationOptions={cancelClassSectionOptions(
        sectionToCancel?.id ?? 0,
      )}
      customDialogDescription={
        <>
          You are about to cancel class section{" "}
          <strong>{`${sectionToCancel?.name}-${sectionToCancel?.intendedYearLevel}${sectionToCancel?.sectionCode}`}</strong>
          .
          <br />
          This action will mark the section as <strong>Cancelled</strong> and
          cannot be undone.
        </>
      }
    />
  );
}
