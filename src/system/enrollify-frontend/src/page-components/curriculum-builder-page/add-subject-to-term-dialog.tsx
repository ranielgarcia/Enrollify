import { SearchSubjectsDialog } from "@/components/shared/search-subjects-dialog";

interface AddSubjectToTermDialogProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  year: number;
  term: number;
  excludeSubjectCodes: string[];
  onSubmit: (subjectCodes: string[]) => Promise<void>;
}

export function AddSubjectToTermDialog({
  isOpen,
  onOpenChange,
  year,
  term,
  excludeSubjectCodes,
  onSubmit,
}: AddSubjectToTermDialogProps) {
  return (
    <SearchSubjectsDialog
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      title={`Add Subject to Term ${term}`}
      description={
        <>
          Search and select subjects to add to <strong>Year {year}</strong>,{" "}
          <strong>Term {term}</strong>.
        </>
      }
      excludeSubjectCodes={excludeSubjectCodes}
      onSubmit={onSubmit}
    />
  );
}
