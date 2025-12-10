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

interface RoomType {
  id: string;
  name: string;
  description: string;
}

interface RoomTypesTableProps {
  roomTypes: RoomType[];
  onEdit: (roomType: RoomType) => void;
  onDelete: (id: string) => void;
}

export function RoomTypesTable({
  roomTypes,
  onEdit,
  onDelete,
}: RoomTypesTableProps) {
  const columns: ColumnDef<RoomType>[] = [
    {
      accessorKey: "name",
      header: "Name",
      cell: ({ row }) => {
        <span className="font-semibold text-accent">
          {row.getValue("name")}
        </span>;
      },
    },
    {
      accessorKey: "description",
      header: "Description",
      cell: ({ row }) => {
        <span className="font-semibold text-accent">
          {row.getValue("description")}
        </span>;
      },
    },
    {
      id: "actions",
      header: "Actions",
      cell: (info) => {
        const roomType = info.row.original;
        return (
          <div className="flex gap-2">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onEdit(roomType)}
              className="hover:bg-blue-500/10 text-blue-600 hover:text-blue-700"
            >
              <Edit2 className="size-4" />
            </Button>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onDelete(roomType.id)}
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
    data: roomTypes,
    columns,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
  });

  return <DataTable table={table} columns={columns} />;
}
