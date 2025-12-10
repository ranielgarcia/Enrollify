import type { ColumnDef } from "@tanstack/react-table";
import { createColumnHelper } from "@tanstack/react-table";
import {
  useReactTable,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
} from "@tanstack/react-table";
import { Button } from "@/components/ui/button";
import { DataTable } from "@/components/ui/data-table";
import { Edit2, Trash2 } from "lucide-react";
import { Badge } from "@/components/ui/badge";

interface Room {
  id: string;
  roomNumber: string;
  building: string;
  capacity: number;
  type: string;
}

interface RoomsTableProps {
  rooms: Room[];
  onEdit: (room: Room) => void;
  onDelete: (id: string) => void;
}

export function RoomsTable({ rooms, onEdit, onDelete }: RoomsTableProps) {
  //   const columnHelper = createColumnHelper<Room>();

  const columns: ColumnDef<Room>[] = [
    {
        
    }
  ];

  //   const columns: ColumnDef<Room>[] = [
  //     columnHelper.accessor("roomNumber", {
  //       header: "Room Number",
  //       cell: (info) => (
  //         <span className="font-semibold text-accent">{info.getValue()}</span>
  //       ),
  //     }),
  //     columnHelper.accessor("building", {
  //       header: "Building",
  //       cell: (info) => <span>{info.getValue()}</span>,
  //     }),
  //     columnHelper.accessor("type", {
  //       header: "Type",
  //       cell: (info) => {
  //         const type = info.getValue();
  //         const colors: Record<string, string> = {
  //           "Lecture Hall": "bg-blue-500/10 text-blue-700 hover:bg-blue-500/20",
  //           Lab: "bg-purple-500/10 text-purple-700 hover:bg-purple-500/20",
  //           "Seminar Room": "bg-teal-500/10 text-teal-700 hover:bg-teal-500/20",
  //           "Tutorial Room":
  //             "bg-orange-500/10 text-orange-700 hover:bg-orange-500/20",
  //           Auditorium: "bg-pink-500/10 text-pink-700 hover:bg-pink-500/20",
  //         };
  //         return <Badge className={colors[type] || ""}>{type}</Badge>;
  //       },
  //     }),
  //     columnHelper.accessor("capacity", {
  //       header: "Capacity",
  //       cell: (info) => <span className="font-medium">{info.getValue()}</span>,
  //     }),
  //     columnHelper.display({
  //       id: "actions",
  //       header: "Actions",
  //       cell: (info) => {
  //         const room = info.row.original;
  //         return (
  //           <div className="flex gap-2">
  //             <Button
  //               variant="ghost"
  //               size="sm"
  //               onClick={() => onEdit(room)}
  //               className="hover:bg-blue-500/10 text-blue-600 hover:text-blue-700"
  //             >
  //               <Edit2 className="size-4" />
  //             </Button>
  //             <Button
  //               variant="ghost"
  //               size="sm"
  //               onClick={() => onDelete(room.id)}
  //               className="hover:bg-destructive/10 text-destructive hover:text-destructive"
  //             >
  //               <Trash2 className="size-4" />
  //             </Button>
  //           </div>
  //         );
  //       },
  //     }),
  //   ];

  // eslint-disable-next-line react-hooks/incompatible-library
  const table = useReactTable({
    data: rooms,
    columns,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
  });

  return <DataTable table={table} columns={columns} />;
}
