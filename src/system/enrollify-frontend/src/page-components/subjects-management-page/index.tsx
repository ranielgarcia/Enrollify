import { getAllRoomTypesOptions } from "@/api/collections/room-type-collection";
import { getAllSubjectsPaginatedOptions } from "@/api/collections/subject-collection";
import type { Subject } from "@/api/models/subject";
import { Button } from "@/components/ui/button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useSuspenseQuery } from "@tanstack/react-query";
import { useNavigate, useParams } from "@tanstack/react-router";
import { BookOpen, LayoutGrid, Link2, List } from "lucide-react";
import { useCallback, useState } from "react";
import { DeleteSubjectAlertDialog } from "./delete-subject-alert-dialog";
import { EquivalenceGroupsTab } from "./equivalence-groups";
import { SubjectFormDrawer } from "./subject-form-drawer";
import { SubjectsCardGrid } from "./subjects-card-grid";
import { SubjectsTable } from "./subjects-table";
import { useCrudState } from "@/hooks/use-crud-state";
import { ManagementPageLayout } from "@/components/management-page-layout";

type ViewMode = "table" | "grid";

export default function SubjectsManagementPage() {
  const { page, pageSize } = useParams({ strict: false });
  const navigate = useNavigate();

  const {
    isFormOpen,
    entityToEdit,
    entityToDelete,
    handleEdit,
    handleDelete,
    handleFormOpenChange,
    handleDeleteDialogOpenChange,
  } = useCrudState<Subject>();

  const [viewMode, setViewMode] = useState<ViewMode>("table");

  const [activeTab, setActiveTab] = useState(
    () => sessionStorage.getItem("subjects-tab") ?? "subjects",
  );

  const handleTabChange = (value: string) => {
    setActiveTab(value);
    sessionStorage.setItem("subjects-tab", value);
  };

  const currentPage = page ? Number(page) : 1;
  const currentPageSize = pageSize ? Number(pageSize) : 10;

  const { data: pagedSubjects } = useSuspenseQuery(
    getAllSubjectsPaginatedOptions(currentPage, currentPageSize),
  );

  const { data: roomTypes } = useSuspenseQuery(getAllRoomTypesOptions());

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

  return (
    <ManagementPageLayout
      title="Subject Management"
      description="Manage subjects and their equivalence relationships"
    >
      <Tabs
        value={activeTab}
        onValueChange={handleTabChange}
        className="space-y-6"
      >
        <TabsList>
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
        <TabsContent value="subjects" className="space-y-4">
          {/* Toolbar: View Toggle + Add Button */}
          <div className="flex items-center justify-between">
            {/* View Mode Toggle */}
            <div className="flex items-center gap-1 bg-muted p-1 rounded-lg">
              <Button
                variant={viewMode === "table" ? "default" : "ghost"}
                size="sm"
                onClick={() => setViewMode("table")}
                className="gap-2"
              >
                <List className="size-4" />
                Table
              </Button>
              <Button
                variant={viewMode === "grid" ? "default" : "ghost"}
                size="sm"
                onClick={() => setViewMode("grid")}
                className="gap-2"
              >
                <LayoutGrid className="size-4" />
                Grid
              </Button>
            </div>

            {/* Add Subject Button */}
            <SubjectFormDrawer
              roomTypes={roomTypes}
              onOpenChange={handleFormOpenChange}
              subjectToUpdate={entityToEdit}
              isOpen={isFormOpen}
              setIsOpen={handleFormOpenChange}
            />
          </div>

          {/* Subjects Display */}
          {viewMode === "table" ? (
            <SubjectsTable
              pagedSubjects={pagedSubjects}
              onEdit={handleEdit}
              onDelete={handleDelete}
              onPreviousPage={handlePreviousPage}
              onNextPage={handleNextPage}
            />
          ) : (
            <SubjectsCardGrid
              pagedSubjects={pagedSubjects}
              onEdit={handleEdit}
              onDelete={handleDelete}
              onPreviousPage={handlePreviousPage}
              onNextPage={handleNextPage}
            />
          )}
        </TabsContent>

        {/* Equivalence Groups Tab */}
        <TabsContent value="equivalence">
          <EquivalenceGroupsTab />
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
