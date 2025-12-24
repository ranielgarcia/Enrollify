import createMutationOptions from "@/hooks/create-mutation-options";
import createAppQueryOptions from "@/hooks/create-query-options";
import { toast } from "sonner";
import { RoomTypeSchema, type RoomType } from "@/api/models/room-type";

const queryKeys = {
  all: () => ["room-types"],
  create: () => [...queryKeys.all(), `create`],
  update: (roomTypeId: number) => [...queryKeys.all(), "update", roomTypeId],
  delete: (roomTypeId: number) => [...queryKeys.all(), "delete", roomTypeId],
};

// ** fetch all room types **
export const getAllRoomTypesOptions = () =>
  createAppQueryOptions({
    path: "/api/room-types",
    options: {
      queryKey: queryKeys.all(),
      select: (roomTypes): RoomType[] => {
        return roomTypes.map((t) => RoomTypeSchema.parse(t));
      },
    },
  });

// ** Create new room type **
export const createRoomTypeOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/room-types",
    mutationKey: queryKeys.create(),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Room type created successfully");
      },
    },
  });

// ** Update room type **

export const updateRoomTypeOptions = (roomTypeId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/room-types",
    mutationKey: queryKeys.update(roomTypeId),
    params: {
      id: roomTypeId,
    },
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Room type updated successfully");
      },
    },
  });

// ** Delete room type **
export const deleteRoomTypeOptions = (roomTypeId: number) =>
  createMutationOptions({
    httpVerb: "delete",
    path: "/api/room-types",
    mutationKey: queryKeys.delete(roomTypeId),
    params: {
      id: roomTypeId,
    },
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Room type is deleted successfully");
      },
    },
  });
