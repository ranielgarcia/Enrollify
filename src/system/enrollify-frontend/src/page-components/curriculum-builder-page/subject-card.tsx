import {
  SearchableSelectWithCustomTrigger,
  type MultiSearchableSelectWithTriggerOption,
} from "@/components/form/searchable-select-with-custom-trigger";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { CalendarDays, Trash2 } from "lucide-react";

export interface SubjectInCurriculum {
  id: number;
  code: string;
  title: string;
  units: number;
  unitsOverride: number | null;
  prerequisites: string[];
  daysPerWeek: number;
  hoursPerDay: number;
}

interface SubjectCardProps {
  subject: SubjectInCurriculum;
  isReadOnly: boolean;
  availablePrerequisiteOptions: MultiSearchableSelectWithTriggerOption[];
  onRemove: () => void;
  onUnitsOverrideChange: (value: number | null) => void;
  onRemovePrerequisite: (prerequisiteCode: string) => void;
  onAddPrerequisites: (subjectCodes: string[]) => void;
  onDaysPerWeekChange: (value: number) => void;
  onHoursPerDayChange: (value: number) => void;
}

export function SubjectCard({
  subject,
  isReadOnly,
  availablePrerequisiteOptions,
  onRemove,
  onUnitsOverrideChange,
  onRemovePrerequisite,
  onAddPrerequisites,
  onDaysPerWeekChange,
  onHoursPerDayChange,
}: SubjectCardProps) {
  return (
    <div className="group rounded-lg border border-l-2 border-l-accent/20 bg-card hover:border-accent/50 transition-colors">
      <div className="p-3 space-y-2.5">
        {/* Header: code, units, override input, delete */}
        <div className="flex items-start justify-between gap-2">
          <div className="flex items-center gap-2 min-w-0">
            <span className="shrink-0 text-[10px] font-bold text-accent uppercase tracking-wider">
              {subject.code}
            </span>
            <div className="flex items-center gap-1">
              <span className="text-[10px] bg-accent/10 text-accent px-1.5 py-0.5 rounded-full font-medium tabular-nums">
                {subject.unitsOverride ?? subject.units} Units
                {subject.unitsOverride !== null && (
                  <span className="ml-1 line-through text-muted-foreground/60">
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
                className="w-16 text-[9px] h-4.5 px-1 rounded border border-dashed border-muted-foreground/30 bg-transparent focus:outline-none focus:border-accent placeholder:text-muted-foreground/40 disabled:opacity-50 disabled:cursor-not-allowed tabular-nums"
              />
            </div>
          </div>
          {!isReadOnly && (
            <Button
              variant="ghost"
              size="sm"
              className="size-6 p-0 shrink-0 opacity-0 group-hover:opacity-100 text-muted-foreground hover:text-destructive transition-all"
              onClick={onRemove}
            >
              <Trash2 className="size-3.5" />
            </Button>
          )}
        </div>

        {/* Subject Title */}
        <p className="text-sm font-semibold leading-tight text-card-foreground">
          {subject.title}
        </p>

        {/* Schedule Section */}
        <div className="rounded-lg bg-muted/20 p-2.5 space-y-1.5">
          <span className="text-[10px] font-semibold uppercase tracking-wider text-muted-foreground/70 flex items-center gap-1.5">
            <CalendarDays className="size-3" />
            Schedule
          </span>
          <div className="flex items-center gap-3">
            <div className="flex items-center gap-1.5">
              <label className="text-[10px] text-muted-foreground shrink-0">
                Days/Week
              </label>
              <input
                type="number"
                min={1}
                max={7}
                step={1}
                placeholder="—"
                value={subject.daysPerWeek ?? ""}
                disabled={isReadOnly}
                onChange={(e) => {
                  const raw = e.target.value;
                  onDaysPerWeekChange(raw === "" ? 1 : Number(raw));
                }}
                className="w-14 text-xs h-7 px-2 rounded-md border border-input bg-background focus:outline-none focus:ring-1 focus:ring-ring placeholder:text-muted-foreground/40 disabled:opacity-50 disabled:cursor-not-allowed tabular-nums"
              />
            </div>
            <div className="flex items-center gap-1.5">
              <label className="text-[10px] text-muted-foreground shrink-0">
                Hours/Day
              </label>
              <input
                type="number"
                min={0.5}
                max={12}
                step={0.5}
                placeholder="—"
                value={subject.hoursPerDay ?? ""}
                disabled={isReadOnly}
                onChange={(e) => {
                  const raw = e.target.value;
                  onHoursPerDayChange(raw === "" ? 1 : Number(raw));
                }}
                className="w-14 text-xs h-7 px-2 rounded-md border border-input bg-background focus:outline-none focus:ring-1 focus:ring-ring placeholder:text-muted-foreground/40 disabled:opacity-50 disabled:cursor-not-allowed tabular-nums"
              />
            </div>
            {subject.daysPerWeek != null && subject.hoursPerDay != null && (
              <span className="text-[10px] text-muted-foreground/60 tabular-nums">
                = {(subject.daysPerWeek * subject.hoursPerDay).toFixed(1)}h/wk
              </span>
            )}
          </div>
        </div>

        {/* Prerequisites */}
        {subject.prerequisites.length > 0 && (
          <div className="flex flex-wrap gap-1">
            {subject.prerequisites.map((pre) => (
              <Badge
                key={pre}
                variant="outline"
                className="text-[9px] px-1.5 h-5 border-dashed bg-accent/5 gap-1 group/badge"
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
    </div>
  );
}
