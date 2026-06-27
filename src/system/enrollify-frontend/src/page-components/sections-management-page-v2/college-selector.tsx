import { useState } from "react";
import { useQueryState } from "nuqs";
import { Button } from "@/components/ui/button";
import { useSuspenseQuery } from "@tanstack/react-query";
import { X } from "lucide-react";
import { getAllCollegesOptions } from "@/api/collections/college-collection";
import { SearchCollegesDialog } from "@/components/shared/search-college-dialog";
import type { College } from "@/api/models/college";

export function CollegeSelector() {
  const [collegeId, setCollegeId] = useQueryState("collegeId");
  const [isDialogOpen, setIsDialogOpen] = useState(false);

  const { data: colleges = [] } = useSuspenseQuery({
    ...getAllCollegesOptions(),
  });

  const selectedCollege = collegeId
    ? colleges.find((c) => c.id.toString() === collegeId)
    : undefined;

  const handleSelectCollege = async (selectedColleges: College[]) => {
    const college = selectedColleges[0];
    if (college) {
      await setCollegeId(college.id.toString());
    }
  };

  const handleClear = () => {
    setCollegeId(null);
  };

  return (
    <div className="flex items-center gap-2">
      <span className="text-sm font-medium text-muted-foreground">
        College:
      </span>
      {selectedCollege ? (
        <div className="flex items-center gap-1">
          <Button
            variant="outline"
            className="min-w-64 justify-start"
            onClick={() => setIsDialogOpen(true)}
          >
            {selectedCollege.name}
          </Button>
          <Button
            variant="ghost"
            size="icon"
            className="size-8"
            onClick={handleClear}
            aria-label="Clear college selection"
          >
            <X className="size-4" />
          </Button>
        </div>
      ) : (
        <Button
          variant="outline"
          className="min-w-64 justify-start"
          onClick={() => setIsDialogOpen(true)}
        >
          Select a college...
        </Button>
      )}
      <SearchCollegesDialog
        isOpen={isDialogOpen}
        onOpenChange={setIsDialogOpen}
        title="Select College"
        description="Search and select a college to filter sections."
        maxSelections={1}
        onSubmit={handleSelectCollege}
      />
    </div>
  );
}
