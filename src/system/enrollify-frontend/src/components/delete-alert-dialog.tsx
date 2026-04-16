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
import { OverlayLoader } from "@/components/app-loading-overlay";
import { useMutation } from "@tanstack/react-query";

interface ValidationQuery {
  data: number | undefined;
  isPending: boolean;
}

type DeleteAlertDialogBaseProps<T> = {
  entityToDelete: T | undefined;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  entityLabel: string;
  getEntityName: (entity: T) => string;
  validationQuery?: ValidationQuery;
  validationMessage?: string;
};

type MutationDeleteProps<T> = DeleteAlertDialogBaseProps<T> & {
  deleteMutationOptions: Parameters<typeof useMutation>[0];
  onConfirmDelete?: never;
};

type CallbackDeleteProps<T> = DeleteAlertDialogBaseProps<T> & {
  deleteMutationOptions?: never;
  onConfirmDelete: (entity: T) => void;
};

export type DeleteAlertDialogProps<T> =
  | MutationDeleteProps<T>
  | CallbackDeleteProps<T>;

export function DeleteAlertDialog<T>({
  entityToDelete,
  isOpen,
  onOpenChange,
  entityLabel,
  getEntityName,
  deleteMutationOptions,
  onConfirmDelete,
  validationQuery,
  validationMessage,
}: DeleteAlertDialogProps<T>) {
  const { mutateAsync, isPending: isMutating } = useMutation(
    deleteMutationOptions ?? { mutationFn: async () => {} }
  );

  const isDeletingInProgress = deleteMutationOptions ? isMutating : false;
  const isLoading =
    isDeletingInProgress || (validationQuery?.isPending ?? false);

  const hasDependencies =
    validationQuery?.data !== undefined && validationQuery.data > 0;

  const handleContinueDelete = async () => {
    if (!entityToDelete) return;

    if (deleteMutationOptions) {
      await mutateAsync(undefined as never);
    } else if (onConfirmDelete) {
      onConfirmDelete(entityToDelete);
      onOpenChange(false);
    }
  };

  const entityName = entityToDelete ? getEntityName(entityToDelete) : "";
  const lowerLabel = entityLabel.toLowerCase();

  return (
    <AlertDialog onOpenChange={onOpenChange} open={isOpen}>
      <AlertDialogContent>
        <OverlayLoader
          isLoading={isLoading}
          text={isDeletingInProgress ? "Deleting..." : "Processing.."}
          size="sm"
        />
        <AlertDialogHeader>
          {hasDependencies ? (
            <AlertDialogTitle className="text-danger">
              Unable to Delete {entityLabel}
            </AlertDialogTitle>
          ) : (
            <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
          )}

          {hasDependencies ? (
            <AlertDialogDescription>
              {validationMessage ?? (
                <>
                  This {lowerLabel} cannot be deleted because it has{" "}
                  <strong>{validationQuery!.data}</strong> associated record(s).
                  Please remove them before deleting.
                </>
              )}
            </AlertDialogDescription>
          ) : (
            <AlertDialogDescription>
              Deleting {lowerLabel} <strong>{entityName}</strong> <br />
              This action cannot be undone. This will permanently delete your{" "}
              {lowerLabel} and remove your data from our servers.
            </AlertDialogDescription>
          )}
        </AlertDialogHeader>
        <AlertDialogFooter>
          {hasDependencies ? (
            <AlertDialogCancel onClick={() => onOpenChange(false)}>
              Close
            </AlertDialogCancel>
          ) : (
            <>
              <AlertDialogCancel onClick={() => onOpenChange(false)}>
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
