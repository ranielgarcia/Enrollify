import type { Teacher } from "@/api/models/teacher";
import { DataTable } from "@/components/data-table/data-table";
import { DataTableColumnHeader } from "@/components/data-table/data-table-column-header";
import { DataTableToolbar } from "@/components/data-table/data-table-toolbar";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { useDataTable } from "@/hooks/use-data-table";
import { createColumnHelper } from "@tanstack/react-table";
import { Edit2, Trash2 } from "lucide-react";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { useMemo } from "react";
import { useTablePermissions } from "@/hooks/use-table-permissions";

interface TeachersTableProps {
  teachers: Teacher[];
  onEdit: (teacher: Teacher) => void;
  onDelete: (teacher: Teacher) => void;
}

export function TeachersTable({
  teachers,
  onEdit,
  onDelete,
}: TeachersTableProps) {
  const { canUpdate, canDelete } = useTablePermissions("canUpdateTeacher", "canDeleteTeacher");
  const columnHelper = createColumnHelper<Teacher>();

  const columns = useMemo(() => [
    columnHelper.display({
      id: "profilePicture",
      header: "",
      enableHiding: false,
      cell: (info) => {
        const teacher = info.row.original;
        const initials = `${teacher.firstName[0]}${teacher.lastName[0]}`;
        return (
          <Avatar className="size-9">
            <AvatarImage src={teacher.profilePicture} alt={initials} />
            <AvatarFallback className="text-xs font-semibold">
              {initials}
            </AvatarFallback>
          </Avatar>
        );
      },
    }),
    columnHelper.accessor(
      (row) => `${row.firstName} ${row.middleName ? row.middleName + " " : ""}${row.lastName}`,
      {
        id: "fullName",
        header: ({ column }) => <DataTableColumnHeader column={column} label="Full Name" />,
        meta: { label: "Full Name" },
        cell: (info) => <span className="font-medium">{info.getValue()}</span>,
      },
    ),
    columnHelper.accessor("academicTitle", {
      header: ({ column }) => <DataTableColumnHeader column={column} label="Academic Title" />,
      meta: { label: "Academic Title" },
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("email", {
      header: ({ column }) => <DataTableColumnHeader column={column} label="Email" />,
      meta: { label: "Email" },
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("phoneNumber", {
      header: ({ column }) => <DataTableColumnHeader column={column} label="Phone" />,
      meta: { label: "Phone" },
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor((row) => row.college.name, {
      id: "college",
      header: ({ column }) => <DataTableColumnHeader column={column} label="College" />,
      meta: { label: "College" },
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor((row) => row.department.name, {
      id: "department",
      header: ({ column }) => <DataTableColumnHeader column={column} label="Department" />,
      meta: { label: "Department" },
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("specialization", {
      header: ({ column }) => <DataTableColumnHeader column={column} label="Specialization" />,
      meta: { label: "Specialization" },
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("officeLocation", {
      header: ({ column }) => <DataTableColumnHeader column={column} label="Office" />,
      meta: { label: "Office" },
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("subjects", {
      header: ({ column }) => <DataTableColumnHeader column={column} label="Subjects" />,
      meta: { label: "Subjects" },
      enableSorting: false,
      cell: (info) => {
        const subjects = info.getValue();
        return (
          <div className="flex flex-wrap gap-1">
            {subjects.slice(0, 2).map((s) => (
              <Badge key={s.id} variant="secondary" className="text-xs">
                {s.code}
              </Badge>
            ))}
            {subjects.length > 2 && (
              <Badge variant="outline" className="text-xs">
                +{subjects.length - 2} more
              </Badge>
            )}
          </div>
        );
      },
    }),
    columnHelper.accessor("createdAt", {
      header: ({ column }) => <DataTableColumnHeader column={column} label="Created At" />,
      meta: { label: "Created At" },
      cell: (info) => <span>{info.getValue()}</span>,
    }),
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
  ], [canUpdate, canDelete, onEdit, onDelete]);

  const { table } = useDataTable({
    data: teachers,
    columns,
  });

  return (
    <DataTable table={table}>
      <DataTableToolbar table={table} />
    </DataTable>
  );
}
