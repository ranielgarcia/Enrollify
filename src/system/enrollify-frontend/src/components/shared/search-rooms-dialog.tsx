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
import {
  Building2,
  ChevronLeft,
  ChevronRight,
  Loader2,
  Search,
  X,
} from "lucide-react";
import { useState, useMemo, useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { getAllRooms } from "@/api/collections/room-collection";
import { useDebounce } from "@/hooks/use-debounce";
import type { Room } from "@/api/models/room";

interface SearchRoomsDialogProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  /** Dialog title */
  title?: string;
  /** Dialog description / subtitle */
  description?: React.ReactNode;
  /** Room IDs already held by the calling entity - these will be excluded from the list */
  excludeRoomIds?: number[];
  /** Maximum number of rooms that can be selected. Omit for unlimited. */
  maxSelections?: number;
  onSubmit: (rooms: Room[]) => Promise<void>;
}

function RoomSkeleton() {
  return (
    <div className="flex items-center gap-3 p-3">
      <Skeleton className="size-4 shrink-0 rounded" />
      <Skeleton className="h-5 w-20 shrink-0 rounded-full" />
      <Skeleton className="h-4 flex-1" />
      <Skeleton className="h-5 w-20 shrink-0 rounded-full" />
    </div>
  );
}

export function SearchRoomsDialog({
  isOpen,
  onOpenChange,
  title = "Add Rooms",
  description,
  excludeRoomIds = [],
  maxSelections,
  onSubmit,
}: SearchRoomsDialogProps) {
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedRooms, setSelectedRooms] = useState<Room[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [page, setPage] = useState(1);

  const pageSize = 10;
  const selectedRoomIds = selectedRooms.map((r) => r.id);
  const debouncedSearchTerm = useDebounce(searchTerm, 300);

  useEffect(() => {
    if (isOpen) {
      setSearchTerm("");
      setSelectedRooms([]);
      setPage(1);
    }
  }, [isOpen]);

  useEffect(() => {
    setPage(1);
  }, [debouncedSearchTerm]);

  const excludeSet = useMemo(() => new Set(excludeRoomIds), [excludeRoomIds]);

  const { data: rooms = [], isLoading } = useQuery({
    ...getAllRooms(),
    enabled: isOpen,
  });

  const filteredRooms = useMemo(() => {
    const normalizedSearch = debouncedSearchTerm.trim().toLowerCase();
    return rooms.filter((room) => {
      if (excludeSet.has(room.id)) return false;
      if (!normalizedSearch) return true;

      const roomNumber = room.roomNumber.toLowerCase();
      const buildingName = room.building?.name?.toLowerCase() ?? "";
      const roomTypeName = room.roomType?.name?.toLowerCase() ?? "";

      return (
        roomNumber.includes(normalizedSearch) ||
        buildingName.includes(normalizedSearch) ||
        roomTypeName.includes(normalizedSearch)
      );
    });
  }, [rooms, excludeSet, debouncedSearchTerm]);

  const totalCount = filteredRooms.length;
  const totalPages = Math.max(1, Math.ceil(filteredRooms.length / pageSize));
  const hasNextPage = page < totalPages;
  const hasPreviousPage = page > 1;

  useEffect(() => {
    if (page > totalPages) {
      setPage(totalPages);
    }
  }, [page, totalPages]);

  const paginatedRooms = useMemo(() => {
    const startIndex = (page - 1) * pageSize;
    return filteredRooms.slice(startIndex, startIndex + pageSize);
  }, [filteredRooms, page, pageSize]);

  const isAtLimit =
    maxSelections !== undefined && selectedRooms.length >= maxSelections;

  const handleToggleRoom = (room: Room) => {
    const isSelected = selectedRooms.some((r) => r.id === room.id);
    if (isSelected) {
      setSelectedRooms((prev) => prev.filter((r) => r.id !== room.id));
      return;
    }

    if (maxSelections !== undefined && selectedRooms.length >= maxSelections) {
      return;
    }

    setSelectedRooms((prev) => [...prev, room]);
  };

  const handleSubmit = async () => {
    if (selectedRoomIds.length === 0) return;
    setIsSubmitting(true);
    try {
      await onSubmit(selectedRooms);
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
              placeholder="Search by room number, building, or type..."
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
                  <RoomSkeleton key={i} />
                ))}
              </div>
            ) : paginatedRooms.length === 0 ? (
              <div className="text-center py-10 text-muted-foreground">
                <Building2 className="size-10 mx-auto mb-3 opacity-30" />
                {searchTerm ? (
                  <>
                    <p className="text-sm font-medium">
                      No rooms matching{" "}
                      <strong className="text-foreground">
                        &ldquo;{searchTerm}&rdquo;
                      </strong>
                    </p>
                    <p className="text-xs mt-1">Try a different search term</p>
                  </>
                ) : (
                  <>
                    <p className="text-sm font-medium">
                      No available rooms to add
                    </p>
                    <p className="text-xs mt-1">
                      All rooms have already been added
                    </p>
                  </>
                )}
              </div>
            ) : (
              <div className="p-1 space-y-0.5">
                {paginatedRooms.map((room) => {
                  const isSelected = selectedRoomIds.includes(room.id);
                  return (
                    <label
                      key={room.id}
                      className={cn(
                        "flex items-center gap-3 p-3 rounded-md cursor-pointer transition-all",
                        "border-l-2 border-transparent hover:border-l-primary hover:bg-muted/50",
                        isSelected && "bg-muted/30 border-l-primary",
                      )}
                    >
                      <Checkbox
                        checked={isSelected}
                        onCheckedChange={() => handleToggleRoom(room)}
                        disabled={isAtLimit && !isSelected}
                      />
                      <Badge
                        variant="secondary"
                        className="shrink-0 font-mono text-xs"
                      >
                        {room.roomNumber}
                      </Badge>
                      <span className="flex-1 min-w-0 truncate text-sm">
                        {room.building.name}
                      </span>
                      <Badge
                        variant="outline"
                        className="shrink-0 text-xs tabular-nums"
                      >
                        {room.capacity} seats
                      </Badge>
                    </label>
                  );
                })}
              </div>
            )}
          </ScrollArea>

          {/* Selection Summary */}
          {selectedRoomIds.length > 0 && (
            <div
              className={cn(
                "flex items-center justify-between rounded-md border px-3 py-2 text-sm",
                isAtLimit
                  ? "bg-amber-50 border-amber-200 text-amber-800"
                  : "bg-primary/5 border-primary/10",
              )}
            >
              <span className="font-medium">
                {selectedRoomIds.length}
                {maxSelections !== undefined ? ` / ${maxSelections}` : ""} room
                {selectedRoomIds.length !== 1 ? "s" : ""} selected
                {isAtLimit && " — limit reached"}
              </span>
              <button
                type="button"
                onClick={() => setSelectedRooms([])}
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
            disabled={isSubmitting || selectedRoomIds.length === 0}
            size="sm"
          >
            {isSubmitting && <Loader2 className="mr-2 size-4 animate-spin" />}
            Add {selectedRoomIds.length > 0 ? selectedRoomIds.length : ""} Room
            {selectedRoomIds.length > 1 ? "s" : ""}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
