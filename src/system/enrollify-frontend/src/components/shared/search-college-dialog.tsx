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
  X,
} from "lucide-react";
import { ModuleIcons } from "@/config/module-icons";
import { useState, useMemo, useEffect } from "react";
import { useSuspenseQuery } from "@tanstack/react-query";
import { getAllCollegesOptions } from "@/api/collections/college-collection";
import { useDebounce } from "@/hooks/use-debounce";
import type { College } from "@/api/models/college";

interface SearchCollegesDialogProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  title?: string;
  description?: React.ReactNode;
  excludeCollegeIds?: number[];
  maxSelections?: number;
  onSubmit: (colleges: College[]) => Promise<void>;
}

export function SearchCollegesDialog({
  isOpen,
  onOpenChange,
  title = "Add Colleges",
  description,
  excludeCollegeIds = [],
  maxSelections,
  onSubmit,
}: SearchCollegesDialogProps) {
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedColleges, setSelectedColleges] = useState<College[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [page, setPage] = useState(1);

  const pageSize = 10;
  const selectedCollegeIds = selectedColleges.map((c) => c.id);
  const debouncedSearchTerm = useDebounce(searchTerm, 300);

  useEffect(() => {
    if (isOpen) {
      setSearchTerm("");
      setSelectedColleges([]);
      setPage(1);
    }
  }, [isOpen]);

  useEffect(() => {
    setPage(1);
  }, [debouncedSearchTerm]);

  const excludeSet = useMemo(
    () => new Set(excludeCollegeIds),
    [excludeCollegeIds]
  );

  const { data: colleges = [], isLoading } = useSuspenseQuery({
    ...getAllCollegesOptions(),
  });

  const filteredColleges = useMemo(() => {
    const normalizedSearch = debouncedSearchTerm.trim().toLowerCase();
    return colleges.filter((college) => {
      if (excludeSet.has(college.id)) return false;
      if (!normalizedSearch) return true;

      const name = college.name.toLowerCase();
      const code = college.code.toLowerCase();
      const dean = college.dean.toLowerCase();
      const description = college.description.toLowerCase();

      return (
        name.includes(normalizedSearch) ||
        code.includes(normalizedSearch) ||
        dean.includes(normalizedSearch) ||
        description.includes(normalizedSearch)
      );
    });
  }, [colleges, excludeSet, debouncedSearchTerm]);

  const totalPages = Math.max(
    1,
    Math.ceil(filteredColleges.length / pageSize)
  );
  const hasNextPage = page < totalPages;
  const hasPreviousPage = page > 1;

  useEffect(() => {
    if (page > totalPages) {
      setPage(totalPages);
    }
  }, [page, totalPages]);

  const paginatedColleges = useMemo(() => {
    const startIndex = (page - 1) * pageSize;
    return filteredColleges.slice(startIndex, startIndex + pageSize);
  }, [filteredColleges, page, pageSize]);

  const isAtLimit =
    maxSelections !== undefined &&
    selectedColleges.length >= maxSelections;

  const handleToggleCollege = (college: College) => {
    const isSelected = selectedColleges.some((c) => c.id === college.id);
    if (isSelected) {
      setSelectedColleges((prev) => prev.filter((c) => c.id !== college.id));
      return;
    }

    if (
      maxSelections !== undefined &&
      selectedColleges.length >= maxSelections
    ) {
      return;
    }

    setSelectedColleges((prev) => [...prev, college]);
  };

  const handleSubmit = async () => {
    if (selectedCollegeIds.length === 0) return;
    setIsSubmitting(true);
    try {
      await onSubmit(selectedColleges);
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
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
            <Input
              placeholder="Search by college name, code, or dean..."
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

          <ScrollArea className="h-[300px] border rounded-md">
            <div className="p-2 space-y-1">
              {isLoading ? (
                <div className="text-center py-8 text-muted-foreground">
                  <Loader2 className="size-8 mx-auto mb-2 animate-spin" />
                  <p className="text-sm">Loading colleges...</p>
                </div>
              ) : paginatedColleges.length === 0 ? (
                <div className="text-center py-8 text-muted-foreground">
                  <ModuleIcons.colleges className="size-8 mx-auto mb-2 opacity-50" />
                  <p className="text-sm">
                    {searchTerm
                      ? "No colleges match your search"
                      : "No available colleges to add"}
                  </p>
                </div>
              ) : (
                paginatedColleges.map((college) => (
                  <label
                    key={college.id}
                    className="flex items-center gap-3 p-3 rounded-md hover:bg-muted cursor-pointer transition-colors"
                  >
                    <Checkbox
                      checked={selectedCollegeIds.includes(college.id)}
                      onCheckedChange={() => handleToggleCollege(college)}
                      disabled={
                        isAtLimit &&
                        !selectedCollegeIds.includes(college.id)
                      }
                    />
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2">
                        <span className="font-semibold text-accent">
                          {college.code}
                        </span>
                        <span className="text-muted-foreground">-</span>
                        <span className="truncate">{college.name}</span>
                      </div>
                      <p className="text-xs text-muted-foreground">
                        {college.dean}
                        {college.description
                          ? ` - ${college.description}`
                          : ""}
                      </p>
                    </div>
                  </label>
                ))
              )}
            </div>
          </ScrollArea>

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

          {(selectedCollegeIds.length > 0 ||
            maxSelections !== undefined) && (
            <p className="text-sm text-muted-foreground">
              {selectedCollegeIds.length}
              {maxSelections !== undefined
                ? ` / ${maxSelections}`
                : ""}{" "}
              college
              {selectedCollegeIds.length !== 1 ? "s" : ""} selected
              {isAtLimit && " - limit reached"}
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
            disabled={isSubmitting || selectedCollegeIds.length === 0}
            size="sm"
          >
            {isSubmitting && (
              <Loader2 className="mr-2 size-4 animate-spin" />
            )}
            Add{" "}
            {selectedCollegeIds.length > 0
              ? selectedCollegeIds.length
              : ""}{" "}
            College
            {selectedCollegeIds.length > 1 ? "s" : ""}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
