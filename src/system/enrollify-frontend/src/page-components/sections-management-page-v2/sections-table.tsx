import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import type { ClassSectionV2 } from "@/api/models/class-section-v2";
import { ClassSectionStatusEnum } from "@/api/models/class-section";
import { MoreHorizontal, Edit2, Trash2 } from "lucide-react";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

interface SectionsTableProps {
  sections: ClassSectionV2[];
  totalRecords: number;
  currentPage: number;
  pageSize: number;
  onEdit: (section: ClassSectionV2) => void;
  onDelete: (section: ClassSectionV2) => void;
}

function getStatusBadgeColor(statusValue: number) {
  switch (statusValue) {
    case ClassSectionStatusEnum.Draft:
      return "bg-secondary text-secondary-foreground";
    case ClassSectionStatusEnum.Open:
      return "bg-green-100 text-green-800";
    case ClassSectionStatusEnum.Cancelled:
      return "bg-destructive/20 text-destructive";
    default:
      return "bg-slate-100 text-slate-800";
  }
}

function getErrorIndicator(section: ClassSectionV2) {
  const { offeringsWithErrors, offeringsWithConflicts } = section.validationSummary;
  const totalIssues = offeringsWithErrors + offeringsWithConflicts;

  if (totalIssues === 0) {
    return <Badge variant="outline">✓ OK</Badge>;
  }

  return (
    <div className="flex items-center gap-1">
      {offeringsWithErrors > 0 && (
        <Badge variant="destructive">⚠️ {offeringsWithErrors}</Badge>
      )}
      {offeringsWithConflicts > 0 && (
        <Badge variant="outline">❌ {offeringsWithConflicts}</Badge>
      )}
    </div>
  );
}

export function SectionsTable({
  sections,
  totalRecords,
  currentPage,
  pageSize,
  onEdit,
  onDelete,
}: SectionsTableProps) {
  if (sections.length === 0) {
    return (
      <Card className="p-8 text-center text-muted-foreground">
        <p>No sections found. Please select a college to view sections.</p>
      </Card>
    );
  }

  return (
    <Card className="overflow-hidden">
      <Table>
        <TableHeader>
          <TableRow className="bg-muted/50">
            <TableHead>Section Code</TableHead>
            <TableHead>Full Name</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Teacher</TableHead>
            <TableHead>Offerings</TableHead>
            <TableHead>Issues</TableHead>
            <TableHead className="w-12 text-right">Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {sections.map((section) => (
            <TableRow key={section.id}>
              <TableCell className="font-medium">{section.sectionCode}</TableCell>
              <TableCell className="text-sm">{section.fullName}</TableCell>
              <TableCell>
                <Badge className={getStatusBadgeColor(section.status.value)}>
                  {section.status.name}
                </Badge>
              </TableCell>
              <TableCell className="text-sm">
                {section.adviser
                  ? `${section.adviser.firstName} ${section.adviser.lastName}`
                  : "—"}
              </TableCell>
              <TableCell className="text-sm">
                {section.validationSummary.totalOfferings} offering
                {section.validationSummary.totalOfferings !== 1 ? "s" : ""}
              </TableCell>
              <TableCell>{getErrorIndicator(section)}</TableCell>
              <TableCell className="text-right">
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="ghost" size="sm">
                      <MoreHorizontal className="size-4" />
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end">
                    <DropdownMenuItem onClick={() => onEdit(section)}>
                      <Edit2 className="mr-2 size-4" />
                      Edit
                    </DropdownMenuItem>
                    <DropdownMenuItem
                      onClick={() => onDelete(section)}
                      className="text-destructive"
                    >
                      <Trash2 className="mr-2 size-4" />
                      Delete
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <div className="border-t px-4 py-3 text-sm text-muted-foreground">
        Showing {(currentPage - 1) * pageSize + 1} to{" "}
        {Math.min(currentPage * pageSize, totalRecords)} of {totalRecords} sections
      </div>
    </Card>
  );
}
