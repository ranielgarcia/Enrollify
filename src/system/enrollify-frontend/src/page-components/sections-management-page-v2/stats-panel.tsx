import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import {
  AlertCircle,
  Database,
  FileText,
  FolderOpen,
  Gauge,
  Scale,
  Wrench,
  XCircle,
} from "lucide-react";

interface StatsPanelProps {
  stats: Record<string, number>;
}

interface StatItemProps {
  icon: React.ReactNode;
  label: string;
  value: number;
  className?: string;
}

const STATS_KEYS = {
  OpenSections: "OPEN_SECTIONS",
  DraftSections: "DRAFT_SECTIONS",
  CancelledSections: "CANCELLED_SECTIONS",
  ScheduleConflicts: "SCHEDULE_CONFLICTS",
  DataInconsistencies: "DATA_INCONSISTENCIES",
  MissingRequirements: "MISSING_REQUIREMENTS",
  CapacityConstraints: "CAPACITY_CONSTRAINTS",
  ResourceMisalignments: "RESOURCE_MISALIGNMENTS",
  SchedulePolicyViolations: "SCHEDULE_POLICY_VIOLATIONS",
  DefaultValues: "DEFAULT_VALUES",
};

function StatItem({ icon, label, value, className }: StatItemProps) {
  return (
    <Button variant="outline" className={cn("h-auto flex-col items-start gap-2 p-4", className)}>
      <div className="flex items-center gap-2">
        {icon}
        <span className="text-sm font-medium">{label}</span>
      </div>
      <span className="text-2xl font-bold">{value}</span>
    </Button>
  );
}

export function StatsPanel({ stats }: StatsPanelProps) {
  return (
    <Card className="p-6">
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-8">
        <StatItem
          icon={<FileText className="size-4" />}
          label="Draft"
          value={stats[STATS_KEYS.DraftSections] ?? 0}
          className="bg-secondary text-secondary-foreground hover:bg-secondary/80"
        />
        <StatItem
          icon={<FolderOpen className="size-4" />}
          label="Open"
          value={stats[STATS_KEYS.OpenSections] ?? 0}
          className="bg-emerald-100 text-emerald-800 dark:bg-emerald-900/30 dark:text-emerald-200 hover:bg-emerald-200 dark:hover:bg-emerald-800/40"
        />
        <StatItem
          icon={<XCircle className="size-4" />}
          label="Cancelled"
          value={stats[STATS_KEYS.CancelledSections] ?? 0}
          className="bg-destructive/15 text-destructive hover:bg-destructive/25"
        />
        <StatItem
          icon={<AlertCircle className="size-4" />}
          label="Conflicts"
          value={stats[STATS_KEYS.ScheduleConflicts] ?? 0}
          className="bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-200 hover:bg-amber-200 dark:hover:bg-amber-800/40"
        />
        <StatItem
          icon={<Database className="size-4" />}
          label="Data Issues"
          value={stats[STATS_KEYS.DataInconsistencies] ?? 0}
          className="bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-200 hover:bg-blue-200 dark:hover:bg-blue-800/40"
        />
        <StatItem
          icon={<Gauge className="size-4" />}
          label="Constraints"
          value={stats[STATS_KEYS.CapacityConstraints] ?? 0}
          className="bg-violet-100 text-violet-800 dark:bg-violet-900/30 dark:text-violet-200 hover:bg-violet-200 dark:hover:bg-violet-800/40"
        />
        <StatItem
          icon={<Wrench className="size-4" />}
          label="Misalignments"
          value={stats[STATS_KEYS.ResourceMisalignments] ?? 0}
          className="bg-cyan-100 text-cyan-800 dark:bg-cyan-900/30 dark:text-cyan-200 hover:bg-cyan-200 dark:hover:bg-cyan-800/40"
        />
        <StatItem
          icon={<Scale className="size-4" />}
          label="Violations"
          value={(stats[STATS_KEYS.SchedulePolicyViolations] ?? 0) + (stats[STATS_KEYS.MissingRequirements] ?? 0)}
          className="bg-rose-100 text-rose-800 dark:bg-rose-900/30 dark:text-rose-200 hover:bg-rose-200 dark:hover:bg-rose-800/40"
        />
      </div>
    </Card>
  );
}
