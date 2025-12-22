import createMutationOptions from "@/hooks/create-mutation-options";
import createAppQueryOptions from "@/hooks/create-query-options";
import {
  QueryClient,
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";
import { toast } from "sonner";

const queryKeys = {
  all: () => ["room-types"],
  create: () => [...queryKeys.all(), `create`],
  delete: (roomTypeId?: number) => [
    ...queryKeys.all(),
    `delete-room-type-${roomTypeId}`,
  ],
};

export const getAllRoomTypesOptions = () =>
  createAppQueryOptions({
    path: "/api/room-types",
    options: {
      queryKey: queryKeys.all(),
    },
  });

export const useGetAllRoomTypes = () => useQuery(getAllRoomTypesOptions());

export const deleteRoomTypeOptions = (
  roomTypeId: number,
  queryClient: QueryClient
) =>
  createMutationOptions({
    httpVerb: "delete",
    path: "/api/room-types",
    mutationKey: queryKeys.delete(roomTypeId),
    params: {
      id: roomTypeId,
    },
    options: {
      onSettled: () =>
        queryClient.invalidateQueries({ queryKey: queryKeys.all() }),
      onSuccess: () => {
        toast.success("Room type is deleted successfully");
      },
    },
  });

export const useDeleteRoomType = (roomTypeId: number) => {
  const queryClient = useQueryClient();
  return useMutation(deleteRoomTypeOptions(roomTypeId, queryClient));
};
