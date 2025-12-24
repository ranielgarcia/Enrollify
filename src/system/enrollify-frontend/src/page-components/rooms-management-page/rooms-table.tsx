import type { ColumnDef } from "@tanstack/react-table";
import {
  useReactTable,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
} from "@tanstack/react-table";
import { Button } from "@/components/ui/button";
import { DataTable } from "@/components/data-table";
import { Edit2, Trash2 } from "lucide-react";
import type { Room } from "@/api/models/room";

interface RoomsTableProps {
  rooms?: Room[];
  onEdit: (room: Room) => void;
  onDelete: (id: number) => void;
}

export function RoomsTable({ rooms, onEdit, onDelete }: RoomsTableProps) {
  const columns: ColumnDef<Room>[] = [
    {
      accessorKey: "roomNumber",
      header: "Room Number",
      cell: ({ row }) => (
        <span className="font-semibold text-accent">
          {row.getValue("roomNumber")}
        </span>
      ),
    },
    {
      accessorKey: "capacity",
      header: "Capacity",
      cell: ({ row }) => <span>{row.getValue("capacity")}</span>,
    },
    {
      accessorKey: "type",
      header: "Type",
      cell: ({ row }) => <span>{row.getValue("type")}</span>,
    },
    {
      accessorKey: "building",
      header: "Building",
      cell: ({ row }) => <span>{row.getValue("building")}</span>,
    },
    {
      accessorKey: "college",
      header: "College",
      cell: ({ row }) => <span>{row.getValue("college")}</span>,
    },
    {
      id: "actions",
      header: "Actions",
      cell: (info) => {
        const room = info.row.original;
        return (
          <div className="flex gap-2">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onEdit(room)}
              className="hover:bg-blue-500/10 text-blue-600 hover:text-blue-700"
            >
              <Edit2 className="size-4" />
            </Button>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onDelete(room.id)}
              className="hover:bg-destructive/10 text-destructive hover:text-destructive"
            >
              <Trash2 className="size-4" />
            </Button>
          </div>
        );
      },
    },
  ];

  // eslint-disable-next-line react-hooks/incompatible-library
  const table = useReactTable({
    data: rooms ?? [],
    columns,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
  });

  return <DataTable table={table} columns={columns} />;
}
