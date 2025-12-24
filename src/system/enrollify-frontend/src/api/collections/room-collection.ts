import createAppQueryOptions from "@/hooks/create-query-options";
import { RoomSchema, type Room } from "../models/room";

const queryKeys = {
  all: () => ["rooms"],
  listByRoomType: (roomTypeId: number) => [
    ...queryKeys.all(),
    "by-room-type",
    roomTypeId,
  ],
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
      select: (roomTypes): Room[] => {
        return roomTypes.map((t) => RoomSchema.parse(t));
      },
    },
  });
