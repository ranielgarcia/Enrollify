import type { Building } from "@/api/models/building";
import { DataTable } from "@/components/data-table";
import { Button } from "@/components/ui/button";
import { useAuthorization } from "@/infrastructure/authorization/components/useAuthorization";
import { formatDateTime } from "@/lib/dateutils";
import {
  createColumnHelper,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  useReactTable,
} from "@tanstack/react-table";
import { Edit2, Trash2 } from "lucide-react";
import { useEffect, useState } from "react";

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
  const { checkPolicy } = useAuthorization();
  const columnHelper = createColumnHelper<Building>();
  const [canUpdate, setCanUpdate] = useState(false);
  const [canDelete, setCanDelete] = useState(false);

  useEffect(() => {
    const checkPolicies = async () => {
      const [updatePermission, deletePermission] = await Promise.all([
        checkPolicy("canUpdateBuilding"),
        checkPolicy("canDeleteBuilding"),
      ]);
      setCanUpdate(updatePermission);
      setCanDelete(deletePermission);
    };

    checkPolicies();
  }, [checkPolicy]);

  const columns = [
    columnHelper.accessor("name", {
      header: "Name",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("description", {
      header: "Description",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("address", {
      header: "Address",
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
        row.updatedByUser?.firstName
          ? `${row.updatedByUser?.firstName} ${row.updatedByUser?.lastName}`
          : "",
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
  ];

  // eslint-disable-next-line react-hooks/incompatible-library
  const table = useReactTable({
    data: buildings ?? [],
    columns,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
  });

  return <DataTable table={table} columns={columns} />;
}
