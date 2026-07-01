import type { ValidationIssue } from "@/api/models/class-scheduling/offering";
import { CheckCircle, AlertCircle, AlertTriangle } from "lucide-react";
import { ConflictCard } from "./conflict-card";

interface ConflictsTabProps {
  validationIssues: ValidationIssue[];
}

export function ConflictsTab({ validationIssues }: ConflictsTabProps) {
  console.log(validationIssues);
  const hardConflicts = validationIssues.filter(
    (c) => c.type.severity === "Error",
  );
  const warnings = validationIssues.filter(
    (c) => c.type.severity === "Warning",
  );
  const infos = validationIssues.filter((c) => c.type.severity === "Info");

  if (validationIssues.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center gap-4">
        <div className="rounded-full bg-green-100 dark:bg-green-950/30 p-4">
          <CheckCircle className="size-8 text-green-600" />
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
    <div className="space-y-6">
      {hardConflicts.length > 0 && (
        <section className="space-y-3">
          <div className="flex items-center gap-2">
            <AlertCircle className="size-5 text-destructive" />
            <h3 className="font-semibold text-destructive">
              Hard Conflicts ({hardConflicts.length})
            </h3>
          </div>
          <p className="text-xs text-muted-foreground">
            These must be resolved before the schedule can be published.
          </p>
          <div className="space-y-3">
            {hardConflicts.map((c, i) => (
              <ConflictCard key={c.id ?? i} validationIssue={c} />
            ))}
          </div>
        </section>
      )}

      {warnings.length > 0 && (
        <section className="space-y-3">
          <div className="flex items-center gap-2">
            <AlertTriangle className="size-5 text-amber-600" />
            <h3 className="font-semibold text-amber-700 dark:text-amber-500">
              Warnings ({warnings.length})
            </h3>
          </div>
          <p className="text-xs text-muted-foreground">
            These are advisory — review before publishing.
          </p>
          <div className="space-y-3">
            {warnings.map((c, i) => (
              <ConflictCard key={c.id ?? i} validationIssue={c} />
            ))}
          </div>
        </section>
      )}

      {infos.length > 0 && (
        <section className="space-y-3">
          <h3 className="font-semibold text-blue-700 dark:text-blue-400">
            Information ({infos.length})
          </h3>
          <div className="space-y-3">
            {infos.map((c, i) => (
              <ConflictCard key={c.id ?? i} validationIssue={c} />
            ))}
          </div>
        </section>
      )}
    </div>
  );
}
