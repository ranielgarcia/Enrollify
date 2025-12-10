import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { useState } from "react";
import type { RoomType } from "./room-types-table";

export function RoomTypeForm() {
  const [typeFormData, setTypeFormData] = useState<Partial<RoomType>>({
    name: "",
    description: "",
  });
  return (
    <Card className="border-2 border-accent mb-4">
      <CardContent className="pt-6">
        <form className="space-y-4">
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
          <div className="flex gap-2 justify-end pt-4">
            <Button
              variant="outline"
              type="button"
              //   onClick={() => {
              //     setShowTypeForm(false);
              //     setEditingTypeId(null);
              //   }}
            >
              Cancel
            </Button>
            <Button
              type="button"
              //   onClick={handleSaveRoomType}
              className="bg-primary text-primary-foreground hover:bg-primary/90"
            >
              Save Room Type
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
