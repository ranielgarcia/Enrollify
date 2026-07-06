import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";
import { ChevronLeft, ChevronRight, Loader2, Search, X } from "lucide-react";
import { ModuleIcons } from "@/config/module-icons";
import { useState, useMemo, useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
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

function CollegeSkeleton() {
  return (
    <div className="flex items-center gap-3 p-3">
      <Skeleton className="size-4 shrink-0 rounded" />
      <Skeleton className="h-5 w-16 shrink-0 rounded-full" />
      <div className="flex-1 space-y-1.5">
        <Skeleton className="h-4 w-3/5" />
        <Skeleton className="h-3 w-2/5" />
      </div>
    </div>
  );
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
    [excludeCollegeIds],
  );

  const { data: colleges = [], isLoading } = useQuery({
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

  const totalCount = filteredColleges.length;
  const totalPages = Math.max(1, Math.ceil(filteredColleges.length / pageSize));
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
    maxSelections !== undefined && selectedColleges.length >= maxSelections;

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

  const pageNumbers = useMemo<(number | "ellipsis")[]>(() => {
    const pages: (number | "ellipsis")[] = [];
    if (totalPages <= 7) {
      for (let i = 1; i <= totalPages; i++) pages.push(i);
    } else if (page <= 3) {
      pages.push(1, 2, 3, 4, "ellipsis", totalPages);
    } else if (page >= totalPages - 2) {
      pages.push(
        1,
        "ellipsis",
        totalPages - 3,
        totalPages - 2,
        totalPages - 1,
        totalPages,
      );
    } else {
      pages.push(
        1,
        "ellipsis",
        page - 1,
        page,
        page + 1,
        "ellipsis",
        totalPages,
      );
    }
    return pages;
  }, [page, totalPages]);

  const itemsStart = totalCount > 0 ? (page - 1) * pageSize + 1 : 0;
  const itemsEnd = Math.min(page * pageSize, totalCount);

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[560px]">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          {description && (
            <DialogDescription asChild={typeof description !== "string"}>
              <span>{description}</span>
            </DialogDescription>
          )}
        </DialogHeader>

        <div className="space-y-4 overflow-hidden">
          <div className="relative w-full">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
            <Input
              placeholder="Search by college name, code, or dean..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-9 pr-9"
              autoFocus
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
            {isLoading ? (
              <div className="p-2 space-y-1">
                {Array.from({ length: 5 }).map((_, i) => (
                  <CollegeSkeleton key={i} />
                ))}
              </div>
            ) : paginatedColleges.length === 0 ? (
              <div className="text-center py-10 text-muted-foreground">
                <ModuleIcons.colleges className="size-10 mx-auto mb-3 opacity-30" />
                {searchTerm ? (
                  <>
                    <p className="text-sm font-medium">
                      No colleges matching{" "}
                      <strong className="text-foreground">
                        &ldquo;{searchTerm}&rdquo;
                      </strong>
                    </p>
                    <p className="text-xs mt-1">Try a different search term</p>
                  </>
                ) : (
                  <>
                    <p className="text-sm font-medium">
                      No available colleges to add
                    </p>
                    <p className="text-xs mt-1">
                      All colleges have already been added
                    </p>
                  </>
                )}
              </div>
            ) : (
              <div className="p-1 space-y-0.5">
                {paginatedColleges.map((college) => {
                  const isSelected = selectedCollegeIds.includes(college.id);
                  return (
                    <label
                      key={college.id}
                      className={cn(
                        "flex items-center gap-3 p-3 rounded-md cursor-pointer transition-all",
                        "border-l-2 border-transparent hover:border-l-primary hover:bg-muted/50",
                        isSelected && "bg-muted/30 border-l-primary",
                      )}
                    >
                      <Checkbox
                        checked={isSelected}
                        onCheckedChange={() => handleToggleCollege(college)}
                        disabled={isAtLimit && !isSelected}
                      />
                      <Badge
                        variant="secondary"
                        className="shrink-0 font-mono text-xs"
                      >
                        {college.code}
                      </Badge>
                      <div className="flex-1 min-w-0">
                        <span className="block truncate text-sm">
                          {college.name}
                        </span>
                        <span className="block truncate text-xs text-muted-foreground">
                          {college.dean}
                          {college.description
                            ? ` - ${college.description}`
                            : ""}
                        </span>
                      </div>
                    </label>
                  );
                })}
              </div>
            )}
          </ScrollArea>

          {/* Selection Summary */}
          {selectedCollegeIds.length > 0 && (
            <div
              className={cn(
                "flex items-center justify-between rounded-md border px-3 py-2 text-sm",
                isAtLimit
                  ? "bg-amber-50 border-amber-200 text-amber-800"
                  : "bg-primary/5 border-primary/10",
              )}
            >
              <span className="font-medium">
                {selectedCollegeIds.length}
                {maxSelections !== undefined ? ` / ${maxSelections}` : ""}{" "}
                college
                {selectedCollegeIds.length !== 1 ? "s" : ""} selected
                {isAtLimit && " — limit reached"}
              </span>
              <button
                type="button"
                onClick={() => setSelectedColleges([])}
                className="text-muted-foreground hover:text-foreground transition-colors p-0.5"
                aria-label="Clear selection"
              >
                <X className="size-4" />
              </button>
            </div>
          )}

          {/* Pagination Controls */}
          {totalPages > 1 && (
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2">
              <p className="text-xs text-muted-foreground order-2 sm:order-1">
                Showing {itemsStart}–{itemsEnd} of {totalCount} result
                {totalCount !== 1 ? "s" : ""}
              </p>
              <div className="flex flex-wrap items-center justify-start gap-1 order-1 sm:order-2">
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={!hasPreviousPage || isLoading}
                  className="size-8 p-0"
                  aria-label="Previous page"
                >
                  <ChevronLeft className="size-4" />
                </Button>
                {pageNumbers.map((p, i) =>
                  p === "ellipsis" ? (
                    <span
                      key={`ellipsis-${i}`}
                      className="px-1 text-muted-foreground text-xs select-none"
                    >
                      ...
                    </span>
                  ) : (
                    <Button
                      key={p}
                      type="button"
                      variant={p === page ? "default" : "outline"}
                      size="sm"
                      onClick={() => setPage(p)}
                      disabled={isLoading}
                      className="size-8 p-0 text-xs"
                      aria-label={`Page ${p}`}
                      aria-current={p === page ? "page" : undefined}
                    >
                      {p}
                    </Button>
                  ),
                )}
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => setPage((p) => p + 1)}
                  disabled={!hasNextPage || isLoading}
                  className="size-8 p-0"
                  aria-label="Next page"
                >
                  <ChevronRight className="size-4" />
                </Button>
              </div>
            </div>
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
            {isSubmitting && <Loader2 className="mr-2 size-4 animate-spin" />}
            Add {selectedCollegeIds.length > 0
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
