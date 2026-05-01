import type { SubjectEquivalenceGroup } from "@/api/models/subject-equivalence";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Package, Plus, Search } from "lucide-react";
import { useState, useMemo } from "react";
import { EquivalenceGroupCard } from "./equivalence-group-card";
import { AddSubjectToGroupDialog } from "./add-subject-to-group-dialog";
import {
  addSubjectsToEquivalenceGroupOptions,
  getAllSubjectEquivalenceGroupsOptions,
} from "@/api/collections/subject-equivalence-group-collection";
import { useMutation, useQuery } from "@tanstack/react-query";

interface EquivalenceGroupsTabProps {
  onEdit: (group: SubjectEquivalenceGroup) => void;
  onDelete: (group: SubjectEquivalenceGroup) => void;
  onCreateNew: () => void;
}

export function EquivalenceGroupsTab({
  onEdit,
  onDelete,
  onCreateNew,
}: EquivalenceGroupsTabProps) {
  const [searchQuery, setSearchQuery] = useState("");

  // Add subject dialog state
  const [isAddSubjectDialogOpen, setIsAddSubjectDialogOpen] = useState(false);
  const [groupToAddSubject, setGroupToAddSubject] =
    useState<SubjectEquivalenceGroup | null>(null);

  const { data: groups } = useQuery(
    getAllSubjectEquivalenceGroupsOptions(true),
  );

  const { mutateAsync: addSubjectsToGroupAsync } = useMutation(
    addSubjectsToEquivalenceGroupOptions(groupToAddSubject?.id ?? 0),
  );

  // Filter groups by search
  const filteredGroups = useMemo(() => {
    if (!searchQuery) return groups;
    const query = searchQuery.toLowerCase();
    return groups?.filter(
      (group) =>
        group.name.toLowerCase().includes(query) ||
        group.subjects?.some(
          (s) =>
            s.code.toLowerCase().includes(query) ||
            s.title.toLowerCase().includes(query),
        ),
    );
  }, [groups, searchQuery]);

  const handleAddSubject = (group: SubjectEquivalenceGroup) => {
    setGroupToAddSubject(group);
    setIsAddSubjectDialogOpen(true);
  };

  const handleAddSubjectsToGroup = async (subjectCodes: string[]) => {
    await addSubjectsToGroupAsync({ subjectCodes });
  };

  return (
    <div className="space-y-6">
      {/* Search */}
      <div className="relative flex-1 max-w-md">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
        <Input
          placeholder="Search groups or subjects..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="pl-9"
        />
      </div>

      {/* Groups Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        {filteredGroups?.map((group) => (
          <EquivalenceGroupCard
            key={group.id}
            group={group}
            onEdit={onEdit}
            onDelete={onDelete}
            onAddSubject={handleAddSubject}
          />
        ))}
      </div>

      {/* Empty State */}
      {filteredGroups?.length === 0 && (
        <div className="text-center py-12 border rounded-lg">
          <Package className="size-12 mx-auto mb-4 text-muted-foreground opacity-50" />
          <h3 className="text-lg font-medium mb-2">
            No equivalence groups found
          </h3>
          <p className="text-muted-foreground mb-4">
            {searchQuery
              ? "Try a different search term"
              : "Create your first equivalence group to link related subjects"}
          </p>
          {!searchQuery && (
            <Button onClick={onCreateNew}>
              <Plus className="size-4 mr-2" />
              Create First Group
            </Button>
          )}
        </div>
      )}

      <AddSubjectToGroupDialog
        isOpen={isAddSubjectDialogOpen}
        onOpenChange={setIsAddSubjectDialogOpen}
        group={groupToAddSubject}
        onSubmit={handleAddSubjectsToGroup}
      />
    </div>
  );
}
