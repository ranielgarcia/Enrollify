import { useState } from "react";
import { useQueryState } from "nuqs";
import { Button } from "@/components/ui/button";
import { useSuspenseQuery } from "@tanstack/react-query";
import { Building2, ChevronDown, X } from "lucide-react";
import { getAllCollegesOptions } from "@/api/collections/college-collection";
import { SearchCollegesDialog } from "@/components/shared/search-college-dialog";
import { cn } from "@/lib/utils";
import type { College } from "@/api/models/college";

interface CollegeSelectorProps {
  className?: string;
}

export function CollegeSelector({ className }: CollegeSelectorProps) {
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

  return (
    <div className={cn("flex items-center", className)}>
      <Button
        type="button"
        variant="outline"
        onClick={() => setIsDialogOpen(true)}
        className={cn(
          "h-9 min-w-56 justify-start gap-2 pl-2.5 pr-2 font-medium",
          !selectedCollege && "text-muted-foreground",
          selectedCollege && "rounded-r-none border-r-0",
        )}
        aria-label={
          selectedCollege
            ? `College: ${selectedCollege.name}`
            : "Select a college"
        }
      >
        <Building2 className="size-4 shrink-0 text-muted-foreground" />
        <span className="truncate">
          {selectedCollege?.name ?? "Select a college..."}
        </span>
        <ChevronDown className="ml-auto size-4 shrink-0 text-muted-foreground" />
      </Button>
      {selectedCollege && (
        <Button
          type="button"
          variant="outline"
          size="icon"
          onClick={() => setCollegeId(null)}
          className="h-9 w-9 rounded-l-none"
          aria-label="Clear college selection"
        >
          <X className="size-4" />
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
