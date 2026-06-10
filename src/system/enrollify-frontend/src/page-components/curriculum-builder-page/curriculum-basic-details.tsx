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

function StatusBadge({ statusValue, statusName }: { statusValue: number; statusName: string }) {
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
    <div className="relative overflow-hidden rounded-xl border bg-linear-to-br from-primary/5 via-background to-background p-6 shadow-sm">
      <div className="pointer-events-none absolute -right-8 -top-8 h-40 w-40 rounded-full bg-primary/8 blur-2xl" />

      <div className="relative flex items-start justify-between gap-4">
        <div className="flex items-center gap-4">
          <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-primary/10 ring-1 ring-primary/20">
            <BookOpen className="h-5 w-5 text-primary" />
          </div>
          <div className="space-y-1">
            <div className="flex items-center gap-2 flex-wrap">
              <h2 className="text-xl font-bold tracking-tight">
                {curriculum.course.name}
              </h2>
              <StatusBadge
                statusValue={curriculum.status.value}
                statusName={curriculum.status.name}
              />
            </div>
            <div className="flex items-center gap-3 text-sm text-muted-foreground flex-wrap">
              <span>
                Version{" "}
                <span className="font-medium text-foreground">
                  {curriculum.version}
                </span>
              </span>
              <span>·</span>
              <span>
                Effective{" "}
                <span className="font-medium text-foreground">
                  {curriculum.effectiveYear}
                </span>
              </span>
              {curriculum.description && (
                <>
                  <span>·</span>
                  <span className="italic">{curriculum.description}</span>
                </>
              )}
            </div>
          </div>
        </div>

        <div className="flex items-center gap-2 shrink-0">
          {!isReadOnly && (
            <Button
              variant="outline"
              size="sm"
              className="gap-1.5 shadow-sm"
              onClick={onEditDetails}
            >
              <Edit2 className="size-3.5" />
              Edit
            </Button>
          )}
          <Button
            variant="outline"
            size="sm"
            className="gap-1.5 shadow-sm"
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
