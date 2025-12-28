import z from "zod";
import { useForm } from "@tanstack/react-form";
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
import {
  createCollegeOptions,
  updateCollegeOptions,
} from "@/api/collections/college-collection";
import type { College } from "@/api/models/college";
import { useMutation } from "@tanstack/react-query";
import { Textarea } from "@/components/ui/textarea";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

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
}

export function CollegeFormDrawer({
  collegeToUpdate,
  onOpenChange,
  isOpen = false,
  setIsOpen,
}: CollegeFormDrawerProps) {
  const defaultFormValues: CollegeForm = {
    code: collegeToUpdate?.code ?? "",
    name: collegeToUpdate?.name ?? "",
    description: collegeToUpdate?.description ?? "",
    dean: collegeToUpdate?.dean ?? "",
  };

  const { mutateAsync: createNewCollegeAsync } = useMutation(
    createCollegeOptions()
  );

  const { mutateAsync: updateCollegeAsync } = useMutation(
    updateCollegeOptions(collegeToUpdate?.id ?? 0)
  );

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onChange: collegeFormSchema,
    },
    onSubmitMeta: defaultMeta,
    onSubmit: async ({ value, meta }) => {
      const formValues = collegeFormSchema.parse(value);
      if (meta.submitAction === "create") {
        await createNewCollegeAsync(formValues);
      } else if (meta.submitAction === "update" && collegeToUpdate) {
        await updateCollegeAsync(formValues);
      }

      if (meta.formAction === "close") {
        setIsOpen(false);
      }
      form.reset();
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
                <div className="pb-4">
                  <form.Field
                    name="code"
                    children={(field) => (
                      <div className="grid w-full max-w-sm items-center gap-3">
                        <Label htmlFor={field.name}>Code:</Label>
                        <Input
                          type="text"
                          id={field.name}
                          placeholder="Code:"
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
                      </div>
                    )}
                  />
                </div>
                <div className="pb-4">
                  <form.Field
                    name="name"
                    children={(field) => (
                      <div className="grid w-full max-w-sm items-center gap-3">
                        <Label htmlFor={field.name}>Name:</Label>
                        <Input
                          type="text"
                          id={field.name}
                          placeholder="Name:"
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
                      </div>
                    )}
                  />
                </div>
                <div className="pb-4">
                  <form.Field
                    name="description"
                    children={(field) => (
                      <div className="grid w-full max-w-sm items-center gap-3">
                        <Label htmlFor={field.name}>Description:</Label>
                        <Textarea
                          placeholder="Description:"
                          id={field.name}
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
                      </div>
                    )}
                  />
                </div>
                <div className="pb-4">
                  <form.Field
                    name="dean"
                    children={(field) => (
                      <div className="grid w-full max-w-sm items-center gap-3">
                        <Label htmlFor={field.name}>Dean:</Label>
                        <Input
                          type="text"
                          id={field.name}
                          placeholder="Dean:"
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
                      </div>
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
