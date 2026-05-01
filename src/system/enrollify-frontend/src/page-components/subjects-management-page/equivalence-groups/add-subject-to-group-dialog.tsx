import type { SubjectEquivalenceGroup } from "@/api/models/subject-equivalence";
import { useMemo } from "react";
import { AddSubjectsDialog } from "@/components/shared/add-subjects-dialog";

interface AddSubjectToGroupDialogProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  group?: SubjectEquivalenceGroup | null;
  onSubmit: (subjectCodes: string[]) => Promise<void>;
}

export function AddSubjectToGroupDialog({
  isOpen,
  onOpenChange,
  group,
  onSubmit,
}: AddSubjectToGroupDialogProps) {
  const existingSubjectCodes = useMemo(
    () => group?.subjects?.map((s) => s.code).filter(Boolean) ?? [],
    [group],
  );

  return (
    <AddSubjectsDialog
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      title="Add Subjects to Group"
      description={
        <>
          Add subjects to <strong>"{group?.name}"</strong> equivalence group.
          <br />
          Select the subjects that are considered equivalent.
        </>
      }
      excludeSubjectCodes={existingSubjectCodes}
      onSubmit={onSubmit}
    />
  );
}
