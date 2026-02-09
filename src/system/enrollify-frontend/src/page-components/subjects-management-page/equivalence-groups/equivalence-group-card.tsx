import type { SubjectEquivalenceGroup } from "@/api/models/subject-equivalence";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Edit2, Package, Plus, Trash2, X } from "lucide-react";

interface EquivalenceGroupCardProps {
  group: SubjectEquivalenceGroup;
  onEdit: (group: SubjectEquivalenceGroup) => void;
  onDelete: (group: SubjectEquivalenceGroup) => void;
  onAddSubject: (group: SubjectEquivalenceGroup) => void;
  onRemoveSubject: (
    group: SubjectEquivalenceGroup,
    subject: { id: number; code: string; title: string; units: number },
  ) => void;
}

export function EquivalenceGroupCard({
  group,
  onEdit,
  onDelete,
  onAddSubject,
  onRemoveSubject,
}: EquivalenceGroupCardProps) {
  const subjects = group.subjects ?? [];

  return (
    <Card className="hover:border-accent/50 transition-colors">
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between">
          <div className="flex items-center gap-2">
            <Package className="size-5 text-accent" />
            <CardTitle className="text-lg">{group.name}</CardTitle>
          </div>
          <div className="flex gap-1">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onEdit(group)}
              className="hover:bg-blue-500/10 text-blue-600 hover:text-blue-700"
            >
              <Edit2 className="size-4" />
            </Button>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onDelete(group)}
              className="text-destructive hover:bg-destructive/10"
            >
              <Trash2 className="size-4" />
            </Button>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <div className="flex flex-wrap gap-2">
          {subjects.map((subject) => (
            <Badge
              key={subject.id}
              variant="secondary"
              className="text-sm py-1.5 px-3 flex items-center gap-2 group hover:bg-secondary/80"
            >
              <span className="font-semibold">{subject.code}</span>
              <span className="text-muted-foreground">-</span>
              <span className="truncate max-w-[150px]">{subject.title}</span>
              <span className="text-xs text-muted-foreground">
                ({subject.units} units)
              </span>
              <button
                onClick={() => onRemoveSubject(group, subject)}
                className="ml-1 opacity-0 group-hover:opacity-100 transition-opacity hover:text-destructive"
                title="Remove from group"
              >
                <X className="size-3" />
              </button>
            </Badge>
          ))}

          {subjects.length === 0 && (
            <p className="text-sm text-muted-foreground italic">
              No subjects in this group
            </p>
          )}

          <Button
            variant="outline"
            size="sm"
            onClick={() => onAddSubject(group)}
            className="h-7 text-xs"
          >
            <Plus className="size-3 mr-1" />
            Add Subject
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
