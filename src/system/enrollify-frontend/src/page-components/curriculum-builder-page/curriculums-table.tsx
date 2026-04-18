import type { Curriculum } from "@/api/models/curriculum";
import { DataTable } from "@/components/data-table/data-table";
import { DataTableColumnHeader } from "@/components/data-table/data-table-column-header";
import { DataTableToolbar } from "@/components/data-table/data-table-toolbar";
import { Button } from "@/components/ui/button";
import { useAuthorization } from "@/infrastructure/authorization/components/useAuthorization";
import { useDataTable } from "@/hooks/use-data-table";
import { truncateText } from "@/lib/text-utils";
import { createColumnHelper } from "@tanstack/react-table";
import { Edit2 } from "lucide-react";
import { useEffect, useMemo, useState } from "react";

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

  const columns = useMemo(() => [
    columnHelper.accessor((row) => row.course.name, {
      id: "course",
      header: ({ column }) => <DataTableColumnHeader column={column} label="Course" />,
      meta: { label: "Course" },
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("effectiveYear", {
      header: ({ column }) => <DataTableColumnHeader column={column} label="Effective Year" />,
      meta: { label: "Effective Year" },
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("version", {
      header: ({ column }) => <DataTableColumnHeader column={column} label="Version" />,
      meta: { label: "Version" },
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor((row) => row.status.name, {
      id: "status",
      header: ({ column }) => <DataTableColumnHeader column={column} label="Status" />,
      meta: { label: "Status" },
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("description", {
      header: ({ column }) => <DataTableColumnHeader column={column} label="Description" />,
      meta: { label: "Description" },
      cell: (info) => <span>{truncateText(info.getValue(), 30)}</span>,
    }),
    columnHelper.accessor("approvedDate", {
      header: ({ column }) => <DataTableColumnHeader column={column} label="Approved Date" />,
      meta: { label: "Approved Date" },
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("createdAt", {
      header: ({ column }) => <DataTableColumnHeader column={column} label="Created At" />,
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
        header: ({ column }) => <DataTableColumnHeader column={column} label="Created By" />,
        meta: { label: "Created By" },
        cell: (info) => <span>{info.getValue()}</span>,
      },
    ),
    columnHelper.accessor("updatedAt", {
      header: ({ column }) => <DataTableColumnHeader column={column} label="Updated At" />,
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
        header: ({ column }) => <DataTableColumnHeader column={column} label="Updated By" />,
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
          </div>
        );
      },
    }),
  ], [canUpdate, onEdit]);

  const { table } = useDataTable({
    data: curriculums ?? [],
    columns,
  });

  return (
    <DataTable table={table}>
      <DataTableToolbar table={table} />
    </DataTable>
  );
}
