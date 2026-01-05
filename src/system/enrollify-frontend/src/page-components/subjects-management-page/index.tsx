import { getAllRoomTypesOptions } from "@/api/collections/room-type-collection";
import { getAllSubjectsPaginatedOptions } from "@/api/collections/subject-collection";
import type { Subject } from "@/api/models/subject";
import { useSuspenseQuery } from "@tanstack/react-query";
import { useNavigate, useParams } from "@tanstack/react-router";
import { useCallback, useState } from "react";
import { SubjectsTable } from "./subjects-table";
import { SubjectFormDrawer } from "./subject-form-drawer";
import { DeleteSubjectAlertDialog } from "./delete-subject-alert-dialog";

export default function SubjectsManagementPage() {
  const { page, pageSize } = useParams({ strict: false });
  const navigate = useNavigate();

  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [subjectToEdit, setSubjectToEdit] = useState<Subject | undefined>();
  const [subjectToDelete, setSubjectToDelete] = useState<Subject | undefined>();

  const currentPage = page ? Number(page) : 1;
  const currentPageSize = pageSize ? Number(pageSize) : 10;

  const { data: pagedSubjects } = useSuspenseQuery(
    getAllSubjectsPaginatedOptions(currentPage, currentPageSize)
  );

  const { data: roomTypes } = useSuspenseQuery(getAllRoomTypesOptions());

  console.log(pagedSubjects);
  console.log(roomTypes);

  const handlePreviousPage = useCallback(() => {
    if (currentPage > 1) {
      navigate({
        to: "/portal/master-data/subjects/{-$page}/{-$pageSize}",
        params: {
          page: String(currentPage - 1),
          pageSize: String(currentPageSize),
        },
      });
    }
  }, [currentPage, currentPageSize, navigate]);

  const handleNextPage = useCallback(() => {
    if (pagedSubjects && currentPage < pagedSubjects.totalPages) {
      navigate({
        to: "/portal/master-data/subjects/{-$page}/{-$pageSize}",
        params: {
          page: String(currentPage + 1),
          pageSize: String(currentPageSize),
        },
      });
    }
  }, [currentPage, currentPageSize, navigate, pagedSubjects]);

  const handleEdit = (subject: Subject) => {
    setSubjectToEdit(subject);
    setIsFormOpen(true);
  };

  const handleDelete = (subject: Subject) => {
    setSubjectToDelete(subject);
  };

  const handleDrawerOnOpenChange = (open: boolean) => {
    setSubjectToEdit(undefined);
    setIsFormOpen(open);
  };

  const handleDeleteSubjectAlertDialogOnOpenChange = (open: boolean) => {
    if (!open) {
      setSubjectToDelete(undefined);
    }
  };

  return (
    <main>
      <div className="p-4 md:p-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-foreground mb-2">
            Subject Management
          </h1>
          <p className="text-muted-foreground">Manage subjects resources</p>
        </div>

        <div className="flex justify-end">
          <SubjectFormDrawer
            roomTypes={roomTypes}
            onOpenChange={handleDrawerOnOpenChange}
            subjectToUpdate={subjectToEdit}
            isOpen={isFormOpen}
            setIsOpen={setIsFormOpen}
          />
        </div>

        <SubjectsTable
          pagedSubjects={pagedSubjects}
          onEdit={handleEdit}
          onDelete={handleDelete}
          onPreviousPage={handlePreviousPage}
          onNextPage={handleNextPage}
        />

        <DeleteSubjectAlertDialog
          isOpen={!!subjectToDelete}
          onOpenChange={handleDeleteSubjectAlertDialogOnOpenChange}
          subjectToDelete={subjectToDelete}
        />
      </div>
    </main>
  );
}
