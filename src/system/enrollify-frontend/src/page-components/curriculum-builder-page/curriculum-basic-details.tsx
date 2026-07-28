import {
  CurriculumStatusEnum,
  type CurriculumWithSubjects,
} from "@/api/models/curriculum";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { BookOpen, Edit2, X } from "lucide-react";

interface CurriculumBasicDetailsProps {
  curriculum: CurriculumWithSubjects;
  onEditDetails: () => void;
  onClose: () => void;
}

function StatusBadge({
  statusValue,
  statusName,
}: {
  statusValue: number;
  statusName: string;
}) {
  if (statusValue === CurriculumStatusEnum.Active) {
    return (
      <Badge className="text-[11px] px-2 py-0.5 font-semibold bg-emerald-500/15 text-emerald-600 border border-emerald-500/25 hover:bg-emerald-500/15">
        ● {statusName}
      </Badge>
    );
  }
  if (statusValue === CurriculumStatusEnum.PhaseOut) {
    return (
      <Badge className="text-[11px] px-2 py-0.5 font-semibold bg-amber-500/15 text-amber-600 border border-amber-500/25 hover:bg-amber-500/15">
        ● {statusName}
      </Badge>
    );
  }
  if (statusValue === CurriculumStatusEnum.Archived) {
    return (
      <Badge className="text-[11px] px-2 py-0.5 font-semibold bg-muted text-muted-foreground border hover:bg-muted">
        ● {statusName}
      </Badge>
    );
  }
  // Draft
  return (
    <Badge className="text-[11px] px-2 py-0.5 font-semibold bg-muted text-muted-foreground border hover:bg-muted">
      ● {statusName}
    </Badge>
  );
}

export default function CurriculumBasicDetails({
  curriculum,
  onEditDetails,
  onClose,
}: CurriculumBasicDetailsProps) {
  const isReadOnly = curriculum.status.value === CurriculumStatusEnum.Active;

  return (
    <div className="rounded-lg border border-l-4 border-l-primary bg-card px-5 py-4">
      <div className="flex items-start justify-between gap-4">
        <div className="flex items-start gap-3">
          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-surface-sunken text-primary mt-0.5">
            <BookOpen className="h-4 w-4" />
          </div>
          <div className="space-y-1.5">
            <div className="flex items-center gap-2.5 flex-wrap">
              <h2 className="text-lg font-bold tracking-tight leading-none">
                {curriculum.course.name}
              </h2>
              <StatusBadge
                statusValue={curriculum.status.value}
                statusName={curriculum.status.name}
              />
            </div>
            <div className="flex items-center gap-2.5 text-xs text-muted-foreground flex-wrap">
              <span className="flex items-center gap-1">
                <span>Version</span>
                <span className="font-semibold text-foreground nums">
                  {curriculum.version}
                </span>
              </span>
              <span className="text-border">·</span>
              <span className="flex items-center gap-1">
                <span>Effective</span>
                <span className="font-semibold text-foreground nums">
                  {curriculum.effectiveYear}
                </span>
              </span>
              {curriculum.description && (
                <>
                  <span className="text-border">·</span>
                  <span className="italic text-muted-foreground/70">
                    {curriculum.description}
                  </span>
                </>
              )}
            </div>
          </div>
        </div>

        <div className="flex items-center gap-1.5 shrink-0">
          {!isReadOnly && (
            <Button
              variant="ghost"
              size="sm"
              className="h-8 gap-1.5 text-xs"
              onClick={onEditDetails}
            >
              <Edit2 className="size-3.5" />
              Edit
            </Button>
          )}
          <Button
            variant="ghost"
            size="sm"
            className="h-8 gap-1.5 text-xs text-muted-foreground"
            onClick={onClose}
          >
            <X className="size-3.5" />
            Close
          </Button>
        </div>
      </div>
    </div>
  );
}
