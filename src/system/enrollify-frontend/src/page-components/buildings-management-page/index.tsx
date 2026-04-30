import { getAllBuildingsOptions } from "@/api/collections/building-collection";
import { useSuspenseQuery } from "@tanstack/react-query";
import { BuildingsTable } from "./buildings-table";
import type { Building } from "@/api/models/building";
import { BuildingFormDrawer } from "./building-form-drawer";
import { DeleteBuildingAlertDialog } from "./delete-building-alert-dialog";
import { getAllCollegesOptions } from "@/api/collections/college-collection";
import { useCrudState } from "@/hooks/use-crud-state";
import { ManagementPageLayout } from "@/components/page-layouts/management-page-layout";
import { ModuleIcons } from "@/config/module-icons";

export default function BuildingPage() {
  const {
    isFormOpen,
    entityToEdit,
    entityToDelete,
    handleEdit,
    handleDelete,
    handleFormOpenChange,
    handleDeleteDialogOpenChange,
  } = useCrudState<Building>();

  const { data: buildings, isPending: isLoadingBuildings } = useSuspenseQuery(
    getAllBuildingsOptions(),
  );

  const { data: colleges } = useSuspenseQuery(getAllCollegesOptions());

  return (
    <ManagementPageLayout
      title="Building Management"
      description="Manage building resources"
      isLoading={isLoadingBuildings}
      createNewItemButton={
        <BuildingFormDrawer
          colleges={colleges}
          onOpenChange={handleFormOpenChange}
          buildingToUpdate={entityToEdit}
          isOpen={isFormOpen}
          setIsOpen={handleFormOpenChange}
        />
      }
      icon={<ModuleIcons.buildings />}
    >
      <BuildingsTable
        buildings={buildings}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />

      <DeleteBuildingAlertDialog
        isOpen={!!entityToDelete}
        onOpenChange={handleDeleteDialogOpenChange}
        buildingToDelete={entityToDelete}
      />
    </ManagementPageLayout>
  );
}
