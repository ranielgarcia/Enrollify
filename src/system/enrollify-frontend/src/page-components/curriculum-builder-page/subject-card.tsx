import {
  SearchableSelectWithCustomTrigger,
  type MultiSearchableSelectWithTriggerOption,
} from "@/components/form/searchable-select-with-custom-trigger";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Trash2 } from "lucide-react";

export interface SubjectInCurriculum {
  id: number;
  code: string;
  title: string;
  units: number;
  unitsOverride: number | null;
  prerequisites: string[];
}

interface SubjectCardProps {
  subject: SubjectInCurriculum;
  isReadOnly: boolean;
  availablePrerequisiteOptions: MultiSearchableSelectWithTriggerOption[];
  onRemove: () => void;
  onUnitsOverrideChange: (value: number | null) => void;
  onRemovePrerequisite: (prerequisiteCode: string) => void;
  onAddPrerequisites: (subjectCodes: string[]) => void;
}

export function SubjectCard({
  subject,
  isReadOnly,
  availablePrerequisiteOptions,
  onRemove,
  onUnitsOverrideChange,
  onRemovePrerequisite,
  onAddPrerequisites,
}: SubjectCardProps) {
  return (
    <div className="group p-3 border rounded-lg bg-background hover:shadow-sm transition-all flex items-start justify-between gap-2">
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 mb-1">
          <span className="text-[10px] font-bold text-accent uppercase">
            {subject.code}
          </span>
          <span className="text-[10px] bg-accent/10 text-accent px-1.5 py-0.5 rounded-full font-medium">
            {subject.unitsOverride ?? subject.units} Units
            {subject.unitsOverride !== null && (
              <span className="ml-1 line-through text-muted-foreground">
                {subject.units}
              </span>
            )}
          </span>
          <input
            type="number"
            min={0}
            step={0.5}
            placeholder={`Override (${subject.units})`}
            value={subject.unitsOverride ?? ""}
            disabled={isReadOnly}
            onChange={(e) => {
              const raw = e.target.value;
              onUnitsOverrideChange(raw === "" ? null : Number(raw));
            }}
            className="w-20 text-[10px] h-5 px-1.5 rounded border border-dashed border-muted-foreground/40 bg-transparent focus:outline-none focus:border-accent placeholder:text-muted-foreground/50 disabled:opacity-50 disabled:cursor-not-allowed"
          />
        </div>
        <p className="text-sm font-semibold truncate leading-tight">
          {subject.title}
        </p>

        {subject.prerequisites.length > 0 && (
          <div className="mt-2 flex flex-wrap gap-1">
            {subject.prerequisites.map((pre) => (
              <Badge
                key={pre}
                variant="outline"
                className="text-[9px] px-1 h-5 border-dashed bg-accent/5 gap-1 group/badge"
              >
                Pre: {pre}
                {!isReadOnly && (
                  <button
                    type="button"
                    className="ml-0.5 rounded-full hover:bg-destructive/20 p-0.5 transition-colors"
                    onClick={(e) => {
                      e.stopPropagation();
                      onRemovePrerequisite(pre);
                    }}
                  >
                    <Trash2 className="size-2.5 text-destructive" />
                  </button>
                )}
              </Badge>
            ))}
          </div>
        )}

        {!isReadOnly && (
          <SearchableSelectWithCustomTrigger
            options={availablePrerequisiteOptions}
            value={[]}
            onValueChange={(subjectCodes: string[]) => {
              onAddPrerequisites(subjectCodes);
            }}
            searchPlaceholder="Search subjects..."
            emptyMessage="No subjects found"
            trigger={
              <Button
                variant="ghost"
                size="sm"
                className="h-auto p-0 text-[9px] text-muted-foreground italic hover:text-accent flex flex-wrap gap-1 justify-start"
              >
                <span>+ Add Prerequisite</span>
              </Button>
            }
          />
        )}
      </div>
      {!isReadOnly && (
        <Button
          variant="ghost"
          size="sm"
          className="size-7 p-0 opacity-0 group-hover:opacity-100 text-destructive transition-opacity"
          onClick={onRemove}
        >
          <Trash2 className="size-3.5" />
        </Button>
      )}
    </div>
  );
}
