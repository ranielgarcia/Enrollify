import type { Teacher } from "@/api/models/teacher";
import { type MultiSearchableSelectOption } from "@/components/form/multi-searchable-select";
import { type SearchableSelectOption } from "@/components/form/searchable-select";
import { Button } from "@/components/ui/button";
import {
  Drawer,
  DrawerContent,
  DrawerDescription,
  DrawerHeader,
  DrawerTitle,
  DrawerTrigger,
} from "@/components/ui/drawer";
import { Input } from "@/components/ui/input";
import { Separator } from "@/components/ui/separator";
import { FormField } from "@/components/form/form-field";
import { FormSelectField } from "@/components/form/form-select-field";
import { FormMultiSelectField } from "@/components/form/form-multi-select-field";
import { FormSection } from "@/components/form/form-section";
import { FormDrawerFooter } from "@/components/form/form-drawer-footer";
import { type FormMeta, defaultFormMeta } from "@/lib/form-meta";
import { useForm } from "@tanstack/react-form";
import { Plus, Upload } from "lucide-react";
import { useRef, useState } from "react";
import { toast } from "sonner";
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
      onBlur: teacherFormSchema,
      onSubmit: teacherFormSchema,
    },
    onSubmitMeta: defaultFormMeta as FormMeta,
    onSubmit: async ({ value, meta }) => {
      onSubmit(value, profilePictureFile);
      toast.success(
        `Teacher ${meta.submitAction === "create" ? "created" : "updated"} successfully`,
      );
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
        <Button
          className="gap-2 bg-primary text-primary-foreground hover:bg-primary/90 cursor-pointer"
          size="sm"
        >
          <Plus className="size-4" />
          Add Teacher
        </Button>
      </DrawerTrigger>
      <DrawerContent>
        <form
          className="flex flex-col overflow-hidden h-full"
          onSubmit={(e) => {
            e.preventDefault();
            e.stopPropagation();
          }}
        >
          <DrawerHeader className="border-b pb-4">
            <DrawerTitle>
              {isUpdateTeacher ? "Update" : "New"} Teacher
            </DrawerTitle>
            <DrawerDescription>
              Fill in the details below to{" "}
              {isUpdateTeacher ? "update this" : "create a new"} teacher
              profile.
            </DrawerDescription>
          </DrawerHeader>

          <div className="flex-1 overflow-y-auto px-4 py-5 space-y-6">
            {/* Profile Picture */}
            <FormSection title="Profile Photo">
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
            </FormSection>

            <Separator />

            {/* Personal Information */}
            <FormSection title="Personal Information">
              <div className="grid grid-cols-2 gap-4">
                <form.Field
                  name="firstName"
                  children={(field) => (
                    <FormField
                      field={field}
                      label="First Name"
                      required
                      autoFocus
                    />
                  )}
                />
                <form.Field
                  name="lastName"
                  children={(field) => (
                    <FormField field={field} label="Last Name" required />
                  )}
                />
              </div>
              <form.Field
                name="middleName"
                children={(field) => (
                  <FormField field={field} label="Middle Name" />
                )}
              />
            </FormSection>

            <Separator />

            {/* Contact Details */}
            <FormSection title="Contact Details">
              <form.Field
                name="email"
                children={(field) => (
                  <FormField
                    field={field}
                    label="Email"
                    type="email"
                    required
                    placeholder="email@university.edu"
                  />
                )}
              />
              <form.Field
                name="phoneNumber"
                children={(field) => (
                  <FormField
                    field={field}
                    label="Phone Number"
                    type="tel"
                    required
                    placeholder="+63 912 345 6789"
                  />
                )}
              />
            </FormSection>

            <Separator />

            {/* Academic Details */}
            <FormSection title="Academic Details">
              <form.Field
                name="collegeId"
                children={(field) => (
                  <FormSelectField
                    field={field}
                    label="College"
                    required
                    options={DUMMY_COLLEGES}
                    placeholder="Select a college"
                    searchPlaceholder="Search colleges..."
                    emptyMessage="No college found"
                    onValueChange={(val) => field.handleChange(val)}
                  />
                )}
              />
              <form.Field
                name="departmentId"
                children={(field) => (
                  <FormSelectField
                    field={field}
                    label="Department"
                    required
                    options={DUMMY_DEPARTMENTS}
                    placeholder="Select a department"
                    searchPlaceholder="Search departments..."
                    emptyMessage="No department found"
                    onValueChange={(val) => field.handleChange(val)}
                  />
                )}
              />
              <form.Field
                name="academicTitle"
                children={(field) => (
                  <FormField
                    field={field}
                    label="Academic Title"
                    required
                    placeholder="e.g. Associate Professor"
                  />
                )}
              />
              <form.Field
                name="qualification"
                children={(field) => (
                  <FormField
                    field={field}
                    label="Qualification"
                    type="textarea"
                    required
                    placeholder="e.g. Ph.D. in Computer Science"
                  />
                )}
              />
              <form.Field
                name="specialization"
                children={(field) => (
                  <FormField
                    field={field}
                    label="Specialization"
                    required
                    placeholder="e.g. Machine Learning, Data Mining"
                  />
                )}
              />
            </FormSection>

            <Separator />

            {/* Office Information */}
            <FormSection title="Office Information">
              <form.Field
                name="officeLocation"
                children={(field) => (
                  <FormField
                    field={field}
                    label="Office Location"
                    required
                    placeholder="e.g. Engineering Building, Room 302"
                  />
                )}
              />
              <form.Field
                name="officeHours"
                children={(field) => (
                  <FormField
                    field={field}
                    label="Office Hours"
                    required
                    placeholder="e.g. MWF 10:00 AM - 12:00 PM"
                  />
                )}
              />
            </FormSection>

            <Separator />

            {/* Biography */}
            <FormSection title="Biography">
              <form.Field
                name="biography"
                children={(field) => (
                  <FormField
                    field={field}
                    label="Biography"
                    type="textarea"
                    placeholder="A brief biography about the teacher..."
                    hint="Optional"
                    maxLength={1000}
                  />
                )}
              />
            </FormSection>

            <Separator />

            {/* Teaching Assignment */}
            <FormSection title="Teaching Assignment">
              <form.Field
                name="subjectIds"
                children={(field) => (
                  <FormMultiSelectField
                    field={field}
                    label="Subjects"
                    required
                    options={DUMMY_SUBJECTS}
                    placeholder="Select subjects..."
                    searchPlaceholder="Search subjects..."
                    emptyMessage="No subject found"
                  />
                )}
              />
            </FormSection>
          </div>

          <FormDrawerFooter
            form={form}
            isUpdate={isUpdateTeacher}
            onCancel={() => setIsOpen(false)}
            entityLabel="Teacher"
          />
        </form>
      </DrawerContent>
    </Drawer>
  );
}
