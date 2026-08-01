import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Bell } from "lucide-react";
import { getUnreadCountOptions } from "@/api/collections/notifications-collection";
import { Button } from "@/components/ui/button";
import { NotificationsDrawer } from "./notifications-drawer";

export function NotificationBellTrigger() {
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const { data: unreadCount = 0 } = useQuery(getUnreadCountOptions());

  const displayCount = unreadCount > 9 ? "9+" : unreadCount;

  return (
    <>
      <Button
        variant="ghost"
        size="icon"
        className="relative hover:bg-sidebar-accent hover:text-sidebar-foreground"
        aria-label={
          unreadCount > 0
            ? `Notifications, ${unreadCount} unread`
            : "Notifications"
        }
        onClick={() => setIsDrawerOpen(true)}
      >
        <Bell className="size-5" />
        {unreadCount > 0 && (
          <span className="absolute -top-0.5 -right-0.5 flex h-4.5 min-w-4.5 items-center justify-center rounded-full bg-destructive px-1 text-[10px] font-medium leading-none text-white">
            {displayCount}
          </span>
        )}
      </Button>

      <NotificationsDrawer
        isOpen={isDrawerOpen}
        onOpenChange={setIsDrawerOpen}
      />
    </>
  );
}
