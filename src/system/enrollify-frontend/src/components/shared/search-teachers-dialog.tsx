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
import {
  ChevronLeft,
  ChevronRight,
  Loader2,
  Search,
  UserRound,
  X,
} from "lucide-react";
import { useState, useMemo, useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { searchTeachersPaginatedOptions } from "@/api/collections/teacher-collection";
import { useDebounce } from "@/hooks/use-debounce";
import type { Teacher } from "@/api/models/teacher";

interface SearchTeachersDialogProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  /** Dialog title */
  title?: string;
  /** Dialog description / subtitle */
  description?: React.ReactNode;
  /** Teacher IDs already held by the calling entity — these will be excluded from the list */
  excludeTeacherIds?: number[];
  /** Maximum number of teachers that can be selected. Omit for unlimited. */
  maxSelections?: number;
  onSubmit: (teachers: Teacher[]) => Promise<void>;
}

export function SearchTeachersDialog({
  isOpen,
  onOpenChange,
  title = "Add Teachers",
  description,
  excludeTeacherIds = [],
  maxSelections,
  onSubmit,
}: SearchTeachersDialogProps) {
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedTeachers, setSelectedTeachers] = useState<Teacher[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [page, setPage] = useState(1);

  const selectedTeacherIds = selectedTeachers.map((t) => t.id);

  const debouncedSearchTerm = useDebounce(searchTerm, 300);

  // Reset state when dialog opens/closes
  useEffect(() => {
    if (isOpen) {
      setSearchTerm("");
      setSelectedTeachers([]);
      setPage(1);
    }
  }, [isOpen]);

  // Reset page to 1 when search term changes
  useEffect(() => {
    setPage(1);
  }, [debouncedSearchTerm]);

  const excludeSet = useMemo(
    () => new Set(excludeTeacherIds),
    [excludeTeacherIds],
  );

  const { data: paginatedTeachers, isLoading } = useQuery(
    searchTeachersPaginatedOptions(page, 10, debouncedSearchTerm, isOpen),
  );

  const totalPages = paginatedTeachers?.totalPages ?? 1;
  const hasNextPage = page < totalPages;
  const hasPreviousPage = page > 1;

  const filteredTeachers = useMemo(() => {
    return paginatedTeachers?.items?.filter(
      (teacher) => !excludeSet.has(teacher.id),
    );
  }, [paginatedTeachers, excludeSet]);

  const isAtLimit =
    maxSelections !== undefined && selectedTeachers.length >= maxSelections;

  const handleToggleTeacher = (teacher: Teacher) => {
    const isSelected = selectedTeachers.some((t) => t.id === teacher.id);
    if (isSelected) {
      setSelectedTeachers((prev) => prev.filter((t) => t.id !== teacher.id));
    } else {
      if (
        maxSelections !== undefined &&
        selectedTeachers.length >= maxSelections
      )
        return;
      setSelectedTeachers((prev) => [...prev, teacher]);
    }
  };

  const handleSubmit = async () => {
    if (selectedTeacherIds.length === 0) return;
    setIsSubmitting(true);
    try {
      await onSubmit(selectedTeachers);
      onOpenChange(false);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          {description && (
            <DialogDescription asChild={typeof description !== "string"}>
              <span>{description}</span>
            </DialogDescription>
          )}
        </DialogHeader>

        <div className="space-y-4">
          {/* Search */}
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
            <Input
              placeholder="Search by name or ID..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-9 pr-9"
            />
            {searchTerm && (
              <button
                type="button"
                onClick={() => setSearchTerm("")}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors"
                aria-label="Clear search"
              >
                <X className="size-4" />
              </button>
            )}
          </div>

          {/* Teacher List */}
          <ScrollArea className="h-[300px] border rounded-md">
            <div className="p-2 space-y-1">
              {isLoading ? (
                <div className="text-center py-8 text-muted-foreground">
                  <Loader2 className="size-8 mx-auto mb-2 animate-spin" />
                  <p className="text-sm">Loading teachers...</p>
                </div>
              ) : filteredTeachers?.length === 0 ? (
                <div className="text-center py-8 text-muted-foreground">
                  <UserRound className="size-8 mx-auto mb-2 opacity-50" />
                  <p className="text-sm">
                    {searchTerm
                      ? "No teachers match your search"
                      : "No available teachers to add"}
                  </p>
                </div>
              ) : (
                filteredTeachers?.map((teacher) => (
                  <label
                    key={teacher.id}
                    className="flex items-center gap-3 p-3 rounded-md hover:bg-muted cursor-pointer transition-colors"
                  >
                    <Checkbox
                      checked={selectedTeacherIds.includes(teacher.id)}
                      onCheckedChange={() => handleToggleTeacher(teacher)}
                      disabled={
                        isAtLimit && !selectedTeacherIds.includes(teacher.id)
                      }
                    />
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2">
                        <span className="font-semibold text-accent">
                          {teacher.teacherIdentifier}
                        </span>
                        <span className="text-muted-foreground">-</span>
                        <span className="truncate">
                          {teacher.firstName} {teacher.lastName}
                        </span>
                      </div>
                      {teacher.department?.name && (
                        <p className="text-xs text-muted-foreground">
                          {teacher.department.name}
                        </p>
                      )}
                    </div>
                  </label>
                ))
              )}
            </div>
          </ScrollArea>

          {/* Pagination Controls */}
          {totalPages > 1 && (
            <div className="flex items-center justify-between">
              <p className="text-sm text-muted-foreground">
                Page {page} of {totalPages}
              </p>
              <div className="flex items-center gap-2">
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={!hasPreviousPage || isLoading}
                >
                  <ChevronLeft className="size-4" />
                  Previous
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => setPage((p) => p + 1)}
                  disabled={!hasNextPage || isLoading}
                >
                  Next
                  <ChevronRight className="size-4" />
                </Button>
              </div>
            </div>
          )}

          {(selectedTeacherIds.length > 0 || maxSelections !== undefined) && (
            <p className="text-sm text-muted-foreground">
              {selectedTeacherIds.length}
              {maxSelections !== undefined ? ` / ${maxSelections}` : ""} teacher
              {selectedTeacherIds.length !== 1 ? "s" : ""} selected
              {isAtLimit && " — limit reached"}
            </p>
          )}
        </div>

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={isSubmitting}
            size="sm"
          >
            Cancel
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={isSubmitting || selectedTeacherIds.length === 0}
            size="sm"
          >
            {isSubmitting && <Loader2 className="mr-2 size-4 animate-spin" />}
            Add {selectedTeacherIds.length > 0
              ? selectedTeacherIds.length
              : ""}{" "}
            Teacher
            {selectedTeacherIds.length > 1 ? "s" : ""}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
