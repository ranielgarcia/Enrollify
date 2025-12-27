import { getAllCoursesOptions } from "@/api/collections/course-collection";
import type { Course } from "@/api/models/course";
import { useSuspenseQuery } from "@tanstack/react-query";
import { useState } from "react";
import { CourseFormDrawer } from "./course-form-drawer";
import { getAllCollegesOptions } from "@/api/collections/college-collection";
import { getAllRoomTypesOptions } from "@/api/collections/room-type-collection";
import { CoursesTable } from "./courses-table";
import { DeleteCourseAlertDialog } from "./delete-course-alert-dialog";

export default function CoursesManagementPage() {
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [courseToEdit, setCourseToEdit] = useState<Course | undefined>();
  const [courseToDelete, setCourseToDelete] = useState<Course | undefined>();

  const { data: courses } = useSuspenseQuery(getAllCoursesOptions());

  const { data: colleges } = useSuspenseQuery(getAllCollegesOptions());
  const { data: roomTypes } = useSuspenseQuery(getAllRoomTypesOptions());

  const handleEdit = (course: Course) => {
    setCourseToEdit(course);
    setIsFormOpen(true);
  };

  const handleDelete = (course: Course) => {
    setCourseToDelete(course);
  };

  const handleDrawerOnOpenChange = (open: boolean) => {
    setCourseToEdit(undefined);
    setIsFormOpen(open);
  };

  const handleDeleteCourseAlertDialogOnOpenChange = (open: boolean) => {
    if (!open) {
      setCourseToDelete(undefined);
    }
  };

  return (
    <main>
      <div className="p-4 md:p-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-foreground mb-2">
            Course Management
          </h1>
          <p className="text-muted-foreground">Manage course resources</p>
        </div>

        <div className="flex justify-end">
          <CourseFormDrawer
            colleges={colleges}
            roomTypes={roomTypes}
            onOpenChange={handleDrawerOnOpenChange}
            courseToUpdate={courseToEdit}
            isOpen={isFormOpen}
            setIsOpen={setIsFormOpen}
          />
        </div>

        <CoursesTable
          courses={courses}
          onEdit={handleEdit}
          onDelete={handleDelete}
        />

        <DeleteCourseAlertDialog
          isOpen={!!courseToDelete}
          onOpenChange={handleDeleteCourseAlertDialogOnOpenChange}
          courseToDelete={courseToDelete}
        />
      </div>
    </main>
  );
}
