import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ACADEMIC_TERM_SYSTEMS } from "@/constants/academic-systems";
import { useEnrollmentContext } from "@/contexts/enrollment-context/enrollment-context";
import { Edit2Icon } from "lucide-react";
import React from "react";

export function EnrollmentContextDialog() {
  const {
    academicYears,
    selectedAcademicYearSlug,
    academicCoreSettings: { academicTermSystem },
    setSelectedAcademicYearSlug,
  } = useEnrollmentContext();

  const termSystemName =
    ACADEMIC_TERM_SYSTEMS[academicTermSystem] || "Unknown Term System";

  const [localSlug, setLocalSlug] = React.useState<string>(
    selectedAcademicYearSlug ?? "",
  );
  const [open, setOpen] = React.useState(false);

  React.useEffect(() => {
    if (open) {
      setLocalSlug(selectedAcademicYearSlug ?? "");
    }
  }, [open, selectedAcademicYearSlug]);

  const handleSave = () => {
    setSelectedAcademicYearSlug(localSlug || null);
    setOpen(false);
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="link">
          Manage Context <Edit2Icon />
        </Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>Enrollment Context</DialogTitle>
          <DialogDescription>
            Select the academic year you want to work in. The term system is
            determined by your institution&apos;s settings.
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-4">
          {/* Term System (read-only) */}
          <div className="grid gap-1.5">
            <Label className="text-xs text-muted-foreground uppercase tracking-wide">
              Term System
            </Label>
            <p className="text-sm font-medium">{termSystemName}</p>
          </div>

          {/* Academic Year Select */}
          <div className="grid gap-1.5">
            <Label htmlFor="academic-year-select">Academic Year</Label>
            <Select value={localSlug} onValueChange={setLocalSlug}>
              <SelectTrigger id="academic-year-select">
                <SelectValue placeholder="Select an academic year" />
              </SelectTrigger>
              <SelectContent>
                {academicYears.map((ay) => (
                  <SelectItem key={ay.id} value={ay.academicYearSlug}>
                    {ay.academicYearTitle}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>

        <DialogFooter>
          <DialogClose asChild>
            <Button variant="outline">Cancel</Button>
          </DialogClose>
          <Button onClick={handleSave} disabled={!localSlug}>
            Apply
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
