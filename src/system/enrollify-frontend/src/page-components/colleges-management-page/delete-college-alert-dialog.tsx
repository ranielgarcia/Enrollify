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
import type { College } from "./models/College";
import { OverlayLoader } from "@/components/app-loading-overlay";
import useAppMutation from "@/hooks/use-app-mutation-v2";
import type { AxiosError } from "axios";
import {
  formatValidationErrors,
  parseApiError,
  type ProblemDetails,
} from "@/lib/axios-utils";
import { toast } from "sonner";

interface DeleteCollegeAlertDialogProps {
  collegeToDelete?: College;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccessful: () => void;
}

export function DeleteCollegeAlertDialog({
  collegeToDelete,
  isOpen,
  onOpenChange,
  onSuccessful,
}: DeleteCollegeAlertDialogProps) {
  const { mutateAsync: deleteCollegeAsync, isPending: isDeletingInProgress } =
    useAppMutation({
      httpVerb: "delete",
      path: "/api/colleges",
      mutationKey: `delete-college-${collegeToDelete?.id}`,
      params: {
        id: collegeToDelete?.id ?? 0,
      },
    });

  const handleContinueDelete = async () => {
    try {
      await deleteCollegeAsync(undefined);
      toast.success("College is deleted successfully");
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
          isLoading={isDeletingInProgress}
          text={isDeletingInProgress ? "Deleting..." : "Processing.."}
          size="sm"
        />
        <AlertDialogHeader>
          <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>

          <AlertDialogDescription>
            Deleting college <strong>{collegeToDelete?.name}</strong> <br />
            This action cannot be undone. This will permanently delete your
            college and remove your data from our servers.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel onClick={() => onOpenChange(false)}>
            Cancel
          </AlertDialogCancel>
          <AlertDialogAction onClick={async () => await handleContinueDelete()}>
            Continue
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
