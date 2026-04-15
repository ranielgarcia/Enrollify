import type { Teacher } from "@/api/models/teacher";
import { DUMMY_TEACHERS } from "@/api/models/teacher-dummy-data";
import { useState } from "react";
import { DeleteTeacherAlertDialog } from "./delete-teacher-alert-dialog";
import { TeacherFormDrawer } from "./teacher-form-drawer";
import { TeachersTable } from "./teachers-table";

export default function TeachersManagementPage() {
  const [teachers, setTeachers] = useState<Teacher[]>(DUMMY_TEACHERS);
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [teacherToEdit, setTeacherToEdit] = useState<Teacher | undefined>();
  const [teacherToDelete, setTeacherToDelete] = useState<Teacher | undefined>();

  const handleEdit = (teacher: Teacher) => {
    setTeacherToEdit(teacher);
    setIsFormOpen(true);
  };

  const handleDelete = (teacher: Teacher) => {
    setTeacherToDelete(teacher);
  };

  const handleDrawerOnOpenChange = (open: boolean) => {
    if (!open) {
      setTeacherToEdit(undefined);
    }
    setIsFormOpen(open);
  };

  const handleDeleteAlertDialogOnOpenChange = (open: boolean) => {
    if (!open) {
      setTeacherToDelete(undefined);
    }
  };

  const handleFormSubmit = (
    data: Parameters<
      React.ComponentProps<typeof TeacherFormDrawer>["onSubmit"]
    >[0],
  ) => {
    if (teacherToEdit) {
      const college = {
        id: Number(data.collegeId),
        name:
          [
            { id: "1", name: "College of Engineering" },
            { id: "2", name: "College of Science" },
            { id: "3", name: "College of Arts and Sciences" },
            { id: "4", name: "College of Business Administration" },
            { id: "5", name: "College of Education" },
          ].find((c) => c.id === data.collegeId)?.name ?? "",
      };
      const department = {
        id: Number(data.departmentId),
        name:
          [
            { id: "1", name: "Computer Engineering" },
            { id: "2", name: "Mathematics" },
            { id: "3", name: "Physics" },
            { id: "4", name: "Electronics Engineering" },
            { id: "5", name: "Chemistry" },
            { id: "6", name: "Management" },
            { id: "7", name: "Accountancy" },
          ].find((d) => d.id === data.departmentId)?.name ?? "",
      };
      setTeachers((prev) =>
        prev.map((t) =>
          t.id === teacherToEdit.id
            ? {
                ...t,
                ...data,
                college,
                department,
                subjects: data.subjectIds.map((id) => ({
                  id: Number(id),
                  code: `SUBJ${id}`,
                  title: `Subject ${id}`,
                })),
              }
            : t,
        ),
      );
    } else {
      const newId = Math.max(...teachers.map((t) => t.id)) + 1;
      const college = {
        id: Number(data.collegeId),
        name:
          [
            { id: "1", name: "College of Engineering" },
            { id: "2", name: "College of Science" },
            { id: "3", name: "College of Arts and Sciences" },
            { id: "4", name: "College of Business Administration" },
            { id: "5", name: "College of Education" },
          ].find((c) => c.id === data.collegeId)?.name ?? "",
      };
      const department = {
        id: Number(data.departmentId),
        name:
          [
            { id: "1", name: "Computer Engineering" },
            { id: "2", name: "Mathematics" },
            { id: "3", name: "Physics" },
            { id: "4", name: "Electronics Engineering" },
            { id: "5", name: "Chemistry" },
            { id: "6", name: "Management" },
            { id: "7", name: "Accountancy" },
          ].find((d) => d.id === data.departmentId)?.name ?? "",
      };
      setTeachers((prev) => [
        ...prev,
        {
          id: newId,
          ...data,
          college,
          department,
          subjects: data.subjectIds.map((id) => ({
            id: Number(id),
            code: `SUBJ${id}`,
            title: `Subject ${id}`,
          })),
          createdAt: new Date().toISOString().split("T")[0],
        },
      ]);
    }
  };

  const handleConfirmDelete = (teacher: Teacher) => {
    setTeachers((prev) => prev.filter((t) => t.id !== teacher.id));
  };

  return (
    <main>
      <div className="p-4 md:p-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-foreground mb-2">
            Teachers Management
          </h1>
          <p className="text-muted-foreground">
            Manage faculty members and their academic profiles
          </p>
        </div>

        <div className="flex justify-end mb-4">
          <TeacherFormDrawer
            onOpenChange={handleDrawerOnOpenChange}
            teacherToUpdate={teacherToEdit}
            isOpen={isFormOpen}
            setIsOpen={setIsFormOpen}
            onSubmit={handleFormSubmit}
          />
        </div>

        <TeachersTable
          teachers={teachers}
          onEdit={handleEdit}
          onDelete={handleDelete}
        />

        <DeleteTeacherAlertDialog
          isOpen={!!teacherToDelete}
          onOpenChange={handleDeleteAlertDialogOnOpenChange}
          teacherToDelete={teacherToDelete}
          onConfirmDelete={handleConfirmDelete}
        />
      </div>
    </main>
  );
}
