import { createColumnHelper } from "@tanstack/react-table";
import { Button } from "@/components/ui/button";
import { DataTable } from "@/components/data-table/data-table";
import { DataTableColumnHeader } from "@/components/data-table/data-table-column-header";
import { DataTableToolbar } from "@/components/data-table/data-table-toolbar";
import { useDataTable } from "@/hooks/use-data-table";
import { Edit2, Trash2 } from "lucide-react";
import type { Room } from "@/api/models/room";
import { useMemo } from "react";
import { useTablePermissions } from "@/hooks/use-table-permissions";

interface RoomsTableProps {
  rooms?: Room[];
  onEdit: (room: Room) => void;
  onDelete: (room: Room) => void;
}

export function RoomsTable({ rooms, onEdit, onDelete }: RoomsTableProps) {
  const { canUpdate, canDelete } = useTablePermissions(
    "canUpdateRooms",
    "canDeleteRooms",
  );
  const columnHelper = createColumnHelper<Room>();

  const columns = useMemo(
    () => [
      columnHelper.accessor("roomNumber", {
        id: "roomNumber",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Room Number" />
        ),
        meta: { label: "Room Number", variant: "text" },
        enableColumnFilter: true,
        cell: (info) => (
          <span className="font-mono text-xs">{info.getValue()}</span>
        ),
      }),
      columnHelper.accessor("capacity", {
        id: "capacity",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Capacity" />
        ),
        meta: { label: "Capacity", variant: "number" },
        enableColumnFilter: true,
        cell: (info) => <span>{info.getValue()}</span>,
      }),
      columnHelper.accessor((row) => row.roomType.name, {
        id: "roomType",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Room Type" />
        ),
        meta: { label: "Room Type", variant: "text" },
        enableColumnFilter: true,
        cell: (info) => <span>{info.getValue()}</span>,
      }),
      columnHelper.accessor((row) => row.building.name, {
        id: "building",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Building" />
        ),
        meta: { label: "Building", variant: "text" },
        enableColumnFilter: true,
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

  const { table } = useDataTable({
    data: rooms ?? [],
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
