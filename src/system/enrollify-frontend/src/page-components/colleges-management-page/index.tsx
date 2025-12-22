import { OverlayLoader } from "@/components/app-loading-overlay";
import useAppQuery from "@/hooks/use-app-query-v2";
import { CollegesTable } from "./colleges-table";
import type { College } from "./models/College";
import { parseDateTime } from "@/lib/dateutils";
import type BasicUserInfo from "@/api/models/BasicUserInfo";
import { useState } from "react";
import { CollegeFormDrawer } from "./college-form-drawer";
import { DeleteCollegeAlertDialog } from "./delete-college-alert-dialog";

export default function CollegesPage() {
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [collegeToEdit, setCollegeToEdit] = useState<College | undefined>();
  const [collegeToDelete, setCollegeToDelete] = useState<College | undefined>();
  const {
    data: collegesData,
    refetch: refetchColleges,
    isPending: isLoadingColleges,
  } = useAppQuery({
    path: "/api/colleges",
    queryOptions: {
      queryKey: ["/api/colleges"],
    },
  });

  console.log(collegeToEdit);
  console.log(collegeToDelete);

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

  const colleges: College[] = collegesData
    ? collegesData.map(
        (c) =>
          ({
            id: c.id,
            code: c.code,
            name: c.name,
            description: c.description,
            dean: c.dean,
            createdAt: parseDateTime(c.createdAt),
            createdBy: {
              ...c.createdByUser,
            } as BasicUserInfo,
            updatedAt: parseDateTime(c.updatedAt),
            updatedBy: {
              ...c.updatedByUser,
            } as BasicUserInfo,
            deletedAt: parseDateTime(c.deletedAt),
            deletedBy: {
              ...c.deletedByUser,
            } as BasicUserInfo,
            isActive: c.isActive,
          }) as College
      )
    : [];

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
