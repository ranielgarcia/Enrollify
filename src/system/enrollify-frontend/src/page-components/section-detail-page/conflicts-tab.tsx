import type { Offering } from "@/api/models/class-scheduling/offering";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { AlertTriangle, CheckCircle2, Siren } from "lucide-react";
import { OfferingValidationSection } from "@/components/scheduling/offering-validation-section";

interface ConflictsTabProps {
  offerings: Offering[];
}

export function ConflictsTab({ offerings }: ConflictsTabProps) {
  const offeringsWithIssues = offerings.filter(
    (o) => (o.validationIssues?.length ?? 0) > 0,
  );

  const totalValidationIssues = offeringsWithIssues.reduce(
    (sum, o) => sum + (o.validationIssues?.length ?? 0),
    0,
  );

  const errorValidationIssues = offeringsWithIssues.reduce(
    (sum, o) =>
      sum +
      (o.validationIssues?.filter((issue) => issue.type.severity === "Error")
        .length ?? 0),
    0,
  );

  if (offeringsWithIssues.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center gap-4">
        <div className="rounded-full bg-green-100 dark:bg-green-950/30 p-4">
          <CheckCircle2 className="size-8 text-green-600" />
        </div>
        <div>
          <p className="text-base font-semibold text-foreground">
            No conflicts detected
          </p>
          <p className="text-sm text-muted-foreground mt-1">
            All schedule assignments look good.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-4">
      {errorValidationIssues > 0 && (
        <Alert variant="destructive">
          <AlertTitle>Cannot open for enrollment</AlertTitle>
          <AlertDescription>
            {errorValidationIssues} Error-severity conflict
            {errorValidationIssues !== 1 ? "s" : ""} must be resolved in the
            Room Scheduler before this section can be opened.
          </AlertDescription>
        </Alert>
      )}

      <div className="flex items-center gap-3 rounded-lg border bg-muted/30 p-3">
        <div className="flex items-center gap-2 text-sm">
          <Siren className="size-4 text-rose-500" />
          <span className="font-semibold tabular-nums text-rose-600 dark:text-rose-400">
            {totalValidationIssues}
          </span>
          <span className="text-muted-foreground">
            total validation issue
            {totalValidationIssues !== 1 ? "s" : ""}
          </span>
        </div>
        <div className="h-4 w-px bg-border" />
        <div className="flex items-center gap-2 text-sm">
          <AlertTriangle className="size-4 text-rose-500" />
          <span className="font-semibold tabular-nums text-rose-600 dark:text-rose-400">
            {errorValidationIssues}
          </span>
          <span className="text-muted-foreground">blocking enrollment</span>
        </div>
      </div>

      <div className="space-y-4">
        {offeringsWithIssues.map((offering) => (
          <OfferingValidationSection key={offering.id} offering={offering} />
        ))}
      </div>
    </div>
  );
}
