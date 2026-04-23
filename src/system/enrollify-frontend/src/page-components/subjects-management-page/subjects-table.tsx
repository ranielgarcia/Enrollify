import type { PagedResult } from "@/api/models/paged-result";
import type { Subject } from "@/api/models/subject";
import { DataTable } from "@/components/data-table/data-table";
import { DataTableColumnHeader } from "@/components/data-table/data-table-column-header";
import { Button } from "@/components/ui/button";
import { truncateText } from "@/lib/text-utils";
import { createColumnHelper } from "@tanstack/react-table";
import { Edit2, Trash2 } from "lucide-react";
import { useMemo } from "react";
import { useTablePermissions } from "@/hooks/use-table-permissions";
import { useDataTable } from "@/hooks/use-data-table";
import { SubjectsTableActionBar } from "./subjects-table-action-bar";
import { DataTableSortList } from "@/components/data-table/data-table-sort-list";
import { DataTableAdvancedToolbar } from "@/components/data-table/data-table-advanced-toolbar";
import { DataTableFilterList } from "@/components/data-table/data-table-filter-list";

interface SubjectsTableProps {
  pagedSubjects: PagedResult<Subject>;
  onEdit: (subject: Subject) => void;
  onDelete: (subject: Subject) => void;
}

export function SubjectsTable({
  pagedSubjects,
  onEdit,
  onDelete,
}: SubjectsTableProps) {
  const { canUpdate, canDelete } = useTablePermissions(
    "canUpdateSubject",
    "canDeleteSubject",
  );
  const columnHelper = createColumnHelper<Subject>();

  const columns = useMemo(
    () => [
      columnHelper.accessor("code", {
        id: "code",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Code" />
        ),
        meta: { label: "Code", variant: "text" },
        enableColumnFilter: true,
        cell: (info) => <span>{info.getValue()}</span>,
      }),
      columnHelper.accessor("title", {
        id: "title",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Title" />
        ),
        meta: { label: "Title", variant: "text" },
        enableColumnFilter: true,
        enableSorting: true,
        enableHiding: true,
        cell: (info) => <span>{info.getValue()}</span>,
      }),
      columnHelper.accessor("description", {
        id: "description",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Description" />
        ),
        meta: { label: "Description", variant: "text" },
        enableColumnFilter: true,
        enableSorting: true,
        enableHiding: true,
        cell: (info) => <span>{truncateText(info.getValue(), 30)}</span>,
      }),
      columnHelper.accessor("units", {
        id: "units",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Units" />
        ),
        meta: { label: "Units", variant: "number" },
        enableColumnFilter: true,
        enableSorting: true,
        enableHiding: true,
        cell: (info) => <span>{Number(info.getValue()).toFixed(1)}</span>,
      }),
      columnHelper.accessor((row) => row.preferRoomType?.name, {
        id: "preferRoomType",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Prefer Room Type" />
        ),
        meta: { label: "Prefer Room Type", variant: "text" },
        enableColumnFilter: true,
        enableSorting: true,
        enableHiding: true,
        cell: (info) => <span>{info.getValue()}</span>,
      }),
      columnHelper.accessor("createdAt", {
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Created At" />
        ),
        meta: { label: "Created At" },
        cell: (info) => <span>{info.getValue()}</span>,
      }),
      columnHelper.accessor(
        (row) =>
          row.createdBy
            ? `${row.createdBy.firstName} ${row.createdBy.lastName}`
            : "",
        {
          id: "createdBy",
          header: ({ column }) => (
            <DataTableColumnHeader column={column} label="Created By" />
          ),
          meta: { label: "Created By" },
          cell: (info) => <span>{info.getValue()}</span>,
        },
      ),
      columnHelper.accessor("updatedAt", {
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Updated At" />
        ),
        meta: { label: "Updated At" },
        cell: (info) => <span>{info.getValue()}</span>,
      }),
      columnHelper.accessor(
        (row) =>
          row.updatedBy?.firstName
            ? `${row.updatedBy.firstName} ${row.updatedBy.lastName}`
            : "",
        {
          id: "updatedBy",
          header: ({ column }) => (
            <DataTableColumnHeader column={column} label="Updated By" />
          ),
          meta: { label: "Updated By" },
          cell: (info) => <span>{info.getValue()}</span>,
        },
      ),
      columnHelper.display({
        id: "actions",
        header: "Actions",
        enableHiding: false,
        cell: (info) => {
          const item = info.row.original;

          return (
            <div className="flex gap-2">
              <Button
                variant="ghost"
                size="sm"
                onClick={() => onEdit(item)}
                className="hover:bg-blue-500/10 text-blue-600 hover:text-blue-700"
                disabled={!canUpdate}
              >
                <Edit2 className="size-4" />
              </Button>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => onDelete(item)}
                className="hover:bg-destructive/10 text-destructive hover:text-destructive"
                disabled={!canDelete}
              >
                <Trash2 className="size-4" />
              </Button>
            </div>
          );
        },
      }),
    ],
    [canUpdate, canDelete, onEdit, onDelete],
  );

  const { table, shallow, debounceMs, throttleMs } = useDataTable({
    data: pagedSubjects?.items ?? [],
    columns,
    pageCount: pagedSubjects?.totalPages ?? -1,
    manualPagination: true,
  });

  return (
    <DataTable
      table={table}
      actionBar={<SubjectsTableActionBar table={table} />}
    >
      {/* <DataTableToolbar table={table}>
        <DataTableSortList table={table} align="end" />
      </DataTableToolbar> */}

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
