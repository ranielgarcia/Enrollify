import { useMemo } from "react";
import type { ConflictResult } from "@/api/models/offering";
import {
  Drawer,
  DrawerContent,
  DrawerHeader,
  DrawerTitle,
  DrawerFooter,
} from "@/components/ui/drawer";
import { Button } from "@/components/ui/button";
import { AlertCircle, AlertTriangle, Info, X } from "lucide-react";
import { ConflictCard } from "./conflict-card";

interface OfferingConflictsDrawerProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  offeringSubjectCode: string;
  conflicts: ConflictResult[];
}

const severityConfig = {
  Error: {
    label: "Hard Conflicts",
    icon: AlertCircle,
    textColor: "text-destructive",
    description: "These must be resolved before the schedule can be published.",
  },
  Warning: {
    label: "Warnings",
    icon: AlertTriangle,
    textColor: "text-amber-600 dark:text-amber-500",
    description: "These are advisory — review before publishing.",
  },
  Info: {
    label: "Information",
    icon: Info,
    textColor: "text-blue-600 dark:text-blue-500",
    description: "Additional information about this offering.",
  },
} as const;

export function OfferingConflictsDrawer({
  isOpen,
  onOpenChange,
  offeringSubjectCode,
  conflicts,
}: OfferingConflictsDrawerProps) {
  const groupedConflicts = useMemo(() => {
    const grouped: Record<"Error" | "Warning" | "Info", ConflictResult[]> = {
      Error: [],
      Warning: [],
      Info: [],
    };

    conflicts.forEach((conflict) => {
      grouped[conflict.severity as "Error" | "Warning" | "Info"].push(
        conflict,
      );
    });

    return grouped;
  }, [conflicts]);

  const renderSeverityGroup = (
    severity: "Error" | "Warning" | "Info",
    conflictList: ConflictResult[],
  ) => {
    if (conflictList.length === 0) return null;

    const config = severityConfig[severity];
    const Icon = config.icon;

    return (
      <section key={severity} className="space-y-3">
        <div className="flex items-center gap-2">
          <Icon className={`size-5 ${config.textColor}`} />
          <h3 className="font-semibold">
            {config.label}
            {conflictList.length > 1 ? ` (${conflictList.length})` : ""}
          </h3>
        </div>
        <p className="text-xs text-muted-foreground">{config.description}</p>
        <div className="space-y-3">
          {conflictList.map((conflict, idx) => (
            <ConflictCard key={conflict.id ?? idx} conflict={conflict} />
          ))}
        </div>
      </section>
    );
  };

  return (
    <Drawer open={isOpen} onOpenChange={onOpenChange} direction="right">
      <DrawerContent className="data-[vaul-drawer-direction=right]:w-[480px] data-[vaul-drawer-direction=right]:sm:max-w-none">
        <DrawerHeader className="border-b pb-4">
          <DrawerTitle>
            Conflicts —{" "}
            <span className="font-mono text-sm">{offeringSubjectCode}</span>
          </DrawerTitle>
        </DrawerHeader>

        <div className="flex-1 overflow-y-auto px-4 py-4 space-y-6">
          {renderSeverityGroup("Error", groupedConflicts.Error)}
          {renderSeverityGroup("Warning", groupedConflicts.Warning)}
          {renderSeverityGroup("Info", groupedConflicts.Info)}
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
