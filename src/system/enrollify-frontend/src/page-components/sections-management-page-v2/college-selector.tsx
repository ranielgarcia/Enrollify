import { useQueryState } from "nuqs";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { ChevronDown } from "lucide-react";
import { generateMockColleges } from "@/lib/mock-data/sections-mock-data";

interface CollegeSelectorProps {
  selectedCollegeId?: string | null;
}

export function CollegeSelector({ selectedCollegeId }: CollegeSelectorProps) {
  const [collegeId, setCollegeId] = useQueryState("collegeId");
  const colleges = generateMockColleges();
  const selectedCollege = colleges.find((c) => c.id === (collegeId ?? selectedCollegeId));

  return (
    <div className="flex items-center gap-2">
      <span className="text-sm font-medium text-muted-foreground">College:</span>
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="outline" className="min-w-64">
            {selectedCollege ? selectedCollege.name : "Select a college..."}
            <ChevronDown className="ml-2 size-4" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent className="w-64">
          {colleges.map((college) => (
            <DropdownMenuItem
              key={college.id}
              onClick={() => setCollegeId(college.id)}
            >
              {college.name}
            </DropdownMenuItem>
          ))}
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );
}
