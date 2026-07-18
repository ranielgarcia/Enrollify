import { useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";
import { Bell, CheckCheck, Trash2 } from "lucide-react";
import {
  dismissAllNotificationsOptions,
  filterNotificationsOptions,
  markAllNotificationsAsReadOptions,
} from "@/api/collections/notifications-collection";
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
import { Button } from "@/components/ui/button";
import {
  Drawer,
  DrawerContent,
  DrawerDescription,
  DrawerFooter,
  DrawerHeader,
  DrawerTitle,
} from "@/components/ui/drawer";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Skeleton } from "@/components/ui/skeleton";
import { NotificationCard } from "./notification-card";

interface NotificationsDrawerProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

const RECENT_NOTIFICATIONS_LIMIT = 20;

export function NotificationsDrawer({
  isOpen,
  onOpenChange,
}: NotificationsDrawerProps) {
  const [isClearAllDialogOpen, setIsClearAllDialogOpen] = useState(false);

  const { data: pagedNotifications, isLoading } = useQuery({
    ...filterNotificationsOptions({
      page: 1,
      pageSize: RECENT_NOTIFICATIONS_LIMIT,
    }),
    enabled: isOpen,
  });

  const { mutate: markAllAsRead, isPending: isMarkingAllAsRead } = useMutation(
    markAllNotificationsAsReadOptions(),
  );
  const { mutate: dismissAll, isPending: isDismissingAll } = useMutation({
    ...dismissAllNotificationsOptions(),
    onSuccess: () => setIsClearAllDialogOpen(false),
  });

  const notifications = pagedNotifications?.items ?? [];
  const hasUnread = notifications.some((n) => !n.isRead);

  return (
    <>
      <Drawer direction="right" open={isOpen} onOpenChange={onOpenChange}>
        <DrawerContent className="data-[vaul-drawer-direction=right]:w-[420px] data-[vaul-drawer-direction=right]:sm:max-w-none">
          <DrawerHeader className="border-b pb-3">
            <DrawerTitle>Notifications</DrawerTitle>
            <DrawerDescription>
              Updates, alerts, and background job results
            </DrawerDescription>
            <div className="flex items-center gap-1 pt-1">
              <Button
                variant="ghost"
                size="sm"
                className="gap-1.5 text-xs"
                disabled={!hasUnread || isMarkingAllAsRead}
                onClick={() => markAllAsRead({})}
              >
                <CheckCheck className="size-3.5" />
                Mark all as read
              </Button>
              <Button
                variant="ghost"
                size="sm"
                className="gap-1.5 text-xs text-destructive hover:text-destructive"
                disabled={notifications.length === 0 || isDismissingAll}
                onClick={() => setIsClearAllDialogOpen(true)}
              >
                <Trash2 className="size-3.5" />
                Clear all
              </Button>
            </div>
          </DrawerHeader>

          <ScrollArea className="flex-1">
            {isLoading ? (
              <div className="flex flex-col gap-3 p-4">
                {Array.from({ length: 4 }).map((_, i) => (
                  <Skeleton key={i} className="h-20 w-full rounded-md" />
                ))}
              </div>
            ) : notifications.length === 0 ? (
              <div className="flex flex-col items-center justify-center gap-2 py-16 text-center text-muted-foreground">
                <Bell className="size-10 opacity-30" />
                <p className="text-sm font-medium">You&apos;re all caught up</p>
                <p className="text-xs">No new notifications right now.</p>
              </div>
            ) : (
              <div className="flex flex-col">
                {notifications.map((notification) => (
                  <NotificationCard
                    key={notification.id}
                    notification={notification}
                    onNavigate={() => onOpenChange(false)}
                  />
                ))}
              </div>
            )}
          </ScrollArea>

          <DrawerFooter className="border-t">
            <Button variant="outline" asChild>
              <Link
                to="/portal/notifications"
                onClick={() => onOpenChange(false)}
              >
                View all notifications
              </Link>
            </Button>
          </DrawerFooter>
        </DrawerContent>
      </Drawer>

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
    </>
  );
}
