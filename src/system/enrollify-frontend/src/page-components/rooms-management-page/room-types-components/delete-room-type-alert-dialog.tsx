import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import type { RoomType } from "../models/RoomType";
import { OverlayLoader } from "@/components/app-loading-overlay";
import useAppQuery from "@/hooks/use-app-query-v2";
import useAppMutation from "@/hooks/use-app-mutation-v2";
import { toast } from "sonner";
import {
  formatValidationErrors,
  parseApiError,
  type ProblemDetails,
} from "@/lib/axios-utils";
import type { AxiosError } from "axios";

interface DeleteRoomTypeAlertDialogProps {
  roomTypeToDelete?: RoomType;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccessful: () => void;
}

export function DeleteRoomTypeAlertDialog({
  roomTypeToDelete,
  isOpen,
  onOpenChange,
  onSuccessful,
}: DeleteRoomTypeAlertDialogProps) {
  const { data: associatedRooms, isPending: isLoadingAssociatedRooms } =
    useAppQuery({
      path: "/api/rooms/list-by-room-type",
      params: {
        roomTypeId: roomTypeToDelete?.id ?? 0,
      },
      queryOptions: {
        queryKey: ["/api/rooms/list-by-room-type", roomTypeToDelete?.id],
        enabled: !!roomTypeToDelete,
      },
    });

  const { mutateAsync: deleteRoomType, isPending: isDeletingInProgress } =
    useAppMutation({
      httpVerb: "delete",
      path: "/api/room-types",
      mutationKey: `delete-room-type-${roomTypeToDelete?.id}`,
      params: {
        id: roomTypeToDelete?.id ?? 0,
      },
    });

  const handleContinueDelete = async () => {
    try {
      await deleteRoomType(undefined);
      toast.success("Room type deleted successfully");
      onSuccessful();
    } catch (err) {
      const axiosError = err as AxiosError<ProblemDetails>;
      const parsed = parseApiError(axiosError);

      // Show error toast with title and detail
      toast.error(parsed.title, {
        description: parsed.validationErrors
          ? formatValidationErrors(parsed.validationErrors)
          : parsed.detail || "Please try again.",
      });
    }
  };

  return (
    <AlertDialog onOpenChange={onOpenChange} open={isOpen}>
      <AlertDialogContent>
        <OverlayLoader
          isLoading={isLoadingAssociatedRooms || isDeletingInProgress}
          text={isDeletingInProgress ? "Deleting..." : "Processing.."}
          size="sm"
        />
        <AlertDialogHeader>
          {associatedRooms && associatedRooms.length > 0 ? (
            <AlertDialogTitle className="text-danger">
              Unable to Delete Room Type
            </AlertDialogTitle>
          ) : (
            <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
          )}

          {associatedRooms && associatedRooms.length > 0 ? (
            <AlertDialogDescription>
              This room type cannot be deleted because it has{" "}
              <strong>{associatedRooms.length}</strong> room(s) associated with
              it. Please reassign or remove these rooms from this room type
              before deleting.
            </AlertDialogDescription>
          ) : (
            <AlertDialogDescription>
              Deleting room type <strong>{roomTypeToDelete?.name}</strong>{" "}
              <br />
              This action cannot be undone. This will permanently delete your
              room type and remove your data from our servers.
            </AlertDialogDescription>
          )}
        </AlertDialogHeader>
        <AlertDialogFooter>
          {associatedRooms && associatedRooms.length > 0 ? (
            <>
              <AlertDialogCancel onClick={() => openOpenChange(false)}>
                Close
              </AlertDialogCancel>
            </>
          ) : (
            <>
              <AlertDialogCancel onClick={() => openOpenChange(false)}>
                Cancel
              </AlertDialogCancel>
              <AlertDialogAction
                onClick={async () => await handleContinueDelete()}
              >
                Continue
              </AlertDialogAction>
            </>
          )}
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
