import type { Teacher } from "@/api/models/teacher";
import {
  MultiSearchableSelect,
  type MultiSearchableSelectOption,
} from "@/components/multi-searchable-select";
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
import { FormField } from "@/components/form-field";
import { useForm } from "@tanstack/react-form";
import { Loader2, Plus, Upload } from "lucide-react";
import { useRef, useState } from "react";
import z from "zod";

const DUMMY_COLLEGES: SearchableSelectOption[] = [
  { value: "1", label: "College of Engineering" },
  { value: "2", label: "College of Science" },
  { value: "3", label: "College of Arts and Sciences" },
  { value: "4", label: "College of Business Administration" },
  { value: "5", label: "College of Education" },
];

const DUMMY_DEPARTMENTS: SearchableSelectOption[] = [
  { value: "1", label: "Computer Engineering" },
  { value: "2", label: "Mathematics" },
  { value: "3", label: "Physics" },
  { value: "4", label: "Electronics Engineering" },
  { value: "5", label: "Chemistry" },
  { value: "6", label: "Management" },
  { value: "7", label: "Accountancy" },
];

const DUMMY_SUBJECTS: MultiSearchableSelectOption[] = [
  { value: "1", label: "CS101 - Introduction to Programming" },
  { value: "2", label: "MATH101 - College Algebra" },
  { value: "3", label: "PHY101 - General Physics I" },
  { value: "4", label: "EE101 - Basic Electrical Engineering" },
  { value: "5", label: "CS301 - Machine Learning" },
  { value: "6", label: "MATH201 - Calculus I" },
  { value: "7", label: "PHY102 - General Physics II" },
  { value: "8", label: "EE201 - Digital Electronics" },
  { value: "9", label: "CHEM101 - General Chemistry" },
  { value: "10", label: "CHEM201 - Organic Chemistry" },
];

type FormMeta = {
  submitAction: "create" | "update" | null;
  formAction: "close" | "stayopen" | null;
};
const defaultMeta: FormMeta = {
  submitAction: null,
  formAction: null,
};

const teacherFormSchema = z.object({
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  middleName: z.string(),
  email: z.email("Valid email is required"),
  phoneNumber: z.string().min(7, "Phone number is required"),
  collegeId: z.string().min(1, "College is required"),
  academicTitle: z.string().min(1, "Academic title is required"),
  departmentId: z.string().min(1, "Department is required"),
  qualification: z.string().min(3, "Qualification is required"),
  specialization: z.string().min(1, "Specialization is required"),
  officeLocation: z.string().min(1, "Office location is required"),
  officeHours: z.string().min(1, "Office hours are required"),
  biography: z.string().optional(),
  subjectIds: z.array(z.string()).min(1, "At least one subject is required"),
});
type TeacherFormData = z.infer<typeof teacherFormSchema>;

interface TeacherFormDrawerProps {
  isOpen: boolean;
  setIsOpen: (open: boolean) => void;
  teacherToUpdate?: Teacher | null;
  onOpenChange: (isOpen: boolean) => void;
  onSubmit: (data: TeacherFormData, profilePicture?: File) => void;
}

