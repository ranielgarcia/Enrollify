import type { PagedResult } from "@/api/models/paged-result";
import type { ClassSection } from "@/api/models/class-section";
import { DataTable } from "@/components/data-table/data-table";
import { DataTableColumnHeader } from "@/components/data-table/data-table-column-header";
import { DataTableAdvancedToolbar } from "@/components/data-table/data-table-advanced-toolbar";
import { DataTableFilterList } from "@/components/data-table/data-table-filter-list";
import { DataTableSortList } from "@/components/data-table/data-table-sort-list";
import { DataTableColumnActionsHeader } from "@/components/data-table/data-table-column-action-header";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { useDataTable } from "@/hooks/use-data-table";
import { useTablePermissions } from "@/hooks/use-table-permissions";
import { createColumnHelper } from "@tanstack/react-table";
import { Edit2, Trash2, Eye } from "lucide-react";
import { useMemo } from "react";
import { useNavigate } from "@tanstack/react-router";
import { SectionStatusBadge } from "./section-status-badge";

interface SectionsTableProps {
  pagedSections: PagedResult<ClassSection>;
  onEdit: (section: ClassSection) => void;
  onDelete: (section: ClassSection) => void;
}

const columnHelper = createColumnHelper<ClassSection>();

const YEAR_LEVEL_LABELS: Record<number, string> = {
  1: "1st Year",
  2: "2nd Year",
  3: "3rd Year",
  4: "4th Year",
  5: "5th Year",
  6: "6th Year",
};

export function SectionsTable({
  pagedSections,
  onEdit,
  onDelete,
}: SectionsTableProps) {
  const navigate = useNavigate();
  const { canUpdate, canDelete } = useTablePermissions(
    "canUpdateClassSection",
    "canDeleteClassSection",
  );

  const columns = useMemo(
    () => [
      columnHelper.accessor("name", {
        id: "name",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Section Name" />
        ),
        meta: { label: "Section Name", variant: "text" },
        enableColumnFilter: true,
        cell: (info) => (
          <span className="font-semibold">{info.getValue()}</span>
        ),
      }),
      columnHelper.accessor("sectionCode", {
        id: "sectionCode",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Code" />
        ),
        meta: { label: "Section Code", variant: "text" },
        enableColumnFilter: true,
        cell: (info) => (
          <span className="font-mono text-sm">{info.getValue() ?? "-"}</span>
        ),
      }),
      columnHelper.accessor((row) => row.course?.name, {
        id: "courseName",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Course / Program" />
        ),
        meta: { label: "Course Name", variant: "text" },
        enableColumnFilter: true,
        cell: (info) => (
          <span>
            <span className="font-mono text-xs text-muted-foreground mr-1.5">
              {info.row.original.course?.code}
            </span>
            {info.getValue() ?? "-"}
          </span>
        ),
      }),
      columnHelper.accessor("intendedYearLevel", {
        id: "intendedYearLevel",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Year Level" />
        ),
        meta: { label: "Year Level", variant: "number" },
        enableColumnFilter: true,
        cell: (info) => (
          <Badge variant="secondary">
            {YEAR_LEVEL_LABELS[info.getValue()] ?? `Year ${info.getValue()}`}
          </Badge>
        ),
      }),
      columnHelper.accessor((row) => row.curriculum?.version, {
        id: "curriculumVersion",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Curriculum" />
        ),
        meta: { label: "Curriculum Version", variant: "text" },
        enableColumnFilter: true,
        cell: (info) => (
          <span className="text-sm text-muted-foreground">
            {info.getValue() ?? "-"}
          </span>
        ),
      }),
      columnHelper.accessor((row) => row.academicTerm?.termName, {
        id: "academicTerm",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Academic Term" />
        ),
        meta: { label: "Academic Term", variant: "text" },
        cell: (info) => <span>{info.getValue() ?? "-"}</span>,
      }),
      columnHelper.accessor(
        (row) =>
          row.adviser
            ? `${row.adviser.firstName} ${row.adviser.lastName}`
            : "-",
        {
          id: "adviserName",
          header: ({ column }) => (
            <DataTableColumnHeader column={column} label="Adviser" />
          ),
          meta: { label: "Adviser Name", variant: "text" },
          enableColumnFilter: true,
          cell: (info) => (
            <span>
              <span>{info.getValue()}</span>
              {info.row.original.adviser?.email && (
                <span className="block text-xs text-muted-foreground">
                  {info.row.original.adviser.email}
                </span>
              )}
            </span>
          ),
        },
      ),
      columnHelper.accessor("statusId", {
        id: "status",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Status" />
        ),
        meta: { label: "Status", variant: "text" },
        enableColumnFilter: true,
        cell: (info) => <SectionStatusBadge status={info.getValue()} />,
      }),
      columnHelper.display({
        id: "actions",
        header: () => <DataTableColumnActionsHeader label="Actions" />,
        size: 10,
        enableHiding: false,
        cell: (info) => {
          const item = info.row.original;
          return (
            <div className="flex gap-1">
              <Button
                variant="ghost"
                size="sm"
                onClick={() =>
                  navigate({
                    to: "/portal/curriculum-and-scheduling/sections/$sectionId",
                    params: { sectionId: String(item.id) },
                  })
                }
                className="hover:bg-primary/10 text-primary hover:text-primary"
                title="View details"
              >
                <Eye className="size-4" />
              </Button>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => onEdit(item)}
                className="hover:bg-blue-500/10 text-blue-600 hover:text-blue-700"
                disabled={!canUpdate}
                title="Edit"
              >
                <Edit2 className="size-4" />
              </Button>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => onDelete(item)}
                className="hover:bg-destructive/10 text-destructive hover:text-destructive"
                disabled={!canDelete}
                title="Delete"
              >
                <Trash2 className="size-4" />
              </Button>
            </div>
          );
        },
      }),
    ],
    [canUpdate, canDelete, onEdit, onDelete, navigate],
  );

  const { table, shallow, debounceMs, throttleMs } = useDataTable({
    data: pagedSections?.items ?? [],
    columns,
    pageCount: pagedSections?.totalPages ?? -1,
    manualPagination: true,
    debounceMs: 600,
    initialState: {
      columnVisibility: {},
    },
  });

  return (
    <DataTable table={table}>
      <DataTableAdvancedToolbar table={table}>
        <DataTableSortList table={table} align="start" />
        <DataTableFilterList
          table={table}
          shallow={shallow}
          debounceMs={debounceMs}
          throttleMs={throttleMs}
          align="start"
        />
      </DataTableAdvancedToolbar>
    </DataTable>
  );
}
