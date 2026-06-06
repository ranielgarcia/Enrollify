import type { ConflictResult } from "@/api/models/offering";
import { Badge } from "@/components/ui/badge";
import { Link } from "@tanstack/react-router";
import { AlertCircle, AlertTriangle, Info, ArrowRight } from "lucide-react";

interface ConflictCardProps {
  conflict: ConflictResult;
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

const TYPE_LABELS: Record<string, string> = {
  TEACHER_DOUBLE_BOOKED: "Teacher Double-Booked",
  ROOM_DOUBLE_BOOKED: "Room Double-Booked",
  SECTION_OVERLAP: "Section Overlap",
  TEACHER_OVERLOAD: "Teacher Overload",
};

export function ConflictCard({ conflict }: ConflictCardProps) {
  const config = SEVERITY_CONFIG[conflict.severity];
  const Icon = config.icon;
  const typeLabel = TYPE_LABELS[conflict.type] ?? conflict.type;

  return (
    <div className={`rounded-lg border p-4 space-y-3 ${config.containerClass}`}>
      <div className="flex items-start gap-3">
        <Icon className={`size-5 mt-0.5 shrink-0 ${config.iconClass}`} />
        <div className="flex-1 space-y-1">
          <div className="flex items-center gap-2 flex-wrap">
            <Badge variant={config.badgeVariant} className="text-xs">
              {typeLabel}
            </Badge>
            {conflict.day && conflict.startTime && conflict.endTime && (
              <span className="text-xs text-muted-foreground font-mono">
                {conflict.day} · {conflict.startTime}–{conflict.endTime}
              </span>
            )}
          </div>
          <p className="text-sm text-foreground">{conflict.message}</p>
        </div>
      </div>

      {conflict.affectedOfferings && conflict.affectedOfferings.length > 0 && (
        <div className="pl-8 space-y-1.5">
          <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide">
            Affected Offerings
          </p>
          {conflict.affectedOfferings.map((o) => (
            <div
              key={o.id}
              className="flex items-center gap-2 text-xs text-muted-foreground"
            >
              <ArrowRight className="size-3 shrink-0" />
              <span className="font-mono font-medium">{o.subject.code}</span>
              <span>{o.subject.title}</span>
              <span className="text-muted-foreground/60">·</span>
              <span>
                <Link
                  to={`/portal/curriculum-and-scheduling/sections-details/$sectionId`}
                  params={{ sectionId: o.section.id.toString() }}
                >
                  {o.section.name}
                </Link>
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
