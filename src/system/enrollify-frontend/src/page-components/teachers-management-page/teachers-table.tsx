import type { PagedResult } from "@/api/models/paged-result";
import type { Teacher } from "@/api/models/teacher";
import { DataTable } from "@/components/data-table/data-table";
import { DataTableColumnHeader } from "@/components/data-table/data-table-column-header";
import { DataTableAdvancedToolbar } from "@/components/data-table/data-table-advanced-toolbar";
import { DataTableFilterList } from "@/components/data-table/data-table-filter-list";
import { DataTableSortList } from "@/components/data-table/data-table-sort-list";
import { Button as PrimitiveButton } from "@/components/ui/button";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { useDataTable } from "@/hooks/use-data-table";
import { useTablePermissions } from "@/hooks/use-table-permissions";
import { createColumnHelper } from "@tanstack/react-table";
import { BookOpen, Edit2 } from "lucide-react";
import { useMemo } from "react";
import { TeachersTableActionBar } from "./teachers-table-action-bar";
import { DerivedButton } from "@/components/derived/button";

interface TeachersTableProps {
  pagedTeachers: PagedResult<Teacher>;
  onEdit: (teacher: Teacher) => void;
  onManageSubjects: (teacher: Teacher) => void;
}

export function TeachersTable({
  pagedTeachers,
  onEdit,
  onManageSubjects,
}: TeachersTableProps) {
  const { canUpdate } = useTablePermissions(
    "canUpdateTeacher",
    "canUpdateTeacher",
  );
  const columnHelper = createColumnHelper<Teacher>();

  const columns = useMemo(
    () => [
      columnHelper.display({
        id: "avatar",
        header: "",
        enableHiding: false,
        cell: (info) => {
          const teacher = info.row.original;
          const initials = `${teacher.firstName?.[0] ?? ""}${teacher.lastName?.[0] ?? ""}`;
          return (
            <Avatar className="size-9">
              <AvatarFallback className="text-xs font-semibold">
                {initials}
              </AvatarFallback>
            </Avatar>
          );
        },
      }),
      columnHelper.accessor("teacherIdentifier", {
        id: "teacherIdentifier",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Teacher ID" />
        ),
        meta: { label: "Teacher ID", variant: "text" },
        enableColumnFilter: true,
        cell: (info) => (
          <span className="font-mono text-xs">{info.getValue()}</span>
        ),
      }),
      columnHelper.accessor(
        (row) =>
          `${row.firstName}${row.middleName ? " " + row.middleName : ""} ${row.lastName}`,
        {
          id: "fullName",
          header: ({ column }) => (
            <DataTableColumnHeader column={column} label="Full Name" />
          ),
          meta: { label: "Full Name", variant: "text" },
          enableColumnFilter: true,
          cell: (info) => (
            <span className="font-medium">{info.getValue()}</span>
          ),
        },
      ),
      columnHelper.accessor("academicTitle", {
        id: "academicTitle",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Academic Title" />
        ),
        meta: { label: "Academic Title", variant: "text" },
        enableColumnFilter: true,
        cell: (info) => <span>{info.getValue() ?? "-"}</span>,
      }),
      columnHelper.accessor("email", {
        id: "email",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Email" />
        ),
        meta: { label: "Email", variant: "text" },
        enableColumnFilter: true,
        cell: (info) => <span>{info.getValue()}</span>,
      }),
      columnHelper.accessor("phoneNumber", {
        id: "phoneNumber",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Phone" />
        ),
        meta: { label: "Phone" },
        cell: (info) => <span>{info.getValue()}</span>,
      }),
      columnHelper.accessor((row) => row.department?.name, {
        id: "department",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Department" />
        ),
        meta: { label: "Department", variant: "text" },
        enableColumnFilter: true,
        cell: (info) => <span>{info.getValue() ?? "-"}</span>,
      }),
      columnHelper.accessor("specialization", {
        id: "specialization",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Specialization" />
        ),
        meta: { label: "Specialization", variant: "text" },
        enableColumnFilter: true,
        cell: (info) => <span>{info.getValue() ?? "-"}</span>,
      }),
      columnHelper.accessor("officeLocation", {
        id: "officeLocation",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} label="Office" />
        ),
        meta: { label: "Office" },
        cell: (info) => <span>{info.getValue() ?? "-"}</span>,
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
              <PrimitiveButton
                variant="ghost"
                size="sm"
                onClick={() => onEdit(item)}
                className="hover:bg-blue-500/10 text-blue-600 hover:text-blue-700"
                disabled={!canUpdate}
              >
                <Edit2 className="size-4" />
              </PrimitiveButton>
              <DerivedButton
                variant="ghost"
                size="sm"
                onClick={() => onManageSubjects(item)}
                className="hover:bg-amber-500/10 text-amber-600 hover:text-amber-700"
                tooltipSide="bottom"
                tooltip="Manage subjects"
              >
                <BookOpen className="size-4" />
              </DerivedButton>
            </div>
          );
        },
      }),
    ],
    [canUpdate, onEdit, onManageSubjects],
  );

  const { table, shallow, debounceMs, throttleMs } = useDataTable({
    data: pagedTeachers?.items ?? [],
    columns,
    pageCount: pagedTeachers?.totalPages ?? -1,
    manualPagination: true,
    debounceMs: 600,
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
    <DataTable
      table={table}
      actionBar={<TeachersTableActionBar table={table} />}
    >
      <DataTableAdvancedToolbar table={table}>
        <DataTableSortList table={table} align="start" />
        <DataTableFilterList
          table={table}
          shallow={shallow}
          debounceMs={debounceMs}
          throttleMs={throttleMs}
          align="start"
        />
      </DataTableAdvancedToolbar>
    </DataTable>
  );
}
