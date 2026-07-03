import { useQuery } from "@tanstack/react-query";
import {
  Drawer,
  DrawerContent,
  DrawerFooter,
  DrawerHeader,
  DrawerTitle,
  DrawerDescription,
} from "@/components/ui/drawer";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { getOfferingDetailsForClassSectionOptions } from "@/api/collections/class-section-collection";
import { AlertTriangle, CheckCircle2, Map, Siren } from "lucide-react";
import { OfferingValidationSection } from "./offering-validation-section";

interface ConflictPreviewDrawerProps {
  sectionId: number | null;
  sectionName: string;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function ConflictPreviewDrawer({
  sectionId,
  sectionName,
  isOpen,
  onOpenChange,
}: ConflictPreviewDrawerProps) {
  const { data: offerings, isLoading } = useQuery({
    ...getOfferingDetailsForClassSectionOptions(sectionId ?? 0),
    enabled: isOpen && !!sectionId,
  });

  const offeringsWithValidationIssues =
    offerings?.filter((o) => (o.validationIssues?.length ?? 0) > 0) ?? [];

  const totalValidationIssues = offeringsWithValidationIssues.reduce(
    (sum, o) => sum + (o.validationIssues?.length ?? 0),
    0,
  );

  const errorValidationIssues = offeringsWithValidationIssues.reduce(
    (sum, o) =>
      sum +
      (o.validationIssues?.filter((issue) => issue.type.severity === "Error")
        .length ?? 0),
    0,
  );

  return (
    <Drawer open={isOpen} onOpenChange={onOpenChange} direction="right">
      <DrawerContent className="data-[vaul-drawer-direction=right]:w-[480px] data-[vaul-drawer-direction=right]:sm:max-w-none h-full overflow-y-auto overflow-x-hidden">
        <DrawerHeader className="border-b pb-4">
          <DrawerTitle className="flex items-center gap-2">
            <Siren className="size-5 text-rose-500" />
            Validation Issues Details
          </DrawerTitle>
          <DrawerDescription>
            Scheduling validation issues for{" "}
            <span className="font-semibold text-foreground">{sectionName}</span>
          </DrawerDescription>
        </DrawerHeader>

        <div className="flex flex-col gap-4 p-4">
          {isLoading ? (
            <div className="space-y-3">
              {Array.from({ length: 3 }).map((_, i) => (
                <Skeleton key={i} className="h-24 w-full rounded-lg" />
              ))}
            </div>
          ) : offeringsWithValidationIssues.length === 0 ? (
            <div className="flex flex-col items-center gap-3 py-12 text-center">
              <CheckCircle2 className="size-10 text-emerald-400" />
              <div>
                <p className="font-medium text-sm">
                  No Validation Issues Detected
                </p>
                <p className="text-xs text-muted-foreground mt-0.5">
                  This section has no scheduling validation issues.
                </p>
              </div>
            </div>
          ) : (
            <>
              {errorValidationIssues > 0 && (
                <Alert variant="destructive">
                  <AlertTitle>Cannot open for enrollment</AlertTitle>
                  <AlertDescription>
                    {errorValidationIssues} Error-severity conflict
                    {errorValidationIssues !== 1 ? "s" : ""} must be resolved in
                    the Room Scheduler before this section can be opened.
                  </AlertDescription>
                </Alert>
              )}

              {/* Summary */}
              <div className="flex items-center gap-3 rounded-lg border bg-muted/30 p-3">
                <div className="flex items-center gap-2 text-sm">
                  <Siren className="size-4 text-rose-500" />
                  <span className="font-semibold tabular-nums text-rose-600 dark:text-rose-400">
                    {totalValidationIssues}
                  </span>
                  <span className="text-muted-foreground">
                    total validation issue
                    {totalValidationIssues !== 1 ? "s" : ""}
                  </span>
                </div>
                <div className="h-4 w-px bg-border" />
                <div className="flex items-center gap-2 text-sm">
                  <AlertTriangle className="size-4 text-rose-500" />
                  <span className="font-semibold tabular-nums text-rose-600 dark:text-rose-400">
                    {errorValidationIssues}
                  </span>
                  <span className="text-muted-foreground">
                    blocking enrollment
                  </span>
                </div>
              </div>

              {/* Validation issue list by offering */}
              <div className="space-y-4">
                {offeringsWithValidationIssues.map((offering) => (
                  <OfferingValidationSection
                    key={offering.id}
                    offering={offering}
                  />
                ))}
              </div>
            </>
          )}
        </div>

        <DrawerFooter className="border-t pt-4">
          <Button
            variant="outline"
            onClick={() => {
              // stub: navigation to room scheduler TBD
            }}
            className="w-full gap-2"
          >
            <Map className="size-4" />
            Open Room Scheduler
          </Button>
        </DrawerFooter>
      </DrawerContent>
    </Drawer>
  );
}
