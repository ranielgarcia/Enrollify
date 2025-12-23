import { OverlayLoader } from "@/components/app-loading-overlay";
import { CollegesTable } from "./colleges-table";
import { useState } from "react";
import { CollegeFormDrawer } from "./college-form-drawer";
import { DeleteCollegeAlertDialog } from "./delete-college-alert-dialog";
import { useGetAllColleges } from "@/api/collections/college-collection";
import type { College } from "@/api/models/college";

export default function CollegesPage() {
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [collegeToEdit, setCollegeToEdit] = useState<College | undefined>();
  const [collegeToDelete, setCollegeToDelete] = useState<College | undefined>();
  const {
    data: colleges,
    refetch: refetchColleges,
    isPending: isLoadingColleges,
  } = useGetAllColleges();

  const handleEdit = (college: College) => {
    setCollegeToEdit(college);
    setIsFormOpen(true);
  };

  const handleDelete = (college: College) => {
    setCollegeToDelete(college);
  };

  const handleDrawerOnOpenChange = (open: boolean) => {
    setCollegeToEdit(undefined);
    setIsFormOpen(open);
  };

  const handleDeleteCollegeAlertDialogOnOpenChange = (open: boolean) => {
    if (!open) {
      setCollegeToDelete(undefined);
    }
  };

  return (
    <main>
      <OverlayLoader isLoading={isLoadingColleges} text="Loading" size="sm" />

      <div className="p-4 md:p-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-foreground mb-2">
            College Management
          </h1>
          <p className="text-muted-foreground">Manage college resources</p>
        </div>

        <div className="flex justify-end">
          <CollegeFormDrawer
            onOpenChange={handleDrawerOnOpenChange}
            collegeToUpdate={collegeToEdit}
            isOpen={isFormOpen}
            setIsOpen={setIsFormOpen}
            onSuccessful={() => refetchColleges()}
          />
        </div>

        <CollegesTable
          colleges={colleges}
          onEdit={handleEdit}
          onDelete={handleDelete}
        />

        <DeleteCollegeAlertDialog
          isOpen={!!collegeToDelete}
          onOpenChange={handleDeleteCollegeAlertDialogOnOpenChange}
          collegeToDelete={collegeToDelete}
          onSuccessful={() => refetchColleges()}
        />
      </div>
    </main>
  );
}
