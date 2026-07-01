import { useState } from "react";
import {
  Drawer,
  DrawerContent,
  DrawerHeader,
  DrawerTitle,
  DrawerDescription,
  DrawerFooter,
} from "@/components/ui/drawer";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { SearchTeachersDialog } from "@/components/shared/search-teachers-dialog";
import { updateClassSectionOptions } from "@/api/collections/class-section-collection";
import { useMutation } from "@tanstack/react-query";
import type { ClassSectionMinimal } from "@/api/models/class-scheduling/class-section";
import type { Teacher } from "@/api/models/teacher";
import { Loader2, UserRound, X } from "lucide-react";
import { SectionStatusBadge } from "./section-status-badge";

interface SectionFormDrawerProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  sectionToUpdate?: ClassSectionMinimal | null;
}

export function SectionFormDrawer({
  isOpen,
  onOpenChange,
  sectionToUpdate,
}: SectionFormDrawerProps) {
  const isUpdating = !!sectionToUpdate;

  const [isTeacherDialogOpen, setIsTeacherDialogOpen] = useState(false);
  const [selectedAdviser, setSelectedAdviser] = useState<Pick<
    Teacher,
    "id" | "firstName" | "lastName"
  > | null>(
    sectionToUpdate?.adviser
      ? {
          id: sectionToUpdate.adviser.id,
          firstName: sectionToUpdate.adviser.firstName,
          lastName: sectionToUpdate.adviser.lastName,
        }
      : null,
  );

  const { mutateAsync, isPending } = useMutation(
    updateClassSectionOptions(sectionToUpdate?.id ?? 0),
  );

  const handleSubmit = async () => {
    if (!sectionToUpdate || !selectedAdviser) return;
    await mutateAsync({ adviserId: selectedAdviser.id });
    onOpenChange(false);
  };

  const handleClose = () => {
    if (isPending) return;
    onOpenChange(false);
  };

  return (
    <>
      <Drawer
        open={isOpen}
        onOpenChange={onOpenChange}
        direction="right"
        dismissible={!isPending}
      >
        <DrawerContent className="data-[vaul-drawer-direction=right]:w-[480px] data-[vaul-drawer-direction=right]:sm:max-w-none h-full overflow-y-auto overflow-x-hidden">
          <DrawerHeader className="border-b pb-4">
            <DrawerTitle>
              {`Edit "${sectionToUpdate?.name ?? "Section"}"`}
            </DrawerTitle>
            <DrawerDescription>
              Update the adviser assignment for this section.
            </DrawerDescription>
          </DrawerHeader>

          {isUpdating && sectionToUpdate && (
            <div className="flex flex-col gap-4 p-4 flex-1">
              {/* Section info */}
              <div className="rounded-lg border bg-muted/30 p-3 space-y-1.5">
                <div className="flex items-center gap-2">
                  <span className="font-semibold text-sm">
                    {sectionToUpdate.fullName}
                  </span>
                  <SectionStatusBadge
                    status={sectionToUpdate.status}
                    size="sm"
                  />
                </div>
                <div className="text-xs text-muted-foreground">
                  Year Level {sectionToUpdate.intendedYearLevel}
                </div>
              </div>

              {/* Adviser field */}
              <div className="space-y-2">
                <Label>
                  Adviser <span className="text-destructive ml-0.5">*</span>
                </Label>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => setIsTeacherDialogOpen(true)}
                  className="w-full justify-start gap-2 font-normal"
                >
                  <UserRound className="size-4 text-muted-foreground" />
                  {selectedAdviser ? (
                    <span>
                      {selectedAdviser.firstName} {selectedAdviser.lastName}
                    </span>
                  ) : (
                    <span className="text-muted-foreground">
                      Select adviser...
                    </span>
                  )}
                </Button>

                {selectedAdviser && (
                  <div className="flex items-center gap-2 rounded-md border bg-emerald-50 dark:bg-emerald-950/20 border-emerald-200 dark:border-emerald-800 px-3 py-2 text-sm">
                    <UserRound className="size-4 text-emerald-600 dark:text-emerald-400 shrink-0" />
                    <span className="flex-1 font-medium text-emerald-800 dark:text-emerald-300">
                      {selectedAdviser.firstName} {selectedAdviser.lastName}
                    </span>
                    <button
                      type="button"
                      onClick={() => setSelectedAdviser(null)}
                      className="text-emerald-600 dark:text-emerald-400 hover:text-emerald-800 dark:hover:text-emerald-200"
                    >
                      <X className="size-3.5" />
                    </button>
                  </div>
                )}
              </div>
            </div>
          )}

          <DrawerFooter className="border-t">
            {isUpdating && (
              <Button
                onClick={handleSubmit}
                disabled={!selectedAdviser || isPending}
                className="w-full"
              >
                {isPending && <Loader2 className="mr-2 size-4 animate-spin" />}
                Save Changes
              </Button>
            )}
            <Button
              variant="outline"
              onClick={handleClose}
              disabled={isPending}
            >
              Cancel
            </Button>
          </DrawerFooter>
        </DrawerContent>
      </Drawer>

      <SearchTeachersDialog
        isOpen={isTeacherDialogOpen}
        onOpenChange={setIsTeacherDialogOpen}
        title="Select Adviser"
        description="Search and select an adviser for this section."
        maxSelections={1}
        excludeTeacherIds={selectedAdviser ? [selectedAdviser.id] : []}
        onSubmit={async (teachers) => {
          const teacher = teachers[0];
          if (teacher) {
            setSelectedAdviser({
              id: teacher.id,
              firstName: teacher.firstName,
              lastName: teacher.lastName,
            });
          }
        }}
      />
    </>
  );
}
