import { Button } from "@/components/ui/button";
import {
  Drawer,
  DrawerClose,
  DrawerContent,
  DrawerDescription,
  DrawerFooter,
  DrawerHeader,
  DrawerTitle,
  DrawerTrigger,
} from "@/components/ui/drawer";
import { useState } from "react";
import type { RoomType } from "./room-types-table";
import { Plus } from "lucide-react";

export function RoomTypeFormDrawer() {
  const [isOpen, setIsOpen] = useState(false);
  const [typeFormData, setTypeFormData] = useState<Partial<RoomType>>({
    name: "",
    description: "",
  });
  return (
    <Drawer
      direction="right"
      dismissible={false}
      open={isOpen}
      onOpenChange={setIsOpen}
    >
      <DrawerTrigger asChild>
        <Button className="gap-2 bg-primary text-primary-foreground hover:bg-primary/90">
          <Plus className="size-4" />
          Add Room Type
        </Button>
      </DrawerTrigger>
      <DrawerContent>
        <form className="space-y-4">
          <div className="mx-auto w-full max-w-sm">
            <DrawerHeader>
              <DrawerTitle>Create Room Type</DrawerTitle>
              <DrawerDescription>Set room type details.</DrawerDescription>
            </DrawerHeader>
            <div className="p-4 pb-0">
              <div>
                <label className="text-sm font-medium text-foreground block mb-1">
                  Type Name
                </label>
                <input
                  className="w-full px-3 py-2 rounded-lg border border-border bg-background text-foreground"
                  placeholder="e.g., Lecture Hall"
                  value={typeFormData.name || ""}
                  onChange={(e) =>
                    setTypeFormData({
                      ...typeFormData,
                      name: e.target.value,
                    })
                  }
                />
              </div>
              <div>
                <label className="text-sm font-medium text-foreground block mb-1">
                  Description
                </label>
                <input
                  className="w-full px-3 py-2 rounded-lg border border-border bg-background text-foreground"
                  placeholder="e.g., Large classroom for lectures"
                  value={typeFormData.description || ""}
                  onChange={(e) =>
                    setTypeFormData({
                      ...typeFormData,
                      description: e.target.value,
                    })
                  }
                />
              </div>
            </div>

            <DrawerFooter>
              <Button>Submit</Button>
              <DrawerClose asChild>
                <Button variant="outline" onClick={() => setIsOpen(false)}>
                  Cancel
                </Button>
              </DrawerClose>
            </DrawerFooter>
          </div>
        </form>
      </DrawerContent>
    </Drawer>
  );
}
