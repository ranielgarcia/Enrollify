import type { ClassSectionV2 } from "@/api/models/class-section-v2";
import {
  Drawer,
  DrawerContent,
  DrawerDescription,
  DrawerHeader,
  DrawerTitle,
} from "@/components/ui/drawer";

interface SectionFormDrawerProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  sectionToUpdate?: ClassSectionV2;
}

export function SectionFormDrawer({
  isOpen,
  onOpenChange,
  sectionToUpdate,
}: SectionFormDrawerProps) {
  return (
    <Drawer open={isOpen} onOpenChange={onOpenChange}>
      <DrawerContent>
        <DrawerHeader>
          <DrawerTitle>
            {sectionToUpdate ? "Edit Section" : "New Section"}
          </DrawerTitle>
          <DrawerDescription>
            {sectionToUpdate
              ? "Update section details"
              : "Create a new class section"}
          </DrawerDescription>
        </DrawerHeader>
        <div className="flex-1 overflow-y-auto px-4 py-6">
          <div className="text-center text-muted-foreground">
            <p>Form content coming in Phase 2...</p>
          </div>
        </div>
      </DrawerContent>
    </Drawer>
  );
}
