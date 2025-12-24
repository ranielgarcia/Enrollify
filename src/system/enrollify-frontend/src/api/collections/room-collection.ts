// import createAppQueryOptions from "@/hooks/create-query-options";

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

// // ** fetch all rooms by room type **
// export const getAllRoomsByRoomTypeOptions = (roomTypeId: number) =>
//   createAppQueryOptions({
//     path: "/api/rooms/list-by-room-type",
//     params: {
//       roomTypeId: roomTypeId,
//     },
//     options: {
//       queryKey: queryKeys.listByRoomType(roomTypeId),
//       enabled: !!roomTypeId,
//     },
//   });
