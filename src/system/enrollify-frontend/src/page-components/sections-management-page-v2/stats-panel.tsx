import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import type { SectionStats } from "@/api/models/section-stats";
import {
  FileText,
  FolderOpen,
  XCircle,
  AlertCircle,
  BarChart3,
  Clock,
} from "lucide-react";

interface StatsPanelProps {
  stats: SectionStats;
}

interface StatItemProps {
  icon: React.ReactNode;
  label: string;
  value: number;
}

function StatItem({ icon, label, value }: StatItemProps) {
  return (
    <Button
      variant="outline"
      className="h-auto flex-col items-start gap-2 p-4"
    >
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
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6">
        <StatItem
          icon={<FileText className="size-4" />}
          label="Draft"
          value={stats.totalDraft}
        />
        <StatItem
          icon={<FolderOpen className="size-4" />}
          label="Open"
          value={stats.totalOpen}
        />
        <StatItem
          icon={<XCircle className="size-4" />}
          label="Cancelled"
          value={stats.totalCancelled}
        />
        <StatItem
          icon={<AlertCircle className="size-4" />}
          label="Errors"
          value={stats.sectionsWithUnresolvedErrors}
        />
        <StatItem
          icon={<BarChart3 className="size-4" />}
          label="Conflicts"
          value={stats.sectionsWithConflicts}
        />
        <StatItem
          icon={<Clock className="size-4" />}
          label="Unscheduled"
          value={stats.unscheduledCount}
        />
      </div>
    </Card>
  );
}
