import type { AcademicYear } from "@/api/models/academic-year";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { CalendarDays, Edit2, Trash2 } from "lucide-react";
import { format, parseISO } from "date-fns";

interface PreviousAcademicYearsTabProps {
  previousYears: AcademicYear[];
  onEdit: (year: AcademicYear) => void;
  onDelete: (year: AcademicYear) => void;
}

function formatDate(dateStr?: string | null): string {
  if (!dateStr) return "—";
  try {
    return format(parseISO(dateStr), "MMM d, yyyy");
  } catch {
    return dateStr;
  }
}

/** Derives max term count across all years to build dynamic columns */
function getMaxTermCount(years: AcademicYear[]): number {
  return Math.max(0, ...years.map((y) => y.academicTerms?.length ?? 0));
}

export function PreviousAcademicYearsTab({
  previousYears,
  onEdit,
  onDelete,
}: PreviousAcademicYearsTabProps) {
  if (previousYears.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-20 gap-4 text-center">
        <div className="flex h-16 w-16 items-center justify-center rounded-full bg-muted">
          <CalendarDays className="h-8 w-8 text-muted-foreground" />
        </div>
        <div>
          <h3 className="text-lg font-semibold">No Previous Academic Years</h3>
          <p className="text-sm text-muted-foreground mt-1">
            Previous academic years will appear here once they become inactive.
          </p>
        </div>
      </div>
    );
  }

  const maxTerms = getMaxTermCount(previousYears);
  const termIndices = Array.from({ length: maxTerms }, (_, i) => i);

  return (
    <div className="rounded-md border overflow-x-auto">
      <Table>
        <TableHeader>
          {/* Top header row */}
          <TableRow>
            <TableHead rowSpan={2} className="border-r align-middle min-w-[160px]">
              Academic Year
            </TableHead>
            <TableHead rowSpan={2} className="border-r align-middle min-w-[130px]">
              Start Date
            </TableHead>
            <TableHead rowSpan={2} className="border-r align-middle min-w-[130px]">
              End Date
            </TableHead>
            {termIndices.map((i) => (
              <TableHead
                key={i}
                colSpan={3}
                className="text-center border-r border-l"
              >
                Term {i + 1}
              </TableHead>
            ))}
            <TableHead rowSpan={2} className="align-middle text-right min-w-[100px]">
              Actions
            </TableHead>
          </TableRow>
          {/* Second header row — term sub-columns */}
          {maxTerms > 0 && (
            <TableRow>
              {termIndices.map((i) => (
                <>
                  <TableHead key={`${i}-name`} className="text-xs min-w-[80px]">
                    Name
                  </TableHead>
                  <TableHead key={`${i}-start`} className="text-xs min-w-[110px]">
                    Start
                  </TableHead>
                  <TableHead key={`${i}-end`} className="text-xs border-r min-w-[110px]">
                    End
                  </TableHead>
                </>
              ))}
            </TableRow>
          )}
        </TableHeader>
        <TableBody>
          {previousYears.map((year) => {
            const sortedTerms = (year.academicTerms ?? [])
              .slice()
              .sort((a, b) => (a.termNumber ?? 0) - (b.termNumber ?? 0));

            return (
              <TableRow key={year.id}>
                {/* Academic Year Name */}
                <TableCell className="border-r font-medium">
                  <div className="flex items-center gap-2">
                    <Badge variant="outline" className="font-mono text-xs">
                      {year.academicYearTitle ??
                        `AY ${year.startYear}–${year.endYear}`}
                    </Badge>
                  </div>
                </TableCell>

                {/* Actual Start Date */}
                <TableCell className="border-r text-sm">
                  {formatDate(year.startDate)}
                </TableCell>

                {/* Actual End Date */}
                <TableCell className="border-r text-sm">
                  {formatDate(year.endDate)}
                </TableCell>

                {/* Term columns */}
                {termIndices.map((i) => {
                  const term = sortedTerms[i];
                  return (
                    <>
                      <TableCell key={`${year.id}-${i}-name`} className="text-sm">
                        {term?.termName ?? "—"}
                      </TableCell>
                      <TableCell key={`${year.id}-${i}-start`} className="text-sm">
                        {formatDate(term?.startDate)}
                      </TableCell>
                      <TableCell
                        key={`${year.id}-${i}-end`}
                        className="text-sm border-r"
                      >
                        {formatDate(term?.endDate)}
                      </TableCell>
                    </>
                  );
                })}

                {/* Actions */}
                <TableCell className="text-right">
                  <div className="flex justify-end gap-1">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => onEdit(year)}
                      className="hover:bg-blue-500/10 text-blue-600 hover:text-blue-700"
                    >
                      <Edit2 className="size-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => onDelete(year)}
                      className="hover:bg-destructive/10 text-destructive hover:text-destructive"
                    >
                      <Trash2 className="size-4" />
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </div>
  );
}
