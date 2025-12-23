import z from "zod";
import type { College } from "./models/College";
import { useForm } from "@tanstack/react-form";
import useAppMutation from "@/hooks/deprecated/use-app-mutation-v2";
import type { AxiosError } from "axios";
import {
  formatValidationErrors,
  parseApiError,
  type ProblemDetails,
} from "@/lib/axios-utils";
import { toast } from "sonner";
import {
  Drawer,
  DrawerClose,
  DrawerContent,
  DrawerDescription,
  DrawerFooter,
  DrawerHeader,
  DrawerTitle,
  DrawerTrigger,
} from "@/components/ui/drawer";
import { Button } from "@/components/ui/button";
import { Loader2, Plus } from "lucide-react";
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";
import { Unauthorized } from "@/components/unauthorized";

type FormMeta = {
  submitAction: "create" | "update" | null;
  formAction: "close" | "stayopen" | null;
};
// Metadata is not required to call form.handleSubmit().
// Specify what values to use as default if no meta is passed
const defaultMeta: FormMeta = {
  submitAction: null,
  formAction: null,
};

const collegeFormSchema = z.object({
  code: z.string().min(3, "Code is required"),
  name: z.string().min(3, "Name is required"),
  description: z.string().min(3, "Description is required"),
  dean: z.string().min(3, "Dean is required"),
});
type CollegeForm = z.infer<typeof collegeFormSchema>;

interface CollegeFormDrawerProps {
  isOpen: boolean;
  setIsOpen: (open: boolean) => void;
  collegeToUpdate?: College | null;
  onOpenChange: (isOpen: boolean) => void;
  onSuccessful?: () => void;
}

