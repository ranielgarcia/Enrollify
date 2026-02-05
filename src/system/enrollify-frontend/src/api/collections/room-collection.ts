import createAppQueryOptions from "@/hooks/create-query-options";
import { RoomSchema, type Room } from "../models/room";
import createMutationOptions from "@/hooks/create-mutation-options";
import { toast } from "sonner";

const queryKeys = {
  all: () => ["rooms"],
  create: () => [...queryKeys.all(), `create`],
  update: (roomId: number) => [...queryKeys.all(), "update", roomId],
  delete: (roomId: number) => [...queryKeys.all(), "delete", roomId],
};

// ** fetch all rooms by room type **
export const getAllRooms = () =>
  createAppQueryOptions({
    path: "/api/rooms",
    options: {
      queryKey: queryKeys.all(),
      select: (rooms): Room[] => {
        return rooms.map((t) => RoomSchema.parse(t));
      },
    },
  });

export const createRoomOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/rooms",
    mutationKey: queryKeys.create(),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Room create successfully");
      },
    },
  });

export const UpdateRoomOptions = (roomId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/rooms",
    params: {
      id: roomId,
    },
    mutationKey: queryKeys.update(roomId),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Room updated successfully");
      },
    },
  });

export const DeleteRoomOptions = (roomId: number) =>
  createMutationOptions({
    httpVerb: "delete",
    path: "/api/rooms",
    params: {
      id: roomId,
    },
    mutationKey: queryKeys.delete(roomId),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Room deleted successfully");
      },
    },
  });
