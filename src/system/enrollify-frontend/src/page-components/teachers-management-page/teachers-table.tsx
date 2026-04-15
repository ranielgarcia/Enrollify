import type { Teacher } from "@/api/models/teacher";
import { DataTable } from "@/components/data-table";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  createColumnHelper,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  useReactTable,
} from "@tanstack/react-table";
import { Edit2, Trash2 } from "lucide-react";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";

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
  const columnHelper = createColumnHelper<Teacher>();

  const columns = [
    columnHelper.display({
      id: "profilePicture",
      header: "",
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
        header: "Full Name",
        cell: (info) => <span className="font-medium">{info.getValue()}</span>,
      },
    ),
    columnHelper.accessor("academicTitle", {
      header: "Academic Title",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("email", {
      header: "Email",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("phoneNumber", {
      header: "Phone",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor((row) => row.college.name, {
      id: "college",
      header: "College",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor((row) => row.department.name, {
      id: "department",
      header: "Department",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("specialization", {
      header: "Specialization",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("officeLocation", {
      header: "Office",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    columnHelper.accessor("subjects", {
      header: "Subjects",
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
      header: "Created At",
      cell: (info) => <span>{info.getValue()}</span>,
    }),
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
    data: teachers,
    columns,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
  });

  return <DataTable table={table} columns={columns} />;
}
