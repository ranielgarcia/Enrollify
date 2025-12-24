import createQueryOptions from "@/hooks/create-query-options";
import { BuildingSchema, type Building } from "../models/building";
import createMutationOptions from "@/hooks/create-mutation-options";
import { toast } from "sonner";

const queryKeys = {
  all: () => ["buildings"],
  create: () => [...queryKeys.all(), `create`],
  update: (buildingId: number) => [...queryKeys.all(), "update", buildingId],
  delete: (buildingId: number) => [...queryKeys.all(), "delete", buildingId],
};

export const getAllBuildingsOptions = () =>
  createQueryOptions({
    path: "/api/buildings",
    options: {
      queryKey: queryKeys.all(),
      select: (buildings): Building[] => {
        return buildings.map((t) => BuildingSchema.parse(t));
      },
    },
  });

// ** Create **
export const createBuildingOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/buildings",
    mutationKey: queryKeys.create(),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Building created successfully");
      },
    },
  });

// ** Update **
export const updateBuildingOptions = (buildingId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/buildings",
    params: {
      id: buildingId,
    },
    mutationKey: queryKeys.update(buildingId),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Building updated successfully");
      },
    },
  });

export const deleteBuildingOptions = (buildingId: number) =>
  createMutationOptions({
    httpVerb: "delete",
    path: "/api/buildings",
    mutationKey: queryKeys.delete(buildingId),
    params: {
      id: buildingId,
    },
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Building deleted successfully");
      },
    },
  });
