import { createColumnHelper } from "@tanstack/react-table";
import { Button } from "@/components/ui/button";
import { Edit2, Trash2 } from "lucide-react";
import { DataTable } from "@/components/data-table/data-table";
import { DataTableColumnHeader } from "@/components/data-table/data-table-column-header";
import { DataTableToolbar } from "@/components/data-table/data-table-toolbar";
import { useDataTable } from "@/hooks/use-data-table";
import type { College } from "@/api/models/college";
import { useMemo } from "react";
import { truncateText } from "@/lib/text-utils";
import { useTablePermissions } from "@/hooks/use-table-permissions";
import { DataTableColumnActionsHeader } from "@/components/data-table/data-table-column-action-header";

interface CollegesTableProps {
  colleges?: College[];
  onEdit: (college: College) => void;
  onDelete: (college: College) => void;
}

export function CollegesTable({
  colleges,
  onEdit,
  onDelete,
}: CollegesTableProps) {
  const { canUpdate, canDelete } = useTablePermissions(
    "canUpdateCollege",
    "canDeleteCollege",
  );
  const columnHelper = createColumnHelper<College>();

  const columns = useMemo(
    () => [
      columnHelper.accessor("code", {
        id: "code",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Code" />
        ),
        meta: { label: "Code", variant: "text" },
        enableColumnFilter: true,
        size: 10,
        cell: (info) => (
          <span className="font-mono text-xs">{info.getValue()}</span>
        ),
      }),
      columnHelper.accessor("name", {
        id: "name",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Name" />
        ),
        meta: { label: "Name", variant: "text" },
        enableColumnFilter: true,
        cell: (info) => <span className="font-medium">{info.getValue()}</span>,
      }),
      columnHelper.accessor("description", {
        id: "description",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Description" />
        ),
        meta: { label: "Description", variant: "text" },
        enableColumnFilter: true,
        cell: (info) => <span>{truncateText(info.getValue(), 30)}</span>,
      }),
      columnHelper.accessor("dean", {
        id: "dean",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Dean" />
        ),
        meta: { label: "Dean", variant: "text" },
        enableColumnFilter: true,
        cell: (info) => <span>{info.getValue() ?? "-"}</span>,
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
        header: () => <DataTableColumnActionsHeader label="Actions" />,
        enableHiding: false,
        cell: (info) => {
          const item = info.row.original;

          return (
            <div className="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
              <Button
                variant="ghost"
                size="sm"
                onClick={() => onEdit(item)}
                className="h-7 w-7 p-0 hover:bg-primary/10 text-primary hover:text-primary"
                disabled={!canUpdate}
              >
                <Edit2 className="size-3.5" />
              </Button>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => onDelete(item)}
                className="h-7 w-7 p-0 hover:bg-destructive/10 text-muted-foreground hover:text-destructive"
                disabled={!canDelete}
              >
                <Trash2 className="size-3.5" />
              </Button>
            </div>
          );
        },
      }),
    ],
    [canUpdate, canDelete, onEdit, onDelete],
  );

  const { table } = useDataTable({
    data: colleges ?? [],
    columns,
    initialState: {
      columnVisibility: {
        createdAt: false,
        createdBy: false,
        updatedAt: false,
        updatedBy: false,
      },
    },
  });

  return (
    <DataTable table={table}>
      <DataTableToolbar table={table} />
    </DataTable>
  );
}
