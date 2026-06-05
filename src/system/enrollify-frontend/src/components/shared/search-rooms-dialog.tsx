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
              placeholder="Search by room number, building, or type..."
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
                  <p className="text-sm">Loading rooms...</p>
                </div>
              ) : paginatedRooms.length === 0 ? (
                <div className="text-center py-8 text-muted-foreground">
                  <Building2 className="size-8 mx-auto mb-2 opacity-50" />
                  <p className="text-sm">
                    {searchTerm
                      ? "No rooms match your search"
                      : "No available rooms to add"}
                  </p>
                </div>
              ) : (
                paginatedRooms.map((room) => (
                  <label
                    key={room.id}
                    className="flex items-center gap-3 p-3 rounded-md hover:bg-muted cursor-pointer transition-colors"
                  >
                    <Checkbox
                      checked={selectedRoomIds.includes(room.id)}
                      onCheckedChange={() => handleToggleRoom(room)}
                      disabled={isAtLimit && !selectedRoomIds.includes(room.id)}
                    />
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2">
                        <span className="font-semibold text-accent">
                          {room.roomNumber}
                        </span>
                        <span className="text-muted-foreground">-</span>
                        <span className="truncate">{room.building.name}</span>
                      </div>
                      <p className="text-xs text-muted-foreground">
                        {room.roomType.name} - Capacity: {room.capacity}
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

          {(selectedRoomIds.length > 0 || maxSelections !== undefined) && (
            <p className="text-sm text-muted-foreground">
              {selectedRoomIds.length}
              {maxSelections !== undefined ? ` / ${maxSelections}` : ""} room
              {selectedRoomIds.length !== 1 ? "s" : ""} selected
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