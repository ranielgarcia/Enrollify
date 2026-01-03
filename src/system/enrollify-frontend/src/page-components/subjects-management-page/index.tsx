import { getAllSubjectsPaginatedOptions } from "@/api/collections/subject-collection";
import type { Subject } from "@/api/models/subject";
import { useSuspenseQuery } from "@tanstack/react-query";
import { useParams } from "@tanstack/react-router";
import { useState } from "react";

export default function SubjectsManagementPage() {
  const { page, pageSize } = useParams({ strict: false });

  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [subjectToEdit, setSubjectToEdit] = useState<Subject | undefined>();
  const [subjectToDelete, setSubjectToDelete] = useState<Subject | undefined>();

  const currentPage = page ? Number(page) : 1;
  const currentPageSize = pageSize ? Number(pageSize) : 10;

  const { data: subjectsInCurrentPage } = useSuspenseQuery(
    getAllSubjectsPaginatedOptions(currentPage, currentPageSize)
  );

  return <h1>Subjects management page</h1>;
}
