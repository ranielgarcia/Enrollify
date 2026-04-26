import type { Building } from "@/api/models/building";
import { DataTable } from "@/components/data-table/data-table";
import { DataTableColumnHeader } from "@/components/data-table/data-table-column-header";
import { DataTableToolbar } from "@/components/data-table/data-table-toolbar";
import { Button } from "@/components/ui/button";
import { useDataTable } from "@/hooks/use-data-table";
import { createColumnHelper } from "@tanstack/react-table";
import { Edit2, Trash2 } from "lucide-react";
import { useMemo } from "react";
import { useTablePermissions } from "@/hooks/use-table-permissions";
import { formatDateTimeString } from "@/lib/date-utils";

interface BuildingsTableProps {
  buildings?: Building[];
  onEdit: (building: Building) => void;
  onDelete: (building: Building) => void;
}

export function BuildingsTable({
  buildings,
  onEdit,
  onDelete,
}: BuildingsTableProps) {
  const { canUpdate, canDelete } = useTablePermissions(
    "canUpdateBuilding",
    "canDeleteBuilding",
  );
  const columnHelper = createColumnHelper<Building>();

  const columns = useMemo(
    () => [
      columnHelper.accessor("name", {
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Name" />
        ),
        meta: { label: "Name" },
        cell: (info) => <span>{info.getValue()}</span>,
      }),
      columnHelper.accessor("description", {
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Description" />
        ),
        meta: { label: "Description" },
        cell: (info) => <span>{info.getValue()}</span>,
      }),
      columnHelper.accessor("address", {
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Address" />
        ),
        meta: { label: "Address" },
        cell: (info) => <span>{info.getValue()}</span>,
      }),
      columnHelper.accessor((row) => row.college.name, {
        id: "college",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="College" />
        ),
        meta: { label: "College" },
        cell: (info) => <span>{info.getValue()}</span>,
      }),
      columnHelper.accessor("createdAt", {
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Created At" />
        ),
        meta: { label: "Created At" },
        cell: (info) => <span>{formatDateTimeString(info.getValue())}</span>,
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

  const { table } = useDataTable({
    data: buildings ?? [],
    columns,
  });

  return (
    <DataTable table={table}>
      <DataTableToolbar table={table} />
    </DataTable>
  );
}
