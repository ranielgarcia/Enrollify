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
  BookOpen,
  ChevronLeft,
  ChevronRight,
  Loader2,
  Search,
  X,
} from "lucide-react";
import { useState, useMemo, useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { searchSubjectsPaginatedOptions } from "@/api/collections/subject-collection";
import { useDebounce } from "@/hooks/use-debounce";

interface AddSubjectsDialogProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  /** Dialog title */
  title?: string;
  /** Dialog description / subtitle */
  description?: React.ReactNode;
  /** Subject codes already held by the calling entity — these will be excluded from the list */
  excludeSubjectCodes?: string[];
  onSubmit: (subjectCodes: string[]) => Promise<void>;
}

export function SearchSubjectsDialog({
  isOpen,
  onOpenChange,
  title = "Add Subjects",
  description,
  excludeSubjectCodes = [],
  onSubmit,
}: AddSubjectsDialogProps) {
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedSubjectCodes, setSelectedSubjectCodes] = useState<string[]>(
    [],
  );
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [page, setPage] = useState(1);

  const debouncedSearchTerm = useDebounce(searchTerm, 300);

  // Reset state when dialog opens/closes
  useEffect(() => {
    if (isOpen) {
      setSearchTerm("");
      setSelectedSubjectCodes([]);
      setPage(1);
    }
  }, [isOpen]);

  // Reset page to 1 when search term changes
  useEffect(() => {
    setPage(1);
  }, [debouncedSearchTerm]);

  const excludeSet = useMemo(
    () => new Set(excludeSubjectCodes),
    [excludeSubjectCodes],
  );

  const { data: paginatedSubjects, isLoading } = useQuery(
    searchSubjectsPaginatedOptions(page, 10, debouncedSearchTerm, isOpen),
  );

  const totalPages = paginatedSubjects?.totalPages ?? 1;
  const hasNextPage = page < totalPages;
  const hasPreviousPage = page > 1;

  const filteredSubjects = useMemo(() => {
    return paginatedSubjects?.items?.filter(
      (subject) => !(subject.code && excludeSet.has(subject.code)),
    );
  }, [paginatedSubjects, excludeSet]);

  const handleToggleSubject = (subjectCode?: string) => {
    if (!subjectCode) return;
    setSelectedSubjectCodes((prev) =>
      prev.includes(subjectCode)
        ? prev.filter((code) => code !== subjectCode)
        : [...prev, subjectCode],
    );
  };

  const handleSubmit = async () => {
    if (selectedSubjectCodes.length === 0) return;
    setIsSubmitting(true);
    try {
      await onSubmit(selectedSubjectCodes);
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
              placeholder="Search by code or title..."
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

          {/* Subject List */}
          <ScrollArea className="h-[300px] border rounded-md">
            <div className="p-2 space-y-1">
              {isLoading ? (
                <div className="text-center py-8 text-muted-foreground">
                  <Loader2 className="size-8 mx-auto mb-2 animate-spin" />
                  <p className="text-sm">Loading subjects...</p>
                </div>
              ) : filteredSubjects?.length === 0 ? (
                <div className="text-center py-8 text-muted-foreground">
                  <BookOpen className="size-8 mx-auto mb-2 opacity-50" />
                  <p className="text-sm">
                    {searchTerm
                      ? "No subjects match your search"
                      : "No available subjects to add"}
                  </p>
                </div>
              ) : (
                filteredSubjects?.map((subject) => (
                  <label
                    key={subject.id}
                    className="flex items-center gap-3 p-3 rounded-md hover:bg-muted cursor-pointer transition-colors"
                  >
                    <Checkbox
                      checked={selectedSubjectCodes.includes(subject.code)}
                      onCheckedChange={() => handleToggleSubject(subject.code)}
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

          {selectedSubjectCodes.length > 0 && (
            <p className="text-sm text-muted-foreground">
              {selectedSubjectCodes.length} subject
              {selectedSubjectCodes.length > 1 ? "s" : ""} selected
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
            disabled={isSubmitting || selectedSubjectCodes.length === 0}
            size="sm"
          >
            {isSubmitting && <Loader2 className="mr-2 size-4 animate-spin" />}
            Add{" "}
            {selectedSubjectCodes.length > 0
              ? selectedSubjectCodes.length
              : ""}{" "}
            Subject
            {selectedSubjectCodes.length > 1 ? "s" : ""}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
