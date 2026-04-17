import { getAllCoursesOptions } from "@/api/collections/course-collection";
import type { Course } from "@/api/models/course";
import { useSuspenseQuery } from "@tanstack/react-query";
import { CourseFormDrawer } from "./course-form-drawer";
import { getAllCollegesOptions } from "@/api/collections/college-collection";
import { CoursesTable } from "./courses-table";
import { DeleteCourseAlertDialog } from "./delete-course-alert-dialog";
import { useCrudState } from "@/hooks/use-crud-state";
import { ManagementPageLayout } from "@/components/management-page-layout";
import { GraduationCap } from "lucide-react";

export default function CoursesManagementPage() {
  const {
    isFormOpen,
    entityToEdit,
    entityToDelete,
    handleEdit,
    handleDelete,
    handleFormOpenChange,
    handleDeleteDialogOpenChange,
  } = useCrudState<Course>();

  const { data: courses } = useSuspenseQuery(getAllCoursesOptions());

  const { data: colleges } = useSuspenseQuery(getAllCollegesOptions());

  return (
    <ManagementPageLayout
      title="Course Management"
      description="Manage course resources"
      createNewItemButton={
        <CourseFormDrawer
          colleges={colleges}
          onOpenChange={handleFormOpenChange}
          courseToUpdate={entityToEdit}
          isOpen={isFormOpen}
          setIsOpen={handleFormOpenChange}
        />
      }
      icon={<GraduationCap />}
    >
      <CoursesTable
        courses={courses}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />

      <DeleteCourseAlertDialog
        isOpen={!!entityToDelete}
        onOpenChange={handleDeleteDialogOpenChange}
        courseToDelete={entityToDelete}
      />
    </ManagementPageLayout>
  );
}