export function TeacherFormDrawer({
  isOpen,
  setIsOpen,
  teacherToUpdate,
  onOpenChange,
  onSubmit,
}: TeacherFormDrawerProps) {
  const [profilePictureFile, setProfilePictureFile] = useState<
    File | undefined
  >();
  const [profilePicturePreview, setProfilePicturePreview] = useState<
    string | undefined
  >(teacherToUpdate?.profilePicture);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const defaultFormValues: TeacherFormData = {
    firstName: teacherToUpdate?.firstName ?? "",
    lastName: teacherToUpdate?.lastName ?? "",
    middleName: teacherToUpdate?.middleName ?? "",
    email: teacherToUpdate?.email ?? "",
    phoneNumber: teacherToUpdate?.phoneNumber ?? "",
    collegeId: teacherToUpdate?.college.id.toString() ?? "",
    academicTitle: teacherToUpdate?.academicTitle ?? "",
    departmentId: teacherToUpdate?.department.id.toString() ?? "",
    qualification: teacherToUpdate?.qualification ?? "",
    specialization: teacherToUpdate?.specialization ?? "",
    officeLocation: teacherToUpdate?.officeLocation ?? "",
    officeHours: teacherToUpdate?.officeHours ?? "",
    biography: teacherToUpdate?.biography ?? "",
    subjectIds: teacherToUpdate?.subjects.map((s) => s.id.toString()) ?? [],
  };

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onChange: teacherFormSchema,
    },
    onSubmitMeta: defaultMeta,
    onSubmit: async ({ value, meta }) => {
      onSubmit(value, profilePictureFile);
      if (meta.formAction === "close") {
        setIsOpen(false);
      }
      form.reset();
      setProfilePictureFile(undefined);
      setProfilePicturePreview(undefined);
    },
  });

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setProfilePictureFile(file);
      const reader = new FileReader();
      reader.onloadend = () => {
        setProfilePicturePreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const isUpdateTeacher = !!teacherToUpdate;

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
          Add Teacher
        </Button>
      </DrawerTrigger>
      <DrawerContent>
        <form
          className="space-y-4 h-full"
          onSubmit={(e) => {
            e.preventDefault();
            e.stopPropagation();
          }}
        >
          <div className="w-full h-full flex flex-col">
            <DrawerHeader>
              <DrawerTitle>
                {isUpdateTeacher ? "Update" : "Create"} Teacher
              </DrawerTitle>
              <DrawerDescription>
                Set teacher profile details.
              </DrawerDescription>
            </DrawerHeader>

            <div className="flex-1 overflow-y-auto px-4 pb-4 space-y-4">
              {/* Profile Picture */}
              <div className="grid w-full items-center gap-3">
                <Label>Profile Picture</Label>
                <div className="flex items-center gap-4">
                  {profilePicturePreview ? (
                    <img
                      src={profilePicturePreview}
                      alt="Profile preview"
                      className="size-16 rounded-full object-cover border"
                    />
                  ) : (
                    <div className="size-16 rounded-full bg-muted flex items-center justify-center border text-muted-foreground text-xs">
                      No photo
                    </div>
                  )}
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    className="gap-2 cursor-pointer"
                    onClick={() => fileInputRef.current?.click()}
                  >
                    <Upload className="size-4" />
                    Upload Photo
                  </Button>
                  <Input
                    ref={fileInputRef}
                    type="file"
                    accept="image/*"
                    className="hidden"
                    onChange={handleFileChange}
                  />
                </div>
              </div>

              {/* First Name */}
              <form.Field
                name="firstName"
                children={(field) => (
                  <FormField field={field} label="First Name:" placeholder="First Name" />
                )}
              />

              {/* Last Name */}
              <form.Field
                name="lastName"
                children={(field) => (
                  <FormField field={field} label="Last Name:" placeholder="Last Name" />
                )}
              />

              {/* Middle Name */}
              <form.Field
                name="middleName"
                children={(field) => (
                  <FormField field={field} label="Middle Name:" placeholder="Middle Name" />
                )}
              />

              {/* Email */}
              <form.Field
                name="email"
                children={(field) => (
                  <FormField
                    field={field}
                    label="Email:"
                    type="email"
                    placeholder="email@university.edu"
                  />
                )}
              />

              {/* Phone Number */}
              <form.Field
                name="phoneNumber"
                children={(field) => (
                  <FormField
                    field={field}
                    label="Phone Number:"
                    type="tel"
                    placeholder="+63 912 345 6789"
                  />
                )}
              />

              {/* College */}
              <form.Field
                name="collegeId"
                children={(field) => (
                  <div className="grid w-full items-center gap-3">
                    <Label htmlFor={field.name}>College:</Label>
                    <SearchableSelect
                      options={DUMMY_COLLEGES}
                      value={field.state.value}
                      onValueChange={(val) => field.handleChange(val)}
                      name={field.name}
                      placeholder="Select a college"
                      searchPlaceholder="Search colleges..."
                      emptyMessage="No college found"
                    />
                    {!field.state.meta.isValid && (
                      <em role="alert" className="text-destructive text-sm">
                        {field.state.meta.errors
                          .map((e) => e?.message)
                          .join(", ")}
                      </em>
                    )}
                  </div>
                )}
              />

              {/* Academic Title */}
              <form.Field
                name="academicTitle"
                children={(field) => (
                  <FormField
                    field={field}
                    label="Academic Title:"
                    placeholder="e.g. Associate Professor"
                  />
                )}
              />

              {/* Department */}
              <form.Field
                name="departmentId"
                children={(field) => (
                  <div className="grid w-full items-center gap-3">
                    <Label htmlFor={field.name}>Department:</Label>
                    <SearchableSelect
                      options={DUMMY_DEPARTMENTS}
                      value={field.state.value}
                      onValueChange={(val) => field.handleChange(val)}
                      name={field.name}
                      placeholder="Select a department"
                      searchPlaceholder="Search departments..."
                      emptyMessage="No department found"
                    />
                    {!field.state.meta.isValid && (
                      <em role="alert" className="text-destructive text-sm">
                        {field.state.meta.errors
                          .map((e) => e?.message)
                          .join(", ")}
                      </em>
                    )}
                  </div>
                )}
              />

              {/* Qualification */}
              <form.Field
                name="qualification"
                children={(field) => (
                  <FormField
                    field={field}
                    label="Qualification:"
                    type="textarea"
                    placeholder="e.g. Ph.D. in Computer Science, M.Sc. in Software Engineering"
                  />
                )}
              />

              {/* Specialization */}
              <form.Field
                name="specialization"
                children={(field) => (
                  <FormField
                    field={field}
                    label="Specialization:"
                    placeholder="e.g. Machine Learning, Data Mining"
                  />
                )}
              />

              {/* Office Information */}
              <div className="space-y-3">
                <Label className="text-sm font-semibold">
                  Office Information
                </Label>
                <form.Field
                  name="officeLocation"
                  children={(field) => (
                    <div className="grid w-full items-center gap-2">
                      <Label
                        htmlFor={field.name}
                        className="text-muted-foreground text-xs"
                      >
                        Office Location:
                      </Label>
                      <Input
                        type="text"
                        id={field.name}
                        placeholder="e.g. Engineering Building, Room 302"
                        value={field.state.value}
                        onChange={(e) => field.handleChange(e.target.value)}
                      />
                      {!field.state.meta.isValid && (
                        <em role="alert" className="text-destructive text-sm">
                          {field.state.meta.errors
                            .map((e) => e?.message)
                            .join(", ")}
                        </em>
                      )}
                    </div>
                  )}
                />
                <form.Field
                  name="officeHours"
                  children={(field) => (
                    <div className="grid w-full items-center gap-2">
                      <Label
                        htmlFor={field.name}
                        className="text-muted-foreground text-xs"
                      >
                        Office Hours:
                      </Label>
                      <Input
                        type="text"
                        id={field.name}
                        placeholder="e.g. MWF 10:00 AM - 12:00 PM"
                        value={field.state.value}
                        onChange={(e) => field.handleChange(e.target.value)}
                      />
                      {!field.state.meta.isValid && (
                        <em role="alert" className="text-destructive text-sm">
                          {field.state.meta.errors
                            .map((e) => e?.message)
                            .join(", ")}
                        </em>
                      )}
                    </div>
                  )}
                />
              </div>

              {/* Biography */}
              <form.Field
                name="biography"
                children={(field) => (
                  <div className="grid w-full items-center gap-3">
                    <Label htmlFor={field.name}>
                      Biography{" "}
                      <span className="text-muted-foreground text-xs">
                        (optional)
                      </span>
                    </Label>
                    <Textarea
                      id={field.name}
                      placeholder="A brief biography about the teacher..."
                      value={field.state.value ?? ""}
                      onChange={(e) => field.handleChange(e.target.value)}
                      rows={4}
                    />
                  </div>
                )}
              />

              {/* Subjects */}
              <form.Field
                name="subjectIds"
                children={(field) => (
                  <div className="grid w-full items-center gap-3">
                    <Label htmlFor={field.name}>Subjects:</Label>
                    <MultiSearchableSelect
                      options={DUMMY_SUBJECTS}
                      value={field.state.value}
                      onValueChange={(val) => field.handleChange(val)}
                      name={field.name}
                      placeholder="Select subjects..."
                      searchPlaceholder="Search subjects..."
                      emptyMessage="No subject found"
                    />
                    {!field.state.meta.isValid && (
                      <em role="alert" className="text-destructive text-sm">
                        {field.state.meta.errors
                          .map((e) => e?.message)
                          .join(", ")}
                      </em>
                    )}
                  </div>
                )}
              />
            </div>

            <DrawerFooter>
              <form.Subscribe
                selector={(state) => [state.canSubmit, state.isSubmitting]}
                children={([canSubmit, isSubmitting]) => (
                  <Button
                    className="cursor-pointer"
                    disabled={!canSubmit}
                    type="submit"
                    onClick={() =>
                      form.handleSubmit({
                        submitAction: isUpdateTeacher ? "update" : "create",
                        formAction: "close",
                      })
                    }
                  >
                    {isSubmitting ? (
                      <Loader2 className="size-4 animate-spin" />
                    ) : isUpdateTeacher ? (
                      "Update"
                    ) : (
                      "Submit"
                    )}
                  </Button>
                )}
              />
              <DrawerClose asChild>
                <Button
                  className="cursor-pointer"
                  variant="outline"
                  onClick={() => setIsOpen(false)}
                >
                  Cancel
                </Button>
              </DrawerClose>
            </DrawerFooter>
          </div>
        </form>
      </DrawerContent>
    </Drawer>
  );
}
