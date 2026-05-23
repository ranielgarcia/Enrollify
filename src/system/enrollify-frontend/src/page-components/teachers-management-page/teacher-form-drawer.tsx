import {
  createTeacherOptions,
  updateTeacherOptions,
} from "@/api/collections/teacher-collection";
import type { Department } from "@/api/models/department";
import type { Teacher } from "@/api/models/teacher";
import { type SearchableSelectOption } from "@/components/form/searchable-select";
import { SearchableSelect } from "@/components/form/searchable-select";
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
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import { FormField } from "@/components/form/form-field";
import { FormSelectField } from "@/components/form/form-select-field";
import { FormSection } from "@/components/form/form-section";
import { FormDrawerFooter } from "@/components/form/form-drawer-footer";
import { Unauthorized } from "@/components/unauthorized";
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";
import { type FormMeta, defaultFormMeta } from "@/lib/form-meta";
import { useForm } from "@tanstack/react-form";
import { useMutation } from "@tanstack/react-query";
import { CopyPlus, Plus, Upload, X } from "lucide-react";
import { useRef, useState } from "react";
import z from "zod";
import { FormPhoneField } from "@/components/form/form-phone-field";
import { Badge } from "@/components/ui/badge";
import { ManageTeacherSubjectsDialog } from "./manage-teacher-subjects-dialog";

const teacherFormSchema = z.object({
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  middleName: z.string().optional(),
  teacherIdentifier: z.string().min(1, "Teacher ID is required"),
  email: z.email("Valid email is required"),
  phoneNumber: z
    .string()
    .min(11, "Phone number is required")
    .max(11, "Phone number must be 11 digits"),
  departmentId: z.number().min(1, "Department is required"),
  academicTitle: z.string().optional(),
  qualification: z.string().optional(),
  specialization: z.string().optional(),
  officeLocation: z.string().optional(),
  officeHours: z.string().optional(),
  biography: z.string().optional(),
  subjectCodes: z.array(z.string()).min(1, "At least one subject is required"),
});
type TeacherFormData = z.infer<typeof teacherFormSchema>;

interface TeacherFormDrawerProps {
  isOpen: boolean;
  setIsOpen: (open: boolean) => void;
  teacherToUpdate?: Teacher | null;
  onOpenChange: (isOpen: boolean) => void;
  departments: Department[];
}

