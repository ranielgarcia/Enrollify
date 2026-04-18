import { Button } from "@/components/ui/button";
import { DrawerFooter } from "@/components/ui/drawer";
import { Loader2 } from "lucide-react";

interface FormDrawerFooterProps {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  form: { Subscribe: any; handleSubmit: (meta?: any) => void };
  isUpdate: boolean;
  onCancel: () => void;
  entityLabel: string;
  showSaveAndAddAnother?: boolean;
}

export function FormDrawerFooter({
  form,
  isUpdate,
  onCancel,
  entityLabel,
  showSaveAndAddAnother = false,
}: FormDrawerFooterProps) {
  return (
    <DrawerFooter className="border-t bg-background pt-4">
      <form.Subscribe
        selector={(state: { canSubmit: boolean; isSubmitting: boolean }) => [state.canSubmit, state.isSubmitting]}
        children={([canSubmit, isSubmitting]: [boolean, boolean]) => (
          <div className="flex flex-col gap-2">
            <Button
              className="cursor-pointer"
              disabled={!canSubmit || isSubmitting}
              type="submit"
              onClick={() =>
                form.handleSubmit({
                  submitAction: isUpdate ? "update" : "create",
                  formAction: "close",
                })
              }
            >
              {isSubmitting ? (
                <Loader2 className="size-4 animate-spin" />
              ) : isUpdate ? (
                `Update ${entityLabel}`
              ) : (
                `Create ${entityLabel}`
              )}
            </Button>
            {showSaveAndAddAnother && !isUpdate && (
              <Button
                variant="secondary"
                className="cursor-pointer"
                disabled={!canSubmit || isSubmitting}
                type="submit"
                onClick={() =>
                  form.handleSubmit({
                    submitAction: "create",
                    formAction: "stayopen",
                  })
                }
              >
                Save & Add Another
              </Button>
            )}
          </div>
        )}
      />
      <Button variant="outline" className="cursor-pointer" onClick={onCancel}>
        Cancel
      </Button>
    </DrawerFooter>
  );
}
