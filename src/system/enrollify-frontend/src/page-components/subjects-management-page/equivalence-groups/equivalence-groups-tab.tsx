import type { SubjectEquivalenceGroup } from "@/api/models/subject-equivalence";
import type { Subject } from "@/api/models/subject";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Package, Plus, Search } from "lucide-react";
import { useState, useMemo } from "react";
import { EquivalenceGroupCard } from "./equivalence-group-card";
import { EquivalenceGroupFormDialog } from "./equivalence-group-form-dialog";
import { DeleteEquivalenceGroupDialog } from "./delete-equivalence-group-dialog";
import { AddSubjectToGroupDialog } from "./add-subject-to-group-dialog";

// Mock data for UI demonstration
const mockEquivalenceGroups: SubjectEquivalenceGroup[] = [
  {
    id: 1,
    name: "Programming Fundamentals",
    subjects: [
      { id: 1, code: "CS101", title: "Introduction to Programming", units: 3 },
      { id: 2, code: "IT101", title: "Programming Basics", units: 3 },
      { id: 3, code: "CpE101", title: "Computer Programming 1", units: 3 },
    ],
    createdAt: new Date().toISOString(),
    createdBy: null,
    updatedAt: null,
    updatedBy: null,
    deletedAt: null,
    isActive: true,
  },
  {
    id: 2,
    name: "Calculus Equivalents",
    subjects: [
      { id: 4, code: "MATH101", title: "Calculus I", units: 4 },
      {
        id: 5,
        code: "ENGR-MATH1",
        title: "Engineering Mathematics 1",
        units: 4,
      },
    ],
    createdAt: new Date().toISOString(),
    createdBy: null,
    updatedAt: null,
    updatedBy: null,
    deletedAt: null,
    isActive: true,
  },
  {
    id: 3,
    name: "Physics Foundations",
    subjects: [
      { id: 6, code: "PHYS101", title: "General Physics I", units: 4 },
      { id: 7, code: "ENGR-PHYS1", title: "Engineering Physics 1", units: 4 },
    ],
    createdAt: new Date().toISOString(),
    createdBy: null,
    updatedAt: null,
    updatedBy: null,
    deletedAt: null,
    isActive: true,
  },
];

interface EquivalenceGroupsTabProps {
  allSubjects: Subject[];
}

