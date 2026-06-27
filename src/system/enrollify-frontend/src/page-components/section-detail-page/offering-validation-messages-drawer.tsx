import { useMemo } from "react";
import type { OfferingValidationMessage } from "@/api/models/class-scheduling/class-section";
import {
  Drawer,
  DrawerContent,
  DrawerHeader,
  DrawerTitle,
  DrawerFooter,
} from "@/components/ui/drawer";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { AlertTriangle, AlertCircle, Info, X } from "lucide-react";

interface OfferingValidationMessagesDrawerProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  offeringSubjectCode: string;
  validationMessages: OfferingValidationMessage[];
}

const severityConfig = {
  error: {
    label: "Errors",
    icon: AlertTriangle,
    badgeVariant: "destructive" as const,
    backgroundColor: "bg-destructive/5",
    textColor: "text-destructive",
    borderColor: "border-destructive/20",
  },
  warning: {
    label: "Warnings",
    icon: AlertCircle,
    badgeVariant: "secondary" as const,
    backgroundColor: "bg-amber-500/5",
    textColor: "text-amber-700 dark:text-amber-400",
    borderColor: "border-amber-400/20",
  },
  info: {
    label: "Information",
    icon: Info,
    badgeVariant: "secondary" as const,
    backgroundColor: "bg-blue-500/5",
    textColor: "text-blue-700 dark:text-blue-400",
    borderColor: "border-blue-400/20",
  },
} as const;

export function OfferingValidationMessagesDrawer({
  isOpen,
  onOpenChange,
  offeringSubjectCode,
  validationMessages,
}: OfferingValidationMessagesDrawerProps) {
  const groupedMessages = useMemo(() => {
    const grouped: Record<
      "error" | "warning" | "info",
      OfferingValidationMessage[]
    > = {
      error: [],
      warning: [],
      info: [],
    };

    validationMessages.forEach((msg) => {
      const severity = msg.severity.name.toLowerCase() as
        | "error"
        | "warning"
        | "info";
      grouped[severity].push(msg);
    });

    // Sort each group by computedAt (oldest first)
    Object.keys(grouped).forEach((key) => {
      grouped[key as "error" | "warning" | "info"].sort(
        (a, b) =>
          new Date(a.computedAt).getTime() - new Date(b.computedAt).getTime(),
      );
    });

    return grouped;
  }, [validationMessages]);

  const renderSeverityGroup = (
    severity: "error" | "warning" | "info",
    messages: OfferingValidationMessage[],
  ) => {
    if (messages.length === 0) return null;

    const config = severityConfig[severity];
    const Icon = config.icon;

    return (
      <div key={severity} className="space-y-2">
        <div className="flex items-center gap-2">
          <Icon className={`size-4 ${config.textColor}`} />
          <h3 className="text-sm font-semibold">
            {config.label}
            {messages.length > 1 ? ` (${messages.length})` : ""}
          </h3>
        </div>

        <div className="space-y-2 ml-6">
          {messages.map((msg, idx) => (
            <div
              key={idx}
              className={`rounded-lg border p-3 ${config.backgroundColor} ${config.borderColor}`}
            >
              <div className="flex items-start gap-2">
                <div className="flex-1 min-w-0">
                  <p className="text-sm text-foreground">{msg.message}</p>
                  <div className="flex items-center gap-2 mt-1.5 text-xs text-muted-foreground">
                    {msg.code && (
                      <span className="font-mono bg-muted px-1.5 py-0.5 rounded">
                        {msg.code}
                      </span>
                    )}
                    <span>{msg.computedAt}</span>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  };

  return (
    <Drawer open={isOpen} onOpenChange={onOpenChange} direction="right">
      <DrawerContent className="data-[vaul-drawer-direction=right]:w-[480px] data-[vaul-drawer-direction=right]:sm:max-w-none">
        <DrawerHeader className="border-b pb-4">
          <DrawerTitle>
            Validation Issues —{" "}
            <span className="font-mono text-sm">{offeringSubjectCode}</span>
          </DrawerTitle>
        </DrawerHeader>

        <div className="flex-1 overflow-y-auto px-4 py-4 space-y-6">
          {renderSeverityGroup("error", groupedMessages.error)}
          {renderSeverityGroup("warning", groupedMessages.warning)}
          {renderSeverityGroup("info", groupedMessages.info)}
        </div>

        <DrawerFooter className="border-t">
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Close
          </Button>
        </DrawerFooter>
      </DrawerContent>
    </Drawer>
  );
}
