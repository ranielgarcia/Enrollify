import {
  createColumnHelper,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  useReactTable,
} from "@tanstack/react-table";
import { Button } from "@/components/ui/button";
import { Edit2, Trash2 } from "lucide-react";
import { formatDateTime } from "@/lib/dateutils";
import { DataTable } from "@/components/data-table";
import type { College } from "@/api/models/college";

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
  const columnHelper = createColumnHelper<College>();

  const columns = [
    columnHelper.accessor("code", {
      header: "Code",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("name", {
      header: "Name",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("description", {
      header: "Description",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("dean", {
      header: "Dean",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("createdAt", {
      header: "Created At",
      cell: (info) => <span>{formatDateTime(info.getValue())}</span>,
    }),
    columnHelper.accessor(
      (row) =>
        row.createdByUser
          ? `${row.createdByUser?.firstName} ${row.createdByUser?.lastName}`
          : "Err",
      {
        id: "createdBy",
        header: "Created By",
        cell: (info) => <span>{info.getValue()}</span>,
      }
    ),
    columnHelper.accessor("updatedAt", {
      header: "Updated At",
      cell: (info) => <span>{formatDateTime(info.getValue())}</span>,
    }),
    columnHelper.accessor(
      (row) =>
        row.createdByUser?.firstName
          ? `${row.createdByUser?.firstName} ${row.createdByUser?.lastName}`
          : "Err",
      {
        id: "updatedBy",
        header: "Updated By",
        cell: (info) => <span>{info.getValue()}</span>,
      }
    ),
    columnHelper.display({
      id: "actions",
      header: "Actions",
      cell: (info) => {
        const item = info.row.original;
        return (
          <div className="flex gap-2">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onEdit(item)}
              className="hover:bg-blue-500/10 text-blue-600 hover:text-blue-700"
            >
              <Edit2 className="size-4" />
            </Button>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onDelete(item)}
              className="hover:bg-destructive/10 text-destructive hover:text-destructive"
            >
              <Trash2 className="size-4" />
            </Button>
          </div>
        );
      },
    }),
  ];

  // eslint-disable-next-line react-hooks/incompatible-library
  const table = useReactTable({
    data: colleges ?? [],
    columns,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
  });

  return <DataTable table={table} columns={columns} />;
}
