import { SearchSubjectsDialog } from "@/components/shared/search-subjects-dialog";

interface ManageTeacherSubjectsDialogProps {
  teacherName?: string;
  existingSubjectCodes: string[];
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (subjectCodes: string[]) => Promise<void>;
}

export function ManageTeacherSubjectsDialog({
  teacherName,
  existingSubjectCodes,
  isOpen,
  onOpenChange,
  onSubmit,
}: ManageTeacherSubjectsDialogProps) {
  const normalizedTeacherName = teacherName?.trim();
  const title = normalizedTeacherName
    ? `Manage Subjects for ${normalizedTeacherName}`
    : "Manage Teacher Subjects";

  return (
    <SearchSubjectsDialog
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      title={title}
      description={
        <>
          Select the subjects to assign to this teacher.
          <br />
          Subjects already selected are excluded from the list.
        </>
      }
      excludeSubjectCodes={existingSubjectCodes}
      onSubmit={onSubmit}
    />
  );
}
