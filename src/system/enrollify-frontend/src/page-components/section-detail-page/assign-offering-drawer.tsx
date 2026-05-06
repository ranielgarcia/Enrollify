import { useState } from "react";
import { createOfferingOptions } from "@/api/collections/offering-collection";
import { FormField } from "@/components/form/form-field";
import { FormSelectField } from "@/components/form/form-select-field";
import { FormSection } from "@/components/form/form-section";
import { FormTimePicker } from "@/components/form/form-time-picker";
import { Button } from "@/components/ui/button";
import {
  Drawer,
  DrawerContent,
  DrawerHeader,
  DrawerTitle,
} from "@/components/ui/drawer";
import { useForm } from "@tanstack/react-form";
import { useMutation, useSuspenseQuery } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import { z } from "zod";
import { getAllTeachersOptions } from "@/api/collections/teacher-collection";

const DAY_PATTERNS = [
  { value: "MW", label: "Mon / Wed (MW)" },
  { value: "TTh", label: "Tue / Thu (TTh)" },
  { value: "MWF", label: "Mon / Wed / Fri (MWF)" },
  { value: "MTWTHF", label: "Mon–Fri (MTWTHF)" },
  { value: "SAT", label: "Saturday only" },
];

const assignOfferingSchema = z.object({
  subjectId: z.number().min(1, "Subject is required"),
  teacherId: z.number().min(1, "Teacher is required"),
  roomId: z.number().min(1, "Room is required"),
  maxNumberOfStudents: z.number().int().min(1).nullable(),
  dayPattern: z.string().min(1, "Day pattern is required"),
  startTime: z.string().regex(/^\d{2}:\d{2}$/, "Enter time as HH:MM"),
  endTime: z.string().regex(/^\d{2}:\d{2}$/, "Enter time as HH:MM"),
});

// Mock subjects — replace with real subject collection when available
const MOCK_SUBJECTS = [
  { id: 1, code: "CS101", title: "Programming 1" },
  { id: 2, code: "MATH101", title: "Calculus 1" },
  { id: 3, code: "ENG101", title: "English Communication 1" },
  { id: 4, code: "CS201", title: "Data Structures" },
  { id: 5, code: "PE101", title: "Physical Education 1" },
];

// Mock rooms — replace with real room collection when available
const MOCK_ROOMS = [
  { id: 1, roomNumber: "Rm 101", building: "Main Building" },
  { id: 2, roomNumber: "Rm 202", building: "Science Bldg" },
  { id: 3, roomNumber: "Lab 301", building: "IT Building" },
  { id: 4, roomNumber: "Gym A", building: "Sports Complex" },
];

interface AssignOfferingDrawerProps {
  sectionId: number;
  onSuccess?: () => void;
}

export function AssignOfferingDrawer({
  sectionId,
  onSuccess,
}: AssignOfferingDrawerProps) {
  const [isOpen, setIsOpen] = useState(false);

  const { mutateAsync: createOffering } = useMutation(
    createOfferingOptions(sectionId),
  );

  const { data: teachers } = useSuspenseQuery(getAllTeachersOptions());

  const subjectOptions = MOCK_SUBJECTS.map((s) => ({
    value: s.id.toString(),
    label: `${s.code} — ${s.title}`,
  }));

  const teacherOptions = (teachers ?? []).map((t) => ({
    value: t.id.toString(),
    label: `${t.firstName} ${t.lastName}`,
  }));

  const roomOptions = MOCK_ROOMS.map((r) => ({
    value: r.id.toString(),
    label: `${r.roomNumber} (${r.building})`,
  }));

  const form = useForm({
    defaultValues: {
      subjectId: 0,
      teacherId: 0,
      roomId: 0,
      maxNumberOfStudents: null as number | null,
      dayPattern: "",
      startTime: "",
      endTime: "",
    },
    validators: {
      onBlur: assignOfferingSchema,
      onSubmit: assignOfferingSchema,
    },
    onSubmit: async ({ value }) => {
      await createOffering({ ...value, sectionId } as any);
      setIsOpen(false);
      form.reset();
      onSuccess?.();
    },
  });

  return (
    <>
      <Button className="gap-2" onClick={() => setIsOpen(true)}>
        <Plus className="size-4" />
        Assign Subject Offering
      </Button>

      <Drawer open={isOpen} onOpenChange={setIsOpen} direction="right" dismissible={false}>
        <DrawerContent className="h-full w-full max-w-md overflow-y-auto">
          <DrawerHeader className="border-b pb-4">
            <DrawerTitle>Assign Subject Offering</DrawerTitle>
          </DrawerHeader>

          <form
            className="flex flex-col gap-6 p-4"
            onSubmit={(e) => e.preventDefault()}
          >
            <FormSection title="Subject">
              <form.Field name="subjectId">
                {(field) => (
                  <FormSelectField
                    field={field}
                    label="Subject"
                    options={subjectOptions}
                    placeholder="Select subject"
                    searchPlaceholder="Search subjects..."
                    required
                  />
                )}
              </form.Field>
            </FormSection>

            <FormSection title="Assignment">
              <form.Field name="teacherId">
                {(field) => (
                  <FormSelectField
                    field={field}
                    label="Teacher"
                    options={teacherOptions}
                    placeholder="Select teacher"
                    searchPlaceholder="Search teachers..."
                    required
                  />
                )}
              </form.Field>

              <form.Field name="roomId">
                {(field) => (
                  <FormSelectField
                    field={field}
                    label="Room"
                    options={roomOptions}
                    placeholder="Select room"
                    searchPlaceholder="Search rooms..."
                    required
                  />
                )}
              </form.Field>

              <form.Field name="maxNumberOfStudents">
                {(field) => (
                  <FormField
                    field={field}
                    label="Max Students (optional)"
                    type="number"
                    placeholder="Leave blank to use section capacity"
                    hint="Override the section's student capacity for this offering"
                  />
                )}
              </form.Field>
            </FormSection>

            <FormSection title="Schedule Pattern">
              <form.Field name="dayPattern">
                {(field) => (
                  <FormSelectField
                    field={field}
                    label="Day Pattern"
                    options={DAY_PATTERNS}
                    placeholder="Select day pattern"
                    required
                  />
                )}
              </form.Field>

              <form.Field name="startTime">
                {(field) => (
                  <FormTimePicker
                    field={field}
                    label="Start Time"
                    required
                    hint="e.g. 08:00"
                  />
                )}
              </form.Field>

              <form.Field name="endTime">
                {(field) => (
                  <FormTimePicker
                    field={field}
                    label="End Time"
                    required
                    hint="e.g. 09:30"
                  />
                )}
              </form.Field>
            </FormSection>
          </form>

          <div className="border-t p-4 flex flex-col gap-2">
            <form.Subscribe
              selector={(s) => [s.canSubmit, s.isSubmitting] as const}
            >
              {([canSubmit, isSubmitting]) => (
                <Button
                  disabled={!canSubmit || isSubmitting}
                  onClick={() => form.handleSubmit()}
                >
                  Assign Offering
                </Button>
              )}
            </form.Subscribe>
            <Button variant="outline" onClick={() => setIsOpen(false)}>
              Cancel
            </Button>
          </div>
        </DrawerContent>
      </Drawer>
    </>
  );
}