export function CollegeFormDrawer({
  collegeToUpdate,
  onOpenChange,
  isOpen = false,
  setIsOpen,
  onSuccessful,
}: CollegeFormDrawerProps) {
  const defaultFormValues: CollegeForm = {
    code: collegeToUpdate?.code ?? "",
    name: collegeToUpdate?.name ?? "",
    description: collegeToUpdate?.description ?? "",
    dean: collegeToUpdate?.dean ?? "",
  };

  const { mutateAsync: createNewCollegeAsync } = useAppMutation({
    httpVerb: "post",
    path: "/api/colleges",
    mutationKey: "create-new-college",
  });

  const { mutateAsync: updateCollegeAsync } = useAppMutation({
    httpVerb: "put",
    path: "/api/colleges",
    params: collegeToUpdate
      ? {
          id: collegeToUpdate.id,
        }
      : undefined,
    mutationKey: `update-college-${collegeToUpdate?.id}`,
  });

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onChange: collegeFormSchema,
    },
    onSubmitMeta: defaultMeta,
    onSubmit: async ({ value, meta }) => {
      const formValues = collegeFormSchema.parse(value);

      try {
        if (meta.submitAction === "create") {
          await createNewCollegeAsync(formValues);
          toast.success("College created successfully");
        }

        if (meta.submitAction === "update" && collegeToUpdate) {
          await updateCollegeAsync(formValues);
          toast.success("College updated successfully");
        }

        if (meta.formAction === "close") {
          setIsOpen(false);
          if (onSuccessful) onSuccessful();
          form.reset();
        } else {
          form.reset();
        }
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
    },
  });

  const isUpdateCollege = !!collegeToUpdate;
  return (
    <Drawer
      direction="right"
      dismissible={false}
      open={isOpen}
      onOpenChange={onOpenChange}
    >
      <DrawerTrigger asChild>
        <Button className="gap-2 bg-primary text-primary-foreground hover:bg-primary/90 cursor-pointer">
          <Plus className="size-4" />
          Add College
        </Button>
      </DrawerTrigger>
      <DrawerContent>
        <AuthorizeView
          policy="canCreateCollege"
          unauthorized={
            <Unauthorized
              message="Your current role does not have the necessary permissions to create college."
              buttonLabel="Back to Home"
              backCallback={() => setIsOpen(false)}
              redirectOptions={{
                to: "/portal",
              }}
            />
          }
        >
          <form
            className="space-y-4"
            onSubmit={(e) => {
              e.preventDefault();
              e.stopPropagation();
            }}
          >
            <div className="mx-auto w-full max-w-sm">
              <DrawerHeader>
                <DrawerTitle>
                  {isUpdateCollege ? "Update" : "Create"} College
                </DrawerTitle>
                <DrawerDescription>Set college details.</DrawerDescription>
              </DrawerHeader>
              <div className="p-4 pb-0">
                <div>
                  <label className="text-sm font-medium text-foreground block mb-1">
                    Code
                  </label>
                  <form.Field
                    name="code"
                    children={(field) => (
                      <>
                        <input
                          className="w-full px-3 py-2 rounded-lg border border-border bg-background text-foreground"
                          placeholder="e.g., CCS"
                          value={field.state.value}
                          onChange={(e) => field.handleChange(e.target.value)}
                        />
                        <em role="alert" className="text-red-800">
                          {field.state.meta.errors
                            .map((e) => e?.message)
                            .join(", ")}
                        </em>
                      </>
                    )}
                  />
                </div>
                <div>
                  <label className="text-sm font-medium text-foreground block mb-1">
                    Name
                  </label>
                  <form.Field
                    name="name"
                    children={(field) => (
                      <>
                        <input
                          className="w-full px-3 py-2 rounded-lg border border-border bg-background text-foreground"
                          placeholder="e.g., College of Computer Studies"
                          value={field.state.value}
                          onChange={(e) => field.handleChange(e.target.value)}
                        />
                        <em role="alert" className="text-red-800">
                          {field.state.meta.errors
                            .map((e) => e?.message)
                            .join(", ")}
                        </em>
                      </>
                    )}
                  />
                </div>
                <div>
                  <label className="text-sm font-medium text-foreground block mb-1">
                    Description
                  </label>
                  <form.Field
                    name="description"
                    children={(field) => (
                      <>
                        <input
                          className="w-full px-3 py-2 rounded-lg border border-border bg-background text-foreground"
                          placeholder="e.g., College of Computer Studies"
                          value={field.state.value}
                          onChange={(e) => field.handleChange(e.target.value)}
                        />
                        {!field.state.meta.isValid && (
                          <em role="alert" className="text-red-800">
                            {field.state.meta.errors
                              .map((e) => e?.message)
                              .join(", ")}
                          </em>
                        )}
                      </>
                    )}
                  />
                </div>
                <div>
                  <label className="text-sm font-medium text-foreground block mb-1">
                    Dean
                  </label>
                  <form.Field
                    name="dean"
                    children={(field) => (
                      <>
                        <input
                          className="w-full px-3 py-2 rounded-lg border border-border bg-background text-foreground"
                          placeholder="e.g., Raniel Garcia"
                          value={field.state.value}
                          onChange={(e) => field.handleChange(e.target.value)}
                        />
                        {!field.state.meta.isValid && (
                          <em role="alert" className="text-red-800">
                            {field.state.meta.errors
                              .map((e) => e?.message)
                              .join(", ")}
                          </em>
                        )}
                      </>
                    )}
                  />
                </div>
              </div>

              <DrawerFooter>
                <form.Subscribe
                  selector={(state) => [state.canSubmit, state.isSubmitting]}
                  children={([canSubmit, isSubmitting]) => (
                    <Button
                      className=" cursor-pointer"
                      disabled={!canSubmit}
                      type="submit"
                      onClick={() =>
                        form.handleSubmit({
                          submitAction: isUpdateCollege ? "update" : "create",
                          formAction: "close",
                        })
                      }
                    >
                      {isSubmitting ? (
                        <Loader2 />
                      ) : isUpdateCollege ? (
                        "Update"
                      ) : (
                        "Submit"
                      )}
                    </Button>
                  )}
                />
                <DrawerClose asChild>
                  <Button
                    className=" cursor-pointer"
                    variant="outline"
                    onClick={() => setIsOpen(false)}
                  >
                    Cancel
                  </Button>
                </DrawerClose>
              </DrawerFooter>
            </div>
          </form>
        </AuthorizeView>
      </DrawerContent>
    </Drawer>
  );
}
