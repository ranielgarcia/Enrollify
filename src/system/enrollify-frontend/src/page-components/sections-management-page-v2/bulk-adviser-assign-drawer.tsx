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
import { Badge } from "@/components/ui/badge";
import { SearchTeachersDialog } from "@/components/shared/search-teachers-dialog";
import { bulkAssignAdviserMutationOptions } from "@/api/collections/class-section-collection";
import { useMutation } from "@tanstack/react-query";
import type { Teacher } from "@/api/models/teacher";
import { Loader2, UserRound, X } from "lucide-react";

interface BulkAdviserAssignDrawerProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  sectionIds: number[];
  onSuccess: () => void;
}

export function BulkAdviserAssignDrawer({
  isOpen,
  onOpenChange,
  sectionIds,
  onSuccess,
}: BulkAdviserAssignDrawerProps) {
  const [selectedAdviser, setSelectedAdviser] = useState<Teacher | null>(null);
  const [isTeacherDialogOpen, setIsTeacherDialogOpen] = useState(false);

  const { mutateAsync, isPending } = useMutation(
    bulkAssignAdviserMutationOptions(),
  );

  const handleSubmit = async () => {
    if (!selectedAdviser) return;

    await mutateAsync({
      classSectionAdviserAssignments: sectionIds.map((id) => ({
        classSectionId: id,
        adviserId: selectedAdviser.id,
      })),
    });

    onSuccess();
    onOpenChange(false);
    setSelectedAdviser(null);
  };

  const handleClose = () => {
    if (isPending) return;
    setSelectedAdviser(null);
    onOpenChange(false);
  };

  return (
    <>
      <Drawer open={isOpen} onOpenChange={onOpenChange} direction="right">
        <DrawerContent className="data-[vaul-drawer-direction=right]:w-[440px] data-[vaul-drawer-direction=right]:sm:max-w-none h-full overflow-y-auto overflow-x-hidden">
          <DrawerHeader className="border-b pb-4">
            <DrawerTitle>Assign Adviser</DrawerTitle>
            <DrawerDescription>
              Assign a single adviser to{" "}
              <Badge variant="secondary" className="text-sm">
                {sectionIds.length} Draft section
                {sectionIds.length !== 1 ? "s" : ""}
              </Badge>
            </DrawerDescription>
          </DrawerHeader>

          <div className="flex flex-col gap-4 p-4 flex-1">
            <div className="rounded-md border bg-muted/30 p-3 text-sm text-muted-foreground">
              The selected adviser will be assigned as the section adviser for
              all {sectionIds.length} selected section
              {sectionIds.length !== 1 ? "s" : ""}. Existing advisers will be
              replaced.
            </div>

            {/* Adviser selector */}
            <div className="space-y-2">
              <label className="text-sm font-medium">Adviser</label>
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
                    {selectedAdviser.teacherIdentifier && (
                      <span className="ml-2 text-muted-foreground text-xs">
                        ({selectedAdviser.teacherIdentifier})
                      </span>
                    )}
                  </span>
                ) : (
                  <span className="text-muted-foreground">
                    Select an adviser...
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

          <DrawerFooter className="border-t">
            <Button
              onClick={handleSubmit}
              disabled={!selectedAdviser || isPending}
              className="w-full"
            >
              {isPending && <Loader2 className="mr-2 size-4 animate-spin" />}
              Assign Adviser to {sectionIds.length} Section
              {sectionIds.length !== 1 ? "s" : ""}
            </Button>
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
        description="Search and select an adviser to assign to the selected sections."
        maxSelections={1}
        onSubmit={async (teachers) => {
          if (teachers[0]) {
            setSelectedAdviser(teachers[0]);
          }
        }}
      />
    </>
  );
}
