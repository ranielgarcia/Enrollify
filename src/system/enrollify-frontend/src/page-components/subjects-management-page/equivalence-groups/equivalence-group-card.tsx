import type { SubjectEquivalenceGroup } from "@/api/models/subject-equivalence";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Edit2, Package, Plus, Trash2 } from "lucide-react";
import { SubjectBadge } from "./subject-badge";
import { useEffect, useState } from "react";
import { useAuthorization } from "@/infrastructure/authorization/components/useAuthorization";

interface EquivalenceGroupCardProps {
  group: SubjectEquivalenceGroup;
  onEdit: (group: SubjectEquivalenceGroup) => void;
  onDelete: (group: SubjectEquivalenceGroup) => void;
  onAddSubject: (group: SubjectEquivalenceGroup) => void;
}

export function EquivalenceGroupCard({
  group,
  onEdit,
  onDelete,
  onAddSubject,
}: EquivalenceGroupCardProps) {
  const subjects = group.subjects ?? [];
  const { checkPolicy } = useAuthorization();
  const [canUpdate, setCanUpdate] = useState(false);
  const [canDelete, setCanDelete] = useState(false);

  useEffect(() => {
    const checkPolicies = async () => {
      const [updatePermission, deletePermission] = await Promise.all([
        checkPolicy("canUpdateSubjectEquivalenceGroup"),
        checkPolicy("canDeleteSubjectEquivalenceGroup"),
      ]);
      setCanUpdate(updatePermission);
      setCanDelete(deletePermission);
    };

    checkPolicies();
  }, [checkPolicy]);

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
              disabled={!canUpdate}
            >
              <Edit2 className="size-4" />
            </Button>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onDelete(group)}
              className="text-destructive hover:bg-destructive/10"
              disabled={!canDelete}
            >
              <Trash2 className="size-4" />
            </Button>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <div className="flex flex-wrap gap-2">
          {subjects.map((subject) => (
            <SubjectBadge key={subject.id} subject={subject} group={group} />
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
