import { removeSubjectFromEquivalenceGroupOptions } from "@/api/collections/subject-equivalence-group-collection";
import type {
  SubjectEquivalenceGroup,
  SubjectSummaryInSubjectEquivalenceGroup,
} from "@/api/models/subject-equivalence";
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

interface RemoveSubjectFromGroupDialogProps {
  subject: SubjectSummaryInSubjectEquivalenceGroup;
  group: SubjectEquivalenceGroup;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function RemoveSubjectFromGroupDialog({
  subject,
  group,
  isOpen,
  onOpenChange,
}: RemoveSubjectFromGroupDialogProps) {
  const { mutateAsync: removeSubject, isPending: isRemoving } = useMutation(
    removeSubjectFromEquivalenceGroupOptions(group.id, subject.code),
  );

  return (
    <AlertDialog open={isOpen} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Remove Subject from Group?</AlertDialogTitle>
          <AlertDialogDescription>
            Are you sure you want to remove <strong>"{subject.code}"</strong>{" "}
            from the equivalence group <strong>"{group?.name}"</strong>?
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel
            disabled={isRemoving}
            onClick={() => onOpenChange(false)}
          >
            Cancel
          </AlertDialogCancel>
          <AlertDialogAction
            onClick={() => removeSubject({ subjectCode: subject.code })}
            disabled={isRemoving}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
          >
            {isRemoving ? "Removing..." : "Remove Subject from Group"}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
