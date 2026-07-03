import type { ValidationIssue } from "@/api/models/class-scheduling/offering";
import { Badge } from "@/components/ui/badge";
import { Link } from "@tanstack/react-router";
import { BookOpen, Clock, DoorOpen } from "lucide-react";
import { cn } from "@/lib/utils";
import SeverityConfig from "@/config/severity-config";

interface ValidationIssueCardProps {
  validationIssue: ValidationIssue;
}

export function ValidationIssueCard({
  validationIssue,
}: ValidationIssueCardProps) {
  const severity = validationIssue.type?.severity ?? "Error";
  const config = SeverityConfig[severity];
  const Icon = config.icon;

  return (
    <div
      className={cn("rounded-lg border p-3 space-y-2", config.containerClass)}
    >
      <div className="flex items-start gap-2">
        <Icon className={cn("size-4 mt-0.5 shrink-0", config.iconClass)} />
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 mb-1 flex-wrap">
            <Badge variant={config.badgeVariant} className="text-xs">
              {validationIssue.type?.label ?? severity}
            </Badge>
            <span className="text-xs font-mono text-muted-foreground">
              {validationIssue.type?.code}
            </span>
          </div>
          <p className="text-sm font-medium">{validationIssue.message}</p>

          {validationIssue.dayOfWeek && (
            <div className="flex items-center gap-1.5 text-xs text-muted-foreground mt-1">
              <Clock className="size-3" />
              {validationIssue.dayOfWeek}{" "}
              {validationIssue.startTime?.substring(0, 5)}–
              {validationIssue.endTime?.substring(0, 5)}
            </div>
          )}
        </div>
      </div>

      {validationIssue.conflictingOfferings &&
        validationIssue.conflictingOfferings.length > 0 && (
          <div className="pl-6 space-y-1">
            <div className="text-xs font-medium text-muted-foreground">
              Conflicts with:
            </div>
            {validationIssue.conflictingOfferings.map((co, idx) => (
              <div
                key={co.id ?? idx}
                className="flex items-center gap-2 text-xs rounded border bg-background px-2 py-1.5"
              >
                <BookOpen className="size-3 text-muted-foreground shrink-0" />
                <span className="font-medium">{co.subject.code}</span>
                <span className="text-muted-foreground">
                  {co.subject.title}
                </span>
                <span className="text-muted-foreground">·</span>
                <Link
                  to="/portal/curriculum-and-scheduling/sections-details/$sectionId"
                  params={{ sectionId: co.section.id.toString() }}
                  className="font-medium text-primary underline underline-offset-2 hover:text-primary/80"
                >
                  {co.section.name}
                </Link>
                {co.room && (
                  <>
                    <span className="text-muted-foreground">·</span>
                    <span className="flex items-center gap-1 text-muted-foreground">
                      <DoorOpen className="size-3" />
                      {co.room.roomNumber}
                    </span>
                  </>
                )}
              </div>
            ))}
          </div>
        )}
    </div>
  );
}
