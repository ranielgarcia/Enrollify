import { formatDistanceToNow, parseISO } from "date-fns";
import { Link } from "@tanstack/react-router";
import { ArrowUpRight, Loader2, Mail, MailOpen, X } from "lucide-react";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import type { Notification } from "@/api/models/notification";
import {
  NotificationCategoryIcons,
  NotificationReferenceTypeConfig,
  NotificationSeverityIcons,
} from "@/config/notification-icons";
import {
  dismissNotificationOptions,
  markNotificationAsReadOptions,
  markNotificationAsUnreadOptions,
} from "@/api/collections/notifications-collection";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Item,
  ItemActions,
  ItemContent,
  ItemDescription,
  ItemFooter,
  ItemMedia,
  ItemTitle,
} from "@/components/ui/item";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";

interface NotificationCardProps {
  notification: Notification;
  /** Invoked right before navigating to a "View" target — used to close the drawer. */
  onNavigate?: () => void;
}

export function NotificationCard({
  notification,
  onNavigate,
}: NotificationCardProps) {
  const severityConfig = NotificationSeverityIcons[notification.severity];
  const SeverityIcon = severityConfig.icon;
  const CategoryIcon = NotificationCategoryIcons[notification.category];
  const referenceConfig = notification.referenceType
    ? NotificationReferenceTypeConfig[notification.referenceType]
    : undefined;

  const { mutate: markAsRead, isPending: isMarkingAsRead } = useMutation(
    markNotificationAsReadOptions(notification.id),
  );
  const { mutate: markAsUnread, isPending: isMarkingAsUnread } = useMutation(
    markNotificationAsUnreadOptions(notification.id),
  );
  const { mutate: dismiss, isPending: isDismissing } = useMutation({
    ...dismissNotificationOptions(notification.id),
    onSuccess: () => {
      toast.success("Notification dismissed");
    },
  });

  const isToggling = isMarkingAsRead || isMarkingAsUnread;
  const relativeTime = formatDistanceToNow(parseISO(notification.createdAt), {
    addSuffix: true,
  });

  return (
    <Item
      variant={notification.isRead ? "default" : "muted"}
      className={cn(
        "border-b border-transparent last:border-b-0 rounded-none px-4 py-3 border-l-2",
        notification.isRead ? "border-l-transparent" : "border-l-primary",
      )}
    >
      <ItemMedia>
        <span
          className={cn(
            "flex size-9 items-center justify-center rounded-full",
            severityConfig.badgeClassName,
          )}
        >
          <SeverityIcon
            className={cn("size-4.5", severityConfig.iconClassName)}
          />
        </span>
      </ItemMedia>

      <ItemContent>
        <ItemTitle>
          {!notification.isRead && (
            <span
              className="size-1.5 shrink-0 rounded-full bg-primary"
              aria-hidden
            />
          )}
          <span className={cn(!notification.isRead && "font-semibold")}>
            {notification.title}
          </span>
        </ItemTitle>
        <ItemDescription>{notification.message}</ItemDescription>
        <ItemFooter className="mt-1 gap-2">
          <div className="flex items-center gap-2 text-xs text-muted-foreground">
            <Badge variant="outline" className="gap-1 font-normal">
              <CategoryIcon className="size-3" />
              {notification.category}
            </Badge>
            <span>{relativeTime}</span>
          </div>
        </ItemFooter>
      </ItemContent>

      <ItemActions className="self-start">
        {referenceConfig && notification.referenceId != null && (
          <Tooltip>
            <TooltipTrigger asChild>
              <Button
                variant="ghost"
                size="icon"
                className="size-8 text-muted-foreground hover:text-primary"
                asChild
              >
                <Link
                  to={referenceConfig.buildHref(notification.referenceId)}
                  onClick={() => onNavigate?.()}
                >
                  <ArrowUpRight className="size-4" />
                </Link>
              </Button>
            </TooltipTrigger>
            <TooltipContent>View details</TooltipContent>
          </Tooltip>
        )}

        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              variant="ghost"
              size="icon"
              className="size-8 text-muted-foreground hover:text-foreground"
              disabled={isToggling}
              onClick={() =>
                notification.isRead ? markAsUnread() : markAsRead()
              }
            >
              {isToggling ? (
                <Loader2 className="size-4 animate-spin" />
              ) : notification.isRead ? (
                <Mail className="size-4" />
              ) : (
                <MailOpen className="size-4" />
              )}
            </Button>
          </TooltipTrigger>
          <TooltipContent>
            {notification.isRead ? "Mark as unread" : "Mark as read"}
          </TooltipContent>
        </Tooltip>

        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              variant="ghost"
              size="icon"
              className="size-8 text-muted-foreground hover:text-destructive"
              disabled={isDismissing}
              onClick={() => dismiss()}
            >
              {isDismissing ? (
                <Loader2 className="size-4 animate-spin" />
              ) : (
                <X className="size-4" />
              )}
            </Button>
          </TooltipTrigger>
          <TooltipContent>Dismiss</TooltipContent>
        </Tooltip>
      </ItemActions>
    </Item>
  );
}
