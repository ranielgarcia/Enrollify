import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { Badge } from "@/components/ui/badge";
import { BookOpen, ChevronDown } from "lucide-react";
import type { Offering } from "@/api/models/class-scheduling/offering";
import { ValidationIssueCard } from "./validation-issue-card";

export function OfferingValidationSection({
  offering,
}: {
  offering: Offering;
}) {
  const validationIssueCount = offering.validationIssues?.length ?? 0;
  if (validationIssueCount === 0) return null;

  const subjectLabel =
    offering.snapshotSubjectCode ?? `Offering #${offering.id}`;

  return (
    <Collapsible defaultOpen>
      <CollapsibleTrigger className="flex w-full items-center gap-2 rounded-md border bg-muted/30 px-3 py-2 text-sm transition-colors hover:bg-muted/50 [&[data-state=closed]>svg:first-child]:-rotate-90 [&[data-state=open]>svg:first-child]:rotate-0">
        <ChevronDown className="size-4 shrink-0 text-muted-foreground transition-transform" />
        <BookOpen className="size-4 shrink-0 text-muted-foreground" />
        <span className="font-semibold">{subjectLabel}</span>
        {offering.snapshotSubjectTitle && (
          <span className="text-xs text-muted-foreground">
            — {offering.snapshotSubjectTitle}
          </span>
        )}
        <Badge variant="destructive" className="ml-auto text-xs">
          {validationIssueCount}{" "}
          {validationIssueCount === 1
            ? "validation issue"
            : "validation issues"}
        </Badge>
      </CollapsibleTrigger>
      <CollapsibleContent>
        <div className="mt-2 space-y-2">
          {offering.validationIssues?.map((issue, idx) => (
            <ValidationIssueCard
              key={issue.id ?? idx}
              validationIssue={issue}
            />
          ))}
        </div>
      </CollapsibleContent>
    </Collapsible>
  );
}
