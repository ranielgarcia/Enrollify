import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ACADEMIC_TERM_SYSTEMS } from "@/constants/academic-systems";
import { useEnrollmentContext } from "@/contexts/enrollment-context/enrollment-context";
import {
  CalendarClock,
  CalendarDays,
  CalendarRange,
  Edit2Icon,
} from "lucide-react";
import React from "react";

export function EnrollmentContextDialog() {
  const {
    academicYears,
    activeAcademicYear,
    selectedAcademicYear,
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
      <Button variant="link" size="sm" onClick={() => setOpen(true)}>
        {selectedAcademicYear?.academicYearTitle ?? "Not selected"}{" "}
        <Edit2Icon className="size-3.5" />
      </Button>

      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-primary/10 ring-1 ring-primary/20">
              <CalendarDays className="h-5 w-5 text-primary" />
            </div>
            <div>
              <DialogTitle className="text-base">
                Enrollment Context
              </DialogTitle>
              <p className="text-xs text-muted-foreground mt-0.5">
                Set the academic year for your current working session.
              </p>
            </div>
          </div>
        </DialogHeader>

        <div className="space-y-5 py-1">
          {/* Current context hero card */}
          <div className="relative overflow-hidden rounded-xl border bg-linear-to-br from-primary/5 via-background to-background p-4 shadow-sm">
            <div className="pointer-events-none absolute -right-6 -top-6 h-28 w-28 rounded-full bg-primary/8 blur-2xl" />
            <div className="relative space-y-3">
              <p className="text-xs font-semibold uppercase tracking-widest text-muted-foreground">
                Current Context
              </p>

              {/* Academic year row */}
              <div className="flex items-center justify-between gap-2">
                <div className="flex items-center gap-2.5">
                  <div className="flex h-7 w-7 items-center justify-center rounded-md bg-primary/10">
                    <CalendarDays className="h-3.5 w-3.5 text-primary" />
                  </div>
                  <div>
                    <p className="text-sm font-semibold leading-none">
                      {selectedAcademicYear?.academicYearTitle ??
                        "Not selected"}
                    </p>
                    <p className="text-xs text-muted-foreground mt-0.5">
                      Academic Year
                    </p>
                  </div>
                </div>
                {selectedAcademicYear &&
                activeAcademicYear &&
                selectedAcademicYear?.id === activeAcademicYear?.id ? (
                  <Badge className="text-[11px] px-2 py-0.5 font-semibold bg-emerald-500/15 text-emerald-600 border border-emerald-500/25 hover:bg-emerald-500/15">
                    ● Active
                  </Badge>
                ) : (
                  <Badge className="text-[11px] px-2 py-0.5 font-semibold bg-muted text-muted-foreground border hover:bg-muted">
                    ● Inactive
                  </Badge>
                )}
              </div>

              {/* Term system row */}
              <div className="flex items-center gap-2.5">
                <div className="flex h-7 w-7 items-center justify-center rounded-md bg-primary/10">
                  <CalendarClock className="h-3.5 w-3.5 text-primary" />
                </div>
                <div>
                  <p className="text-sm font-semibold leading-none">
                    {termSystemName}
                  </p>
                  <p className="text-xs text-muted-foreground mt-0.5">
                    Term System
                  </p>
                </div>
              </div>
            </div>
          </div>

          {/* Dashed divider */}
          <div className="relative pb-2">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-dashed" />
            </div>
          </div>

          {/* Change academic year form section */}
          <section className="space-y-3">
            <div className="flex items-center gap-2">
              <div className="flex h-7 w-7 items-center justify-center rounded-md bg-primary/10">
                <CalendarRange className="h-3.5 w-3.5 text-primary" />
              </div>
              <div>
                <p className="text-sm font-semibold leading-none">
                  Change Academic Year
                </p>
                <p className="text-xs text-muted-foreground mt-0.5">
                  Select the year you want to work in
                </p>
              </div>
            </div>

            <Select value={localSlug} onValueChange={setLocalSlug}>
              <SelectTrigger>
                <SelectValue placeholder="Select an academic year…" />
              </SelectTrigger>
              <SelectContent>
                {academicYears.map((ay) => (
                  <SelectItem key={ay.id} value={ay.academicYearSlug}>
                    {activeAcademicYear && activeAcademicYear.id === ay.id
                      ? `${ay.academicYearTitle} (Active)`
                      : ay.academicYearTitle}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </section>
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
