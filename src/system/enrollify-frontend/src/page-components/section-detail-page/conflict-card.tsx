import type { ValidationIssue } from "@/api/models/class-scheduling/offering";
import { Badge } from "@/components/ui/badge";
import { Link } from "@tanstack/react-router";
import { AlertCircle, AlertTriangle, Info, ArrowRight } from "lucide-react";

interface ConflictCardProps {
  validationIssue: ValidationIssue;
}

const SEVERITY_CONFIG = {
  Error: {
    icon: AlertCircle,
    iconClass: "text-destructive",
    badgeVariant: "destructive" as const,
    containerClass: "border-destructive/50 bg-destructive/5",
  },
  Warning: {
    icon: AlertTriangle,
    iconClass: "text-amber-600",
    badgeVariant: "secondary" as const,
    containerClass: "border-amber-400/50 bg-amber-50 dark:bg-amber-950/20",
  },
  Info: {
    icon: Info,
    iconClass: "text-blue-500",
    badgeVariant: "outline" as const,
    containerClass: "border-blue-300/50 bg-blue-50 dark:bg-blue-950/20",
  },
};

export function ConflictCard({ validationIssue }: ConflictCardProps) {
  const config = SEVERITY_CONFIG[validationIssue.type.severity];
  const Icon = config.icon;
  const typeLabel = validationIssue.type.label;

  return (
    <div className={`rounded-lg border p-4 space-y-3 ${config.containerClass}`}>
      <div className="flex items-start gap-3">
        <Icon className={`size-5 mt-0.5 shrink-0 ${config.iconClass}`} />
        <div className="flex-1 space-y-1">
          <div className="flex items-center gap-2 flex-wrap">
            <Badge variant={config.badgeVariant} className="text-xs">
              {typeLabel}
            </Badge>
            {validationIssue.dayOfWeek &&
              validationIssue.startTime &&
              validationIssue.endTime && (
                <span className="text-xs text-muted-foreground font-mono">
                  {validationIssue.dayOfWeek} · {validationIssue.startTime}–
                  {validationIssue.endTime}
                </span>
              )}
          </div>
          <p className="text-sm text-foreground">{validationIssue.message}</p>
        </div>
      </div>

      {validationIssue.conflictingOfferings &&
        validationIssue.conflictingOfferings.length > 0 && (
          <div className="pl-8 space-y-1.5">
            <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide">
              Affected Offerings
            </p>
            {validationIssue.conflictingOfferings.map((o) => (
              <div
                key={o.id}
                className="flex items-center gap-2 text-xs text-muted-foreground"
              >
                <ArrowRight className="size-3 shrink-0" />
                <span className="font-mono font-medium">{o.subject.code}</span>
                <span>{o.subject.title}</span>
                <span className="text-muted-foreground/60">·</span>
                <Link
                  to={`/portal/curriculum-and-scheduling/sections-details/$sectionId`}
                  params={{ sectionId: o.section.id.toString() }}
                  className="font-medium text-primary underline underline-offset-2 hover:text-primary/80"
                >
                  {o.section.name}
                </Link>
              </div>
            ))}
          </div>
        )}
    </div>
  );
}
