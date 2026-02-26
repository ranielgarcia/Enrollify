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

type ViewMode = "table" | "grid";

export default function SubjectsManagementPage() {
  const { page, pageSize } = useParams({ strict: false });
  const navigate = useNavigate();

  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [subjectToEdit, setSubjectToEdit] = useState<Subject | undefined>();
  const [subjectToDelete, setSubjectToDelete] = useState<Subject | undefined>();
  const [viewMode, setViewMode] = useState<ViewMode>("table");

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
          <p className="text-muted-foreground">
            Manage subjects and their equivalence relationships
          </p>
        </div>

        <Tabs defaultValue="subjects" className="space-y-6">
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
                onOpenChange={handleDrawerOnOpenChange}
                subjectToUpdate={subjectToEdit}
                isOpen={isFormOpen}
                setIsOpen={setIsFormOpen}
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
          isOpen={!!subjectToDelete}
          onOpenChange={handleDeleteSubjectAlertDialogOnOpenChange}
          subjectToDelete={subjectToDelete}
        />
      </div>
    </main>
  );
}