export function EquivalenceGroupsTab({
  allSubjects,
}: EquivalenceGroupsTabProps) {
  const [searchQuery, setSearchQuery] = useState("");
  const [groups, setGroups] = useState<SubjectEquivalenceGroup[]>(
    mockEquivalenceGroups,
  );

  // Form dialog state
  const [isFormDialogOpen, setIsFormDialogOpen] = useState(false);
  const [groupToEdit, setGroupToEdit] =
    useState<SubjectEquivalenceGroup | null>(null);

  // Delete dialog state
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [groupToDelete, setGroupToDelete] =
    useState<SubjectEquivalenceGroup | null>(null);

  // Add subject dialog state
  const [isAddSubjectDialogOpen, setIsAddSubjectDialogOpen] = useState(false);
  const [groupToAddSubject, setGroupToAddSubject] =
    useState<SubjectEquivalenceGroup | null>(null);

  // Filter groups by search
  const filteredGroups = useMemo(() => {
    if (!searchQuery) return groups;
    const query = searchQuery.toLowerCase();
    return groups.filter(
      (group) =>
        group.name.toLowerCase().includes(query) ||
        group.subjects?.some(
          (s) =>
            s.code.toLowerCase().includes(query) ||
            s.title.toLowerCase().includes(query),
        ),
    );
  }, [groups, searchQuery]);

  // Handlers
  const handleCreateNew = () => {
    setGroupToEdit(null);
    setIsFormDialogOpen(true);
  };

  const handleEdit = (group: SubjectEquivalenceGroup) => {
    setGroupToEdit(group);
    setIsFormDialogOpen(true);
  };

  const handleDelete = (group: SubjectEquivalenceGroup) => {
    setGroupToDelete(group);
    setIsDeleteDialogOpen(true);
  };

  const handleAddSubject = (group: SubjectEquivalenceGroup) => {
    setGroupToAddSubject(group);
    setIsAddSubjectDialogOpen(true);
  };

  const handleRemoveSubject = (
    group: SubjectEquivalenceGroup,
    subject: { id: number; code: string; title: string; units: number },
  ) => {
    // Mock: Remove subject from group locally
    setGroups((prev) =>
      prev.map((g) =>
        g.id === group.id
          ? { ...g, subjects: g.subjects?.filter((s) => s.id !== subject.id) }
          : g,
      ),
    );
    // TODO: Call API to remove subject from group
    console.log(`Remove subject ${subject.code} from group ${group.name}`);
  };

  const handleFormSubmit = async (data: { name: string }) => {
    if (groupToEdit) {
      // Mock: Update group locally
      setGroups((prev) =>
        prev.map((g) =>
          g.id === groupToEdit.id ? { ...g, name: data.name } : g,
        ),
      );
      // TODO: Call API to update group
      console.log("Update group:", groupToEdit.id, data);
    } else {
      // Mock: Create new group locally
      const newGroup: SubjectEquivalenceGroup = {
        id: Date.now(),
        name: data.name,
        subjects: [],
        createdAt: new Date().toISOString(),
        createdBy: null,
        updatedAt: null,
        updatedBy: null,
        deletedAt: null,
        isActive: true,
      };
      setGroups((prev) => [...prev, newGroup]);
      // TODO: Call API to create group
      console.log("Create group:", data);
    }
  };

  const handleConfirmDelete = async () => {
    if (!groupToDelete) return;
    // Mock: Delete group locally
    setGroups((prev) => prev.filter((g) => g.id !== groupToDelete.id));
    // TODO: Call API to delete group
    console.log("Delete group:", groupToDelete.id);
  };

  const handleAddSubjectsToGroup = async (
    groupId: number,
    subjectIds: number[],
  ) => {
    // Mock: Add subjects to group locally
    const subjectsToAdd = allSubjects
      .filter((s) => subjectIds.includes(s.id))
      .map((s) => ({
        id: s.id,
        code: s.code,
        title: s.title,
        units: s.units,
      }));

    setGroups((prev) =>
      prev.map((g) =>
        g.id === groupId
          ? { ...g, subjects: [...(g.subjects ?? []), ...subjectsToAdd] }
          : g,
      ),
    );
    // TODO: Call API to add subjects to group
    console.log("Add subjects to group:", groupId, subjectIds);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row gap-4 justify-between">
        <div className="relative flex-1 max-w-md">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
          <Input
            placeholder="Search groups or subjects..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-9"
          />
        </div>
        <Button onClick={handleCreateNew}>
          <Plus className="size-4 mr-2" />
          New Equivalence Group
        </Button>
      </div>

      {/* Groups Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        {filteredGroups.map((group) => (
          <EquivalenceGroupCard
            key={group.id}
            group={group}
            onEdit={handleEdit}
            onDelete={handleDelete}
            onAddSubject={handleAddSubject}
            onRemoveSubject={handleRemoveSubject}
          />
        ))}
      </div>

      {/* Empty State */}
      {filteredGroups.length === 0 && (
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
            <Button onClick={handleCreateNew}>
              <Plus className="size-4 mr-2" />
              Create First Group
            </Button>
          )}
        </div>
      )}

      {/* Dialogs */}
      <EquivalenceGroupFormDialog
        isOpen={isFormDialogOpen}
        onOpenChange={setIsFormDialogOpen}
        groupToEdit={groupToEdit}
        onSubmit={handleFormSubmit}
      />

      <DeleteEquivalenceGroupDialog
        isOpen={isDeleteDialogOpen}
        onOpenChange={setIsDeleteDialogOpen}
        group={groupToDelete}
        onConfirm={handleConfirmDelete}
      />

      <AddSubjectToGroupDialog
        isOpen={isAddSubjectDialogOpen}
        onOpenChange={setIsAddSubjectDialogOpen}
        group={groupToAddSubject}
        availableSubjects={allSubjects}
        onSubmit={handleAddSubjectsToGroup}
      />
    </div>
  );
}
