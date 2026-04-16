import { CollegesTable } from "./colleges-table";
import { CollegeFormDrawer } from "./college-form-drawer";
import { DeleteCollegeAlertDialog } from "./delete-college-alert-dialog";
import type { College } from "@/api/models/college";
import { useSuspenseQuery } from "@tanstack/react-query";
import { getAllCollegesOptions } from "@/api/collections/college-collection";
import { useCrudState } from "@/hooks/use-crud-state";
import { ManagementPageLayout } from "@/components/management-page-layout";

export default function CollegesPage() {
  const {
    isFormOpen,
    entityToEdit,
    entityToDelete,
    handleEdit,
    handleDelete,
    handleFormOpenChange,
    handleDeleteDialogOpenChange,
  } = useCrudState<College>();

  const { data: colleges, isPending: isLoadingColleges } = useSuspenseQuery(
    getAllCollegesOptions()
  );

  return (
    <ManagementPageLayout
      title="College Management"
      description="Manage college resources"
      isLoading={isLoadingColleges}
    >
      <div className="flex justify-end">
        <CollegeFormDrawer
          onOpenChange={handleFormOpenChange}
          collegeToUpdate={entityToEdit}
          isOpen={isFormOpen}
          setIsOpen={handleFormOpenChange}
        />
      </div>

      <CollegesTable
        colleges={colleges}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />

      <DeleteCollegeAlertDialog
        isOpen={!!entityToDelete}
        onOpenChange={handleDeleteDialogOpenChange}
        collegeToDelete={entityToDelete}
      />
    </ManagementPageLayout>
  );
}
