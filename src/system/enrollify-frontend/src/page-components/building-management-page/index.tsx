import { getAllBuildingsOptions } from "@/api/collections/building-collection";
import { OverlayLoader } from "@/components/app-loading-overlay";
import { useSuspenseQuery } from "@tanstack/react-query";
import { BuildingsTable } from "./buildings-table";
import { useState } from "react";
import type { Building } from "@/api/models/building";
import { BuildingFormDrawer } from "./building-form-drawer";
import { DeleteBuildingAlertDialog } from "./delete-building-alert-dialog";

export default function BuildingPage() {
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [buildingToEdit, setBuildingToEdit] = useState<Building | undefined>();
  const [buildingToDelete, setBuildingToDelete] = useState<
    Building | undefined
  >();
  const { data: buildings, isPending: isLoadingBuildings } = useSuspenseQuery(
    getAllBuildingsOptions()
  );

  const handleEdit = (building: Building) => {
    setBuildingToEdit(building);
    setIsFormOpen(true);
  };

  const handleDelete = (building: Building) => {
    setBuildingToDelete(building);
  };

  const handleDrawerOnOpenChange = (open: boolean) => {
    setBuildingToEdit(undefined);
    setIsFormOpen(open);
  };

  const handleDeleteBuildingAlertDialogOnOpenChange = (open: boolean) => {
    if (!open) {
      setBuildingToDelete(undefined);
    }
  };

  return (
    <main>
      <OverlayLoader isLoading={isLoadingBuildings} text="Loading" size="sm" />

      <div className="p-4 md:p-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-foreground mb-2">
            Building Management
          </h1>
          <p className="text-muted-foreground">Manage building resources</p>
        </div>

        <div className="flex justify-end">
          <BuildingFormDrawer
            onOpenChange={handleDrawerOnOpenChange}
            buildingToUpdate={buildingToEdit}
            isOpen={isFormOpen}
            setIsOpen={setIsFormOpen}
          />
        </div>

        <BuildingsTable
          buildings={buildings}
          onEdit={handleEdit}
          onDelete={handleDelete}
        />

        <DeleteBuildingAlertDialog
          isOpen={!!buildingToDelete}
          onOpenChange={handleDeleteBuildingAlertDialogOnOpenChange}
          buildingToDelete={buildingToDelete}
        />
      </div>
    </main>
  );
}
