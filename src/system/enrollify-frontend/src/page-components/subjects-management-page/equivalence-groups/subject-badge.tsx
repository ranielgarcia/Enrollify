import type {
  SubjectEquivalenceGroup,
  SubjectSummaryInSubjectEquivalenceGroup,
} from "@/api/models/subject-equivalence";
import { Badge } from "@/components/ui/badge";
import { X } from "lucide-react";
import { useState } from "react";
import { RemoveSubjectFromGroupDialog } from "./remove-subject-from-group-dialog";

interface SubjectBadgeProps {
  subject: SubjectSummaryInSubjectEquivalenceGroup;
  group: SubjectEquivalenceGroup;
}

export function SubjectBadge({ subject, group }: SubjectBadgeProps) {
  const [isDeleting, setIsDeleting] = useState(false);

  return (
    <>
      <Badge
        key={subject.id}
        variant="secondary"
        className="text-sm py-1.5 px-3 flex items-center gap-2 group hover:bg-secondary/80"
      >
        <span className="font-semibold">{subject.code}</span>
        <span className="text-muted-foreground">-</span>
        <span className="truncate max-w-[150px]">{subject.title}</span>
        <span className="text-xs text-muted-foreground">
          ({subject.units} units)
        </span>
        <button
          onClick={() => setIsDeleting(true)}
          className="ml-1 opacity-0 group-hover:opacity-100 transition-opacity hover:text-destructive"
          title="Remove from group"
        >
          <X className="size-3" />
        </button>
      </Badge>
      <RemoveSubjectFromGroupDialog
        subject={subject}
        group={group}
        isOpen={isDeleting}
        onOpenChange={setIsDeleting}
      />
    </>
  );
}
