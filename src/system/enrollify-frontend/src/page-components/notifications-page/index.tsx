import { useQueryStates } from "nuqs";
import { useEffect, useState } from "react";
import { useMutation, useSuspenseQuery } from "@tanstack/react-query";
import {
  Bell,
  CheckCheck,
  ChevronLeft,
  ChevronRight,
  Trash2,
} from "lucide-react";
import {
  dismissAllNotificationsOptions,
  filterNotificationsOptions,
  markAllNotificationsAsReadOptions,
} from "@/api/collections/notifications-collection";
import type {
  NotificationCategory,
  NotificationReadState,
  NotificationSeverity,
} from "@/api/models/notification";
import { ManagementPageLayout } from "@/components/page-layouts/management-page-layout";
import { NotificationCard } from "@/components/notifications/notification-card";
import { Button } from "@/components/ui/button";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { useDebounce } from "@/hooks/use-debounce";
import { searchParams } from "./searchParams";
import { NotificationsFilterBar } from "./notifications-filter-bar";

export default function NotificationsPage() {
  const [filters, setFilters] = useQueryStates(searchParams);
  const [searchInput, setSearchInput] = useState(filters.search);
  const debouncedSearch = useDebounce(searchInput, 300);
  const [isClearAllDialogOpen, setIsClearAllDialogOpen] = useState(false);

  useEffect(() => {
    setFilters((prev) => ({ ...prev, search: debouncedSearch, page: 1 }));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedSearch]);

  const { data: pagedNotifications } = useSuspenseQuery(
    filterNotificationsOptions({
      page: filters.page,
      pageSize: filters.pageSize,
      category: filters.category as NotificationCategory | "all",
      severity: filters.severity as NotificationSeverity | "all",
      readState: filters.readState as NotificationReadState,
      search: filters.search,
    }),
  );

  const { mutate: markAllAsRead, isPending: isMarkingAllAsRead } = useMutation(
    markAllNotificationsAsReadOptions(),
  );
  const { mutate: dismissAll, isPending: isDismissingAll } = useMutation({
    ...dismissAllNotificationsOptions(),
    onSuccess: () => setIsClearAllDialogOpen(false),
  });

  const notifications = pagedNotifications.items;
  const hasUnread = notifications.some((n) => !n.isRead);

  const handleClearFilters = () => {
    setSearchInput("");
    setFilters({
      category: "all",
      severity: "all",
      readState: "all",
      search: "",
      page: 1,
    });
  };

  return (
    <ManagementPageLayout
      title="Notifications"
      description="All your alerts, announcements, and background job updates"
      icon={<Bell className="size-6 text-primary" />}
      createNewItemButton={
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            className="gap-1.5"
            disabled={!hasUnread || isMarkingAllAsRead}
            onClick={() => markAllAsRead({})}
          >
            <CheckCheck className="size-4" />
            Mark all as read
          </Button>
          <Button
            variant="outline"
            size="sm"
            className="gap-1.5 text-destructive hover:text-destructive"
            disabled={pagedNotifications.totalCount === 0 || isDismissingAll}
            onClick={() => setIsClearAllDialogOpen(true)}
          >
            <Trash2 className="size-4" />
            Clear all
          </Button>
        </div>
      }
    >
      <div className="flex min-h-0 flex-1 flex-col gap-4">
        <NotificationsFilterBar
          search={searchInput}
          onSearchChange={setSearchInput}
          category={filters.category}
          onCategoryChange={(value) =>
            setFilters((prev) => ({ ...prev, category: value, page: 1 }))
          }
          severity={filters.severity}
          onSeverityChange={(value) =>
            setFilters((prev) => ({ ...prev, severity: value, page: 1 }))
          }
          readState={filters.readState}
          onReadStateChange={(value) =>
            setFilters((prev) => ({ ...prev, readState: value, page: 1 }))
          }
          onClear={handleClearFilters}
        />

        <div className="min-h-0 flex-1 overflow-y-auto rounded-lg border">
          {notifications.length === 0 ? (
            <div className="flex flex-col items-center justify-center gap-2 py-20 text-center text-muted-foreground">
              <Bell className="size-10 opacity-30" />
              <p className="text-sm font-medium">No notifications found</p>
              <p className="text-xs">Try adjusting your filters</p>
            </div>
          ) : (
            <div className="flex flex-col">
              {notifications.map((notification) => (
                <NotificationCard
                  key={notification.id}
                  notification={notification}
                />
              ))}
            </div>
          )}
        </div>

        {pagedNotifications.totalPages > 1 && (
          <div className="flex items-center justify-between text-sm text-muted-foreground">
            <span>
              Page {pagedNotifications.page} of {pagedNotifications.totalPages}{" "}
              &middot; {pagedNotifications.totalCount} notifications
            </span>
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="sm"
                disabled={filters.page <= 1}
                onClick={() =>
                  setFilters((prev) => ({ ...prev, page: prev.page - 1 }))
                }
              >
                <ChevronLeft className="size-4" />
                Previous
              </Button>
              <Button
                variant="outline"
                size="sm"
                disabled={filters.page >= pagedNotifications.totalPages}
                onClick={() =>
                  setFilters((prev) => ({ ...prev, page: prev.page + 1 }))
                }
              >
                Next
                <ChevronRight className="size-4" />
              </Button>
            </div>
          </div>
        )}
      </div>

      <AlertDialog
        open={isClearAllDialogOpen}
        onOpenChange={setIsClearAllDialogOpen}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Clear all notifications?</AlertDialogTitle>
            <AlertDialogDescription>
              This will dismiss all of your current notifications. You
              won&apos;t be able to undo this action.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={isDismissingAll}>
              Cancel
            </AlertDialogCancel>
            <AlertDialogAction
              disabled={isDismissingAll}
              onClick={(e) => {
                e.preventDefault();
                dismissAll({});
              }}
            >
              Clear all
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </ManagementPageLayout>
  );
}
