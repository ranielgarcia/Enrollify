import type { Curriculum } from "@/api/models/curriculum";
import { DataTable } from "@/components/data-table";
import { Button } from "@/components/ui/button";
import { useAuthorization } from "@/infrastructure/authorization/components/useAuthorization";
import {
  createColumnHelper,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  useReactTable,
} from "@tanstack/react-table";
import { Edit2 } from "lucide-react";
import { useEffect, useState } from "react";

interface CurriculumsTableProps {
  curriculums?: Curriculum[];
  onEdit: (curriculum: Curriculum) => void;
}

export function CurriculumsTable({
  curriculums,
  onEdit,
}: CurriculumsTableProps) {
  const { checkPolicy } = useAuthorization();
  const columnHelper = createColumnHelper<Curriculum>();
  const [canUpdate, setCanUpdate] = useState(false);

  useEffect(() => {
    const checkPolicies = async () => {
      const [updatePermission] = await Promise.all([
        checkPolicy("canUpdateCurriculum"),
      ]);
      setCanUpdate(updatePermission);
    };

    checkPolicies();
  }, [checkPolicy]);

  const columns = [
    columnHelper.accessor((row) => row.course.name, {
      header: "Course",
      id: "course",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("effectiveYear", {
      header: "Effective Year",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("version", {
      header: "Version",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor((row) => row.status.name, {
      header: "Status",
      id: "status",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("description", {
      header: "Description",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("approvedDate", {
      header: "Approved Date",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("createdAt", {
      header: "Created At",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor(
      (row) =>
        row.createdBy
          ? `${row.createdBy.firstName} ${row.createdBy.lastName}`
          : "",
      {
        id: "createdBy",
        header: "Created By",
        cell: (info) => <span>{info.getValue()}</span>,
      }
    ),
    columnHelper.accessor("updatedAt", {
      header: "Updated At",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor(
      (row) =>
        row.updatedBy?.firstName
          ? `${row.updatedBy.firstName} ${row.updatedBy.lastName}`
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
          </div>
        );
      },
    }),
  ];

  // eslint-disable-next-line react-hooks/incompatible-library
  const table = useReactTable({
    data: curriculums ?? [],
    columns,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
  });

  return <DataTable table={table} columns={columns} />;
}
