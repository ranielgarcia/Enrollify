import type { SubjectEquivalenceGroup } from "@/api/models/subject-equivalence";
import type { Subject } from "@/api/models/subject";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
import { ScrollArea } from "@/components/ui/scroll-area";
import { BookOpen, Loader2, Search } from "lucide-react";
import { useState, useMemo, useEffect } from "react";

interface AddSubjectToGroupDialogProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  group?: SubjectEquivalenceGroup | null;
  availableSubjects: Subject[];
  onSubmit: (groupId: number, subjectIds: number[]) => Promise<void>;
}

export function AddSubjectToGroupDialog({
  isOpen,
  onOpenChange,
  group,
  availableSubjects,
  onSubmit,
}: AddSubjectToGroupDialogProps) {
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedSubjectIds, setSelectedSubjectIds] = useState<number[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Reset state when dialog opens/closes
  useEffect(() => {
    if (isOpen) {
      setSearchQuery("");
      setSelectedSubjectIds([]);
    }
  }, [isOpen]);

  // Get subjects already in this group
  const existingSubjectIds = useMemo(() => {
    return new Set(group?.subjects?.map((s) => s.id) ?? []);
  }, [group]);

  // Filter subjects: not already in group and matching search
  const filteredSubjects = useMemo(() => {
    return availableSubjects.filter((subject) => {
      // Exclude subjects already in the group
      if (existingSubjectIds.has(subject.id)) return false;

      // Filter by search query
      if (searchQuery) {
        const query = searchQuery.toLowerCase();
        return (
          subject.code.toLowerCase().includes(query) ||
          subject.title.toLowerCase().includes(query)
        );
      }
      return true;
    });
  }, [availableSubjects, existingSubjectIds, searchQuery]);

  const handleToggleSubject = (subjectId: number) => {
    setSelectedSubjectIds((prev) =>
      prev.includes(subjectId)
        ? prev.filter((id) => id !== subjectId)
        : [...prev, subjectId],
    );
  };

  const handleSubmit = async () => {
    if (!group || selectedSubjectIds.length === 0) return;

    setIsSubmitting(true);
    try {
      await onSubmit(group.id, selectedSubjectIds);
      onOpenChange(false);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Add Subjects to Group</DialogTitle>
          <DialogDescription>
            Add subjects to <strong>"{group?.name}"</strong> equivalence group.
            <br />
            Select the subjects that are considered equivalent.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          {/* Search */}
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
            <Input
              placeholder="Search by code or title..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-9"
            />
          </div>

          {/* Subject List */}
          <ScrollArea className="h-[300px] border rounded-md">
            <div className="p-2 space-y-1">
              {filteredSubjects.length === 0 ? (
                <div className="text-center py-8 text-muted-foreground">
                  <BookOpen className="size-8 mx-auto mb-2 opacity-50" />
                  <p className="text-sm">
                    {searchQuery
                      ? "No subjects match your search"
                      : "No available subjects to add"}
                  </p>
                </div>
              ) : (
                filteredSubjects.map((subject) => (
                  <label
                    key={subject.id}
                    className="flex items-center gap-3 p-3 rounded-md hover:bg-muted cursor-pointer transition-colors"
                  >
                    <Checkbox
                      checked={selectedSubjectIds.includes(subject.id)}
                      onCheckedChange={() => handleToggleSubject(subject.id)}
                    />
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2">
                        <span className="font-semibold text-accent">
                          {subject.code}
                        </span>
                        <span className="text-muted-foreground">-</span>
                        <span className="truncate">{subject.title}</span>
                      </div>
                      <p className="text-xs text-muted-foreground">
                        {Number(subject.units).toFixed(1)} units
                      </p>
                    </div>
                  </label>
                ))
              )}
            </div>
          </ScrollArea>

          {selectedSubjectIds.length > 0 && (
            <p className="text-sm text-muted-foreground">
              {selectedSubjectIds.length} subject
              {selectedSubjectIds.length > 1 ? "s" : ""} selected
            </p>
          )}
        </div>

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={isSubmitting}
          >
            Cancel
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={isSubmitting || selectedSubjectIds.length === 0}
          >
            {isSubmitting && <Loader2 className="mr-2 size-4 animate-spin" />}
            Add {selectedSubjectIds.length > 0
              ? selectedSubjectIds.length
              : ""}{" "}
            Subject
            {selectedSubjectIds.length > 1 ? "s" : ""}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
