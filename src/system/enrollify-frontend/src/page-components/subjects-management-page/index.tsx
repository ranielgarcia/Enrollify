import { getAllRoomTypesOptions } from "@/api/collections/room-type-collection";
import { filterSubjectsPaginatedOptions } from "@/api/collections/subject-collection";
import type { Subject } from "@/api/models/subject";
import type { SubjectEquivalenceGroup } from "@/api/models/subject-equivalence";
import { Button } from "@/components/ui/button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useSuspenseQuery } from "@tanstack/react-query";
import { BookOpen, Link2, Plus } from "lucide-react";
import { useState } from "react";
import { DeleteSubjectAlertDialog } from "./delete-subject-alert-dialog";
import {
  DeleteEquivalenceGroupDialog,
  EquivalenceGroupFormDialog,
  EquivalenceGroupsTab,
} from "./equivalence-groups";
import { SubjectFormDrawer } from "./subject-form-drawer";
import { SubjectsTable } from "./subjects-table";
import { useCrudState } from "@/hooks/use-crud-state";
import { ManagementPageLayout } from "@/components/page-layouts/management-page-layout";
import { searchParams } from "./searchParams";
import { useQueryStates } from "nuqs";

export default function SubjectsManagementPage() {
  const [{ page, perPage, filters, sort, joinOperator }] =
    useQueryStates(searchParams);

  console.log(joinOperator);
  const {
    isFormOpen,
    entityToEdit,
    entityToDelete,
    handleEdit,
    handleDelete,
    handleFormOpenChange,
    handleDeleteDialogOpenChange,
  } = useCrudState<Subject>();

  const {
    isFormOpen: isEquivalenceFormOpen,
    entityToEdit: equivalenceToEdit,
    entityToDelete: equivalenceToDelete,
    handleEdit: handleEquivalenceEdit,
    handleDelete: handleEquivalenceDelete,
    handleFormOpenChange: handleEquivalenceFormOpenChange,
    handleDeleteDialogOpenChange: handleEquivalenceDeleteDialogOpenChange,
    openCreateForm: openEquivalenceCreateForm,
  } = useCrudState<SubjectEquivalenceGroup>();

  const [activeTab, setActiveTab] = useState(
    () => sessionStorage.getItem("subjects-tab") ?? "subjects",
  );

  const handleTabChange = (value: string) => {
    setActiveTab(value);
    sessionStorage.setItem("subjects-tab", value);
  };

  const currentPage = page ? Number(page) : 1;
  const currentPageSize = perPage ? Number(perPage) : 10;

  const { data: pagedSubjects } = useSuspenseQuery(
    filterSubjectsPaginatedOptions(
      currentPage,
      currentPageSize,
      filters,
      sort,
      joinOperator,
    ),
  );

  const { data: roomTypes } = useSuspenseQuery(getAllRoomTypesOptions());

  return (
    <ManagementPageLayout
      title="Subject Management"
      description="Manage subjects and their equivalence relationships"
      icon={<BookOpen />}
      createNewItemButton={
        activeTab === "subjects" ? (
          <SubjectFormDrawer
            roomTypes={roomTypes}
            onOpenChange={handleFormOpenChange}
            subjectToUpdate={entityToEdit}
            isOpen={isFormOpen}
            setIsOpen={handleFormOpenChange}
          />
        ) : (
          <Button onClick={openEquivalenceCreateForm}>
            <Plus className="size-4 mr-2" />
            New Equivalence Group
          </Button>
        )
      }
    >
      <Tabs
        value={activeTab}
        onValueChange={handleTabChange}
        className="min-h-0 flex-1 space-y-6"
      >
        <TabsList variant="line">
          <TabsTrigger value="subjects" className="gap-2">
            <BookOpen className="size-4" />
            Subjects
          </TabsTrigger>
          <TabsTrigger value="equivalence" className="gap-2">
            <Link2 className="size-4" />
            Equivalence Groups
          </TabsTrigger>
        </TabsList>

        {/* Subjects Tab */}
        <TabsContent
          value="subjects"
          className="flex min-h-0 flex-col space-y-4"
        >
          <SubjectsTable
            pagedSubjects={pagedSubjects}
            onEdit={handleEdit}
            onDelete={handleDelete}
          />
        </TabsContent>

        {/* Equivalence Groups Tab */}
        <TabsContent
          value="equivalence"
          className="flex min-h-0 flex-col space-y-4"
        >
          <EquivalenceGroupsTab
            onEdit={handleEquivalenceEdit}
            onDelete={handleEquivalenceDelete}
            onCreateNew={openEquivalenceCreateForm}
          />
          <EquivalenceGroupFormDialog
            isOpen={isEquivalenceFormOpen}
            onOpenChange={handleEquivalenceFormOpenChange}
            groupToEdit={equivalenceToEdit}
          />
          <DeleteEquivalenceGroupDialog
            isOpen={!!equivalenceToDelete}
            onOpenChange={handleEquivalenceDeleteDialogOpenChange}
            groupToDelete={equivalenceToDelete}
          />
        </TabsContent>
      </Tabs>

      <DeleteSubjectAlertDialog
        isOpen={!!entityToDelete}
        onOpenChange={handleDeleteDialogOpenChange}
        subjectToDelete={entityToDelete}
      />
    </ManagementPageLayout>
  );
}
