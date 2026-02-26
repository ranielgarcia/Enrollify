import {
  createCourseOptions,
  updateCourseOptions,
} from "@/api/collections/course-collection";
import type { College } from "@/api/models/college";
import type { Course } from "@/api/models/course";
import {
  SearchableSelect,
  type SearchableSelectOption,
} from "@/components/searchable-select";
import { Button } from "@/components/ui/button";
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
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Unauthorized } from "@/components/unauthorized";
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";
import { useForm } from "@tanstack/react-form";
import { useMutation } from "@tanstack/react-query";
import { Plus, Loader2 } from "lucide-react";
import z from "zod";

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

const courseFormSchema = z.object({
  code: z.string().min(3, "Code is required"),
  name: z.string().min(3, "Name is required"),
  description: z.string().min(3, "Description is required"),
  durationYears: z
    .number()
    .min(1, "Duration must be at least 1 year")
    .max(10, "Duration cannot exceed 10 years"),
  collegeId: z.number().min(1, "College is required"),
});

type CourseFormData = z.infer<typeof courseFormSchema>;

interface CourseFormDrawerProps {
  colleges: College[];
  isOpen: boolean;
  setIsOpen: (open: boolean) => void;
  courseToUpdate?: Course | undefined;
  onOpenChange: (isOpen: boolean) => void;
}

export function CourseFormDrawer({
  colleges,
  isOpen,
  setIsOpen,
  courseToUpdate,
  onOpenChange,
}: CourseFormDrawerProps) {
  const defaultFormValues: CourseFormData = {
    code: courseToUpdate?.code ?? "",
    name: courseToUpdate?.name ?? "",
    description: courseToUpdate?.description ?? "",
    durationYears: courseToUpdate?.durationYears ?? 1,
    collegeId: courseToUpdate?.college?.id ?? 0,
  };

  const { mutateAsync: createNewCourseAsync } = useMutation(
    createCourseOptions(),
  );
  const { mutateAsync: updateCourseAsync } = useMutation(
    updateCourseOptions(courseToUpdate?.id ?? 0),
  );

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onChange: courseFormSchema,
    },
    onSubmitMeta: defaultMeta,
    onSubmit: async ({ value, meta }) => {
      const formValues = courseFormSchema.parse(value);
      if (meta.submitAction === "create") {
        await createNewCourseAsync(formValues);
      } else if (meta.submitAction === "update") {
        await updateCourseAsync(formValues);
      }

      if (meta.formAction === "close") {
        setIsOpen(false);
      }
      form.reset();
    },
  });

  const collegesOptions: SearchableSelectOption[] =
    colleges?.map((college) => ({
      value: college.id.toString(),
      label: college.name,
    })) ?? [];

  const isUpdateCourse = !!courseToUpdate;

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
          Add Course
        </Button>
      </DrawerTrigger>
      <DrawerContent>
        <AuthorizeView
          policy={isUpdateCourse ? "canUpdateCourse" : "canCreateCourse"}
          unauthorized={
            <Unauthorized
              message="Your current role does not have the necessary permissions to create course."
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
                  {isUpdateCourse ? "Update" : "Create"} Course
                </DrawerTitle>
                <DrawerDescription>Set department details.</DrawerDescription>
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
                          placeholder="Code:"
                          value={field.state.value}
                          onChange={(e) => field.handleChange(e.target.value)}
                          id={field.name}
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
                          placeholder="Name:"
                          value={field.state.value}
                          onChange={(e) => field.handleChange(e.target.value)}
                          id={field.name}
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
                          value={field.state.value}
                          onChange={(e) => field.handleChange(e.target.value)}
                          id={field.name}
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
                    name="durationYears"
                    children={(field) => (
                      <div className="grid w-full max-w-sm items-center gap-3">
                        <Label htmlFor={field.name}>Duration Years:</Label>
                        <Input
                          type="number"
                          placeholder="Duration Years:"
                          value={field.state.value}
                          onChange={(e) =>
                            field.handleChange(Number(e.target.value))
                          }
                          id={field.name}
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
                    name="collegeId"
                    children={(field) => (
                      <div className="grid w-full max-w-sm items-center gap-3">
                        <Label htmlFor={field.name}>College:</Label>
                        <SearchableSelect
                          options={collegesOptions}
                          value={field.state.value.toString()}
                          onValueChange={(val) =>
                            field.handleChange(Number(val))
                          }
                          name={field.name}
                          placeholder="Select a college"
                          searchPlaceholder="Search colleges..."
                          emptyMessage="No college found"
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
                          submitAction: isUpdateCourse ? "update" : "create",
                          formAction: "close",
                        })
                      }
                    >
                      {isSubmitting ? (
                        <Loader2 />
                      ) : isUpdateCourse ? (
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
