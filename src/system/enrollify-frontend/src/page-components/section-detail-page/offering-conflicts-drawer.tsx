import { useMemo } from "react";
import type { ValidationIssue } from "@/api/models/class-scheduling/offering";
import {
  Drawer,
  DrawerContent,
  DrawerHeader,
  DrawerTitle,
  DrawerFooter,
} from "@/components/ui/drawer";
import { Button } from "@/components/ui/button";
import { ConflictCard } from "./conflict-card";
import SeverityConfig from "@/config/severity-config";

interface OfferingConflictsDrawerProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  offeringSubjectCode: string;
  validationIssues: ValidationIssue[];
}

export function OfferingConflictsDrawer({
  isOpen,
  onOpenChange,
  offeringSubjectCode,
  validationIssues,
}: OfferingConflictsDrawerProps) {
  const groupedConflicts = useMemo(() => {
    const grouped: Record<"Error" | "Warning" | "Info", ValidationIssue[]> = {
      Error: [],
      Warning: [],
      Info: [],
    };

    validationIssues.forEach((issue) => {
      grouped[issue.type.severity as "Error" | "Warning" | "Info"].push(issue);
    });

    return grouped;
  }, [validationIssues]);

  const renderSeverityGroup = (
    severity: "Error" | "Warning" | "Info",
    validationIssueList: ValidationIssue[],
  ) => {
    if (validationIssueList.length === 0) return null;

    const config = SeverityConfig[severity];
    const Icon = config.icon;

    return (
      <section key={severity} className="space-y-3">
        <div className="flex items-center gap-2">
          <Icon className={`size-5 ${config.textColor}`} />
          <h3 className="font-semibold">
            {config.label}
            {validationIssueList.length > 1
              ? ` (${validationIssueList.length})`
              : ""}
          </h3>
        </div>
        <p className="text-xs text-muted-foreground">{config.description}</p>
        <div className="space-y-3">
          {validationIssueList.map((issue, idx) => (
            <ConflictCard key={issue.id ?? idx} validationIssue={issue} />
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
