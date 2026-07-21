import { Search, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  NotificationCategory,
  NotificationReadState,
  NotificationSeverity,
} from "@/api/models/notification";

interface NotificationsFilterBarProps {
  search: string;
  onSearchChange: (value: string) => void;
  category: string;
  onCategoryChange: (value: string) => void;
  severity: string;
  onSeverityChange: (value: string) => void;
  readState: string;
  onReadStateChange: (value: string) => void;
  onClear: () => void;
}

const hasActiveFilters = (
  category: string,
  severity: string,
  readState: string,
  search: string,
) =>
  category !== "all" || severity !== "all" || readState !== "all" || !!search;

export function NotificationsFilterBar({
  search,
  onSearchChange,
  category,
  onCategoryChange,
  severity,
  onSeverityChange,
  readState,
  onReadStateChange,
  onClear,
}: NotificationsFilterBarProps) {
  return (
    <div className="flex flex-wrap items-center gap-2">
      <div className="relative min-w-[220px] flex-1 max-w-sm">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
        <Input
          placeholder="Search notifications..."
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
          className="pl-9"
        />
      </div>

      <Select value={readState} onValueChange={onReadStateChange}>
        <SelectTrigger size="sm" className="w-[140px]">
          <SelectValue placeholder="Read state" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value={NotificationReadState.All}>All</SelectItem>
          <SelectItem value={NotificationReadState.Unread}>Unread</SelectItem>
          <SelectItem value={NotificationReadState.Read}>Read</SelectItem>
        </SelectContent>
      </Select>

      <Select value={category} onValueChange={onCategoryChange}>
        <SelectTrigger size="sm" className="w-40">
          <SelectValue placeholder="Category" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">All categories</SelectItem>
          {Object.values(NotificationCategory).map((value) => (
            <SelectItem key={value} value={value}>
              {value}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Select value={severity} onValueChange={onSeverityChange}>
        <SelectTrigger size="sm" className="w-40">
          <SelectValue placeholder="Severity" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">All severities</SelectItem>
          {Object.values(NotificationSeverity).map((value) => (
            <SelectItem key={value} value={value}>
              {value}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      {hasActiveFilters(category, severity, readState, search) && (
        <Button
          variant="ghost"
          size="sm"
          className="gap-1.5 text-xs"
          onClick={onClear}
        >
          <X className="size-3.5" />
          Clear filters
        </Button>
      )}
    </div>
  );
}
