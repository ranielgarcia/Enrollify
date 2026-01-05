import type { PagedResult } from "@/api/models/paged-result";
import type { Subject } from "@/api/models/subject";
import { DataTable } from "@/components/data-table";
import { Button } from "@/components/ui/button";
import { useAuthorization } from "@/infrastructure/authorization/components/useAuthorization";
import { truncateText } from "@/lib/text-utils";
import {
  createColumnHelper,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  useReactTable,
} from "@tanstack/react-table";
import { Edit2, Trash2 } from "lucide-react";
import { useEffect, useState } from "react";

interface SubjectsTableProps {
  pagedSubjects: PagedResult<Subject>;
  onEdit: (subject: Subject) => void;
  onDelete: (subject: Subject) => void;
  onPreviousPage: () => void;
  onNextPage: () => void;
}

export function SubjectsTable({
  pagedSubjects,
  onEdit,
  onDelete,
  onPreviousPage,
  onNextPage,
}: SubjectsTableProps) {
  const { checkPolicy } = useAuthorization();
  const columnHelper = createColumnHelper<Subject>();
  const [canUpdate, setCanUpdate] = useState(false);
  const [canDelete, setCanDelete] = useState(false);

  useEffect(() => {
    const checkPolicies = async () => {
      const [updatePermission, deletePermission] = await Promise.all([
        checkPolicy("canUpdateSubject"),
        checkPolicy("canDeleteSubject"),
      ]);
      setCanUpdate(updatePermission);
      setCanDelete(deletePermission);
    };

    checkPolicies();
  }, [checkPolicy]);

  const columns = [
    columnHelper.accessor("code", {
      header: "Code",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("title", {
      header: "Title",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("description", {
      header: "Description",
      cell: (info) => <span>{truncateText(info.getValue(), 30)}</span>,
    }),
    columnHelper.accessor("units", {
      header: "Units",
      cell: (info) => <span>{Number(info.getValue()).toFixed(1)}</span>,
    }),
    columnHelper.accessor((row) => row.preferRoomType?.name, {
      id: "preferRoomType",
      header: "Prefer Room Type",
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
    data: pagedSubjects?.items ?? [],
    columns,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    manualPagination: true,
    pageCount: pagedSubjects?.totalPages ?? -1,
    state: {
      pagination: {
        pageIndex: (pagedSubjects?.page ?? 1) - 1, // Convert 1-indexed to 0-indexed
        pageSize: pagedSubjects?.pageSize ?? 10,
      },
    },
  });

  return (
    <DataTable
      table={table}
      columns={columns}
      showPaginationButtons
      onPreviousPage={onPreviousPage}
      onNextPage={onNextPage}
      currentPage={pagedSubjects?.page}
      totalPages={pagedSubjects?.totalPages}
    />
  );
}