export function TeacherFormDrawer({
  isOpen,
  setIsOpen,
  teacherToUpdate,
  onOpenChange,
  departments,
}: TeacherFormDrawerProps) {
  const [isOpenManageSubjects, setIsOpenManageSubjects] =
    useState<boolean>(false);
  const [selectedCollegeId, setSelectedCollegeId] = useState<string>(
    teacherToUpdate?.department?.id
      ? (departments
          .find((d) => d.id === teacherToUpdate.department?.id)
          ?.college.id.toString() ?? "")
      : "",
  );
  const [profilePictureFile, setProfilePictureFile] = useState<
    File | undefined
  >();
  const [profilePicturePreview, setProfilePicturePreview] = useState<
    string | undefined
  >();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const { mutateAsync: createTeacherAsync } = useMutation(
    createTeacherOptions(),
  );
  const { mutateAsync: updateTeacherAsync } = useMutation(
    updateTeacherOptions(teacherToUpdate?.id ?? 0),
  );

  const defaultFormValues: TeacherFormData = {
    firstName: teacherToUpdate?.firstName ?? "",
    lastName: teacherToUpdate?.lastName ?? "",
    middleName: teacherToUpdate?.middleName ?? "",
    teacherIdentifier: teacherToUpdate?.teacherIdentifier ?? "",
    email: teacherToUpdate?.email ?? "",
    phoneNumber: teacherToUpdate?.phoneNumber ?? "",
    departmentId: teacherToUpdate?.department?.id ?? 0,
    academicTitle: teacherToUpdate?.academicTitle ?? "",
    qualification: teacherToUpdate?.qualification ?? "",
    specialization: teacherToUpdate?.specialization ?? "",
    officeLocation: teacherToUpdate?.officeLocation ?? "",
    officeHours: teacherToUpdate?.officeHours ?? "",
    biography: teacherToUpdate?.biography ?? "",
    subjectCodes: teacherToUpdate?.subjects?.map((s) => s.code) ?? [],
  };

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onBlur: teacherFormSchema,
      onSubmit: teacherFormSchema,
    },
    onSubmitMeta: defaultFormMeta as FormMeta,
    onSubmit: async ({ value, meta }) => {
      const formData = new FormData();
      formData.append("firstName", value.firstName);
      formData.append("middleName", value.middleName ?? "");
      formData.append("lastName", value.lastName);
      formData.append("teacherIdentifier", value.teacherIdentifier);
      formData.append("email", value.email);
      formData.append("phoneNumber", value.phoneNumber);
      if (value.departmentId)
        formData.append("departmentId", String(value.departmentId));
      if (value.academicTitle)
        formData.append("academicTitle", value.academicTitle);
      if (value.qualification)
        formData.append("qualification", value.qualification);
      if (value.specialization)
        formData.append("specialization", value.specialization);
      if (value.officeLocation)
        formData.append("officeLocation", value.officeLocation);
      if (value.officeHours) formData.append("officeHours", value.officeHours);
      if (value.biography) formData.append("biography", value.biography);
      if (profilePictureFile) formData.append("photo", profilePictureFile);

      value.subjectCodes.forEach((code) =>
        formData.append("subjectCodes", code),
      );

      if (meta.submitAction === "create") {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        await createTeacherAsync(formData as any);
      } else if (meta.submitAction === "update") {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        await updateTeacherAsync(formData as any);
      }

      if (meta.formAction === "close") {
        setIsOpen(false);
      }
      form.reset();
      setProfilePictureFile(undefined);
      setProfilePicturePreview(undefined);
    },
  });

  const teacherDisplayName = teacherToUpdate
    ? `${teacherToUpdate.firstName} ${teacherToUpdate.lastName}`
    : "";

  const isUpdatingTeacher = !!teacherToUpdate;

  const collegeOptions: SearchableSelectOption[] = Array.from(
    new Map(
      departments
        .filter((d) => d.college)
        .map((d) => [
          d.college.id,
          { value: d.college.id.toString(), label: d.college.name },
        ]),
    ).values(),
  );

  const filteredDepartments = selectedCollegeId
    ? departments.filter((d) => d.college?.id.toString() === selectedCollegeId)
    : departments;

  const departmentOptions: SearchableSelectOption[] = filteredDepartments.map(
    (d) => ({
      value: d.id.toString(),
      label: d.name,
    }),
  );

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

  return (
    <>
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
        <DrawerContent className="data-[vaul-drawer-direction=right]:w-[480px] data-[vaul-drawer-direction=right]:sm:max-w-none">
          <AuthorizeView
            policy={isUpdatingTeacher ? "canUpdateTeacher" : "canCreateTeacher"}
            unauthorized={
              <Unauthorized
                message="Your current role does not have the necessary permissions to manage teachers."
                buttonLabel="Back to Home"
                backCallback={() => setIsOpen(false)}
                redirectOptions={{ to: "/portal" }}
              />
            }
          >
            <form
              className="flex flex-col overflow-hidden h-full"
              onSubmit={(e) => {
                e.preventDefault();
                e.stopPropagation();
              }}
            >
              <DrawerHeader className="border-b pb-4">
                <DrawerTitle>
                  {isUpdatingTeacher ? "Update" : "New"} Teacher
                </DrawerTitle>
                <DrawerDescription>
                  Fill in the details below to{" "}
                  {isUpdatingTeacher ? "update this" : "register a new"} teacher
                  profile.
                </DrawerDescription>
              </DrawerHeader>

              <div
                className="flex-1 overflow-y-auto px-4 py-5 space-y-6"
                tabIndex={-1}
              >
                {/* Profile Photo */}
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

                {/* Identification */}
                <FormSection title="Identification">
                  <form.Field
                    name="teacherIdentifier"
                    children={(field) => (
                      <FormField
                        field={field}
                        label="Teacher ID"
                        required
                        placeholder="e.g. TCH-2024-001"
                        hint="Unique employee/staff identifier"
                      />
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
                      <FormPhoneField
                        field={field}
                        label="Phone Number"
                        required
                      />
                    )}
                  />
                </FormSection>

                <Separator />

                {/* Academic Details */}
                <FormSection title="Academic Details">
                  {/* College selector — local UI filter only, not submitted to API */}
                  <div className="grid w-full items-center gap-1.5">
                    <Label>
                      College{" "}
                      <span className="text-xs text-muted-foreground font-normal">
                        (filter departments)
                      </span>
                    </Label>
                    <SearchableSelect
                      options={collegeOptions}
                      value={selectedCollegeId}
                      onValueChange={(val) => {
                        setSelectedCollegeId(val);
                        form.setFieldValue("departmentId", 0);
                      }}
                      placeholder="All colleges"
                      searchPlaceholder="Search colleges..."
                      emptyMessage="No college found"
                    />
                  </div>
                  <form.Field
                    name="departmentId"
                    children={(field) => (
                      <FormSelectField
                        field={field}
                        label="Department"
                        required
                        options={departmentOptions}
                        placeholder="Select a department"
                        searchPlaceholder="Search departments..."
                        emptyMessage="No department found"
                      />
                    )}
                  />
                  <form.Field
                    name="academicTitle"
                    children={(field) => (
                      <FormField
                        field={field}
                        label="Academic Title"
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

                <FormSection title="Manage Qualified Subjects">
                  <form.Field
                    name="subjectCodes"
                    children={(field) => (
                      <>
                        <div className="flex min-h-16 w-full flex-wrap items-center gap-2 rounded-md border bg-muted/40 p-3">
                          {field.state.value && field.state.value.length > 0 ? (
                            field.state.value.map((s) => (
                              <Badge
                                key={s}
                                variant="outline"
                                className="text-sm py-1 px-2 flex items-center gap-2 group hover:bg-secondary/80"
                              >
                                <span className="font-semibold">{s}</span>
                                <button
                                  type="button"
                                  onClick={() => {
                                    field.handleChange(
                                      field.state.value.filter(
                                        (code) => code !== s,
                                      ),
                                    );
                                    field.handleBlur();
                                  }}
                                  className="ml-1 opacity-0 group-hover:opacity-100 transition-opacity hover:text-destructive"
                                  title="Remove subject"
                                  aria-label="Remove subject"
                                >
                                  <X className="size-3" />
                                </button>
                              </Badge>
                            ))
                          ) : (
                            <span className="text-xs text-muted-foreground">
                              Selected subjects will appear here.
                            </span>
                          )}
                        </div>
                        {field.state.meta.errors.length > 0 && (
                          <p className="text-sm text-destructive">
                            {field.state.meta.errors[0]?.message}
                          </p>
                        )}
                      </>
                    )}
                  />
                  <div className="flex justify-end">
                    <Button
                      type="button"
                      variant="link"
                      size="sm"
                      className="p-0 h-auto gap-1.5"
                      onClick={() => setIsOpenManageSubjects(true)}
                    >
                      <CopyPlus className="size-4" />
                      Manage Subjects
                    </Button>
                  </div>
                </FormSection>
              </div>

              <FormDrawerFooter
                form={form}
                isUpdate={isUpdatingTeacher}
                onCancel={() => setIsOpen(false)}
                entityLabel="Teacher"
              />
            </form>
          </AuthorizeView>
        </DrawerContent>
      </Drawer>
      <ManageTeacherSubjectsDialog
        key={teacherToUpdate?.id ?? "new"}
        isOpen={isOpenManageSubjects}
        onOpenChange={(isOpen) => setIsOpenManageSubjects(isOpen)}
        teacherName={teacherDisplayName}
        existingSubjectCodes={form.getFieldValue("subjectCodes")}
        onSubmit={async (subjectCodes) => {
          const existingCodes = form.getFieldValue("subjectCodes");
          const mergedCodes = Array.from(
            new Set([...existingCodes, ...subjectCodes]),
          );
          form.setFieldValue("subjectCodes", mergedCodes);
          await form.validateField("subjectCodes", "blur");
        }}
      />
    </>
  );
}
