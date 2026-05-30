import { useState } from "react";
import { updateOfferingOptions } from "@/api/collections/offering-collection";
import { getAllRooms } from "@/api/collections/room-collection";
import { searchTeachersPaginatedOptions } from "@/api/collections/teacher-collection";
import type { Offering } from "@/api/models/offering";
import type { components } from "@/api/generated/api";

type UpdateOfferingRequest =
  components["schemas"]["EnrollifyWebAPIFeaturesSubjectOfferingsUpdateSubjectOfferingRequest"];
import { FormSection } from "@/components/form/form-section";
import { FormField } from "@/components/form/form-field";
import { SearchableSelect } from "@/components/form/searchable-select";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import {
  Drawer,
  DrawerContent,
  DrawerHeader,
  DrawerTitle,
} from "@/components/ui/drawer";
import { useForm } from "@tanstack/react-form";
import { useMutation, useQuery } from "@tanstack/react-query";
import { Pencil } from "lucide-react";
import { z } from "zod";

const editOfferingSchema = z.object({
  teacherId: z.number().nullable().optional(),
  roomId: z.number().nullable().optional(),
  daysPerWeek: z.number().int().min(1).max(7),
  hoursPerDay: z.number().min(0.5).max(24),
  subjectUnitsOverride: z.number().positive().nullable().optional(),
  maxNumberOfStudents: z.number().int().positive().nullable().optional(),
});

interface EditOfferingDrawerProps {
  offering: Offering;
  sectionId: number;
}

export function EditOfferingDrawer({
  offering,
  sectionId,
}: EditOfferingDrawerProps) {
  const [isOpen, setIsOpen] = useState(false);

  const { mutateAsync: updateOffering } = useMutation(
    updateOfferingOptions(offering.id, sectionId),
  );

  const { data: rooms = [] } = useQuery({
    ...getAllRooms(),
    enabled: isOpen,
  });

  const { data: teachersPage } = useQuery({
    ...searchTeachersPaginatedOptions(1, 200, undefined, isOpen),
  });
  const teachers = teachersPage?.items ?? [];

  const teacherOptions = teachers.map((t) => ({
    value: String(t.id),
    label: `${t.firstName} ${t.lastName}`,
  }));

  const roomOptions = rooms.map((r) => ({
    value: String(r.id),
    label: `${r.roomNumber} (${r.building?.name ?? ""})`,
  }));

  const form = useForm({
    defaultValues: {
      teacherId: offering.teacher?.id ?? null,
      roomId: offering.room?.id ?? null,
      daysPerWeek: offering.daysPerWeek ?? 3,
      hoursPerDay: offering.hoursPerDay ?? 1.5,
      subjectUnitsOverride: offering.subjectUnitsOverride ?? null,
      maxNumberOfStudents: offering.maxNumberOfStudents ?? null,
    },
    validators: {
      onSubmit: editOfferingSchema,
    },
    onSubmit: async ({ value }) => {
      const payload: UpdateOfferingRequest = {
        teacherId: value.teacherId ?? undefined,
        roomId: value.roomId ?? undefined,
        daysPerWeek: value.daysPerWeek,
        hoursPerDay: value.hoursPerDay,
        subjectUnitsOverride: value.subjectUnitsOverride ?? undefined,
        maxNumberOfStudents: value.maxNumberOfStudents ?? undefined,
      };
      await updateOffering(payload);
      setIsOpen(false);
    },
  });

  const handleClose = () => {
    setIsOpen(false);
    form.reset();
  };

  return (
    <>
      <Button
        variant="ghost"
        size="sm"
        className="h-7 w-7 p-0"
        onClick={() => setIsOpen(true)}
        title="Edit offering"
      >
        <Pencil className="size-3.5" />
      </Button>

      <Drawer
        open={isOpen}
        onOpenChange={setIsOpen}
        direction="right"
        dismissible={false}
      >
        <DrawerContent className="data-[vaul-drawer-direction=right]:w-[480px] data-[vaul-drawer-direction=right]:sm:max-w-none h-full w-full overflow-y-auto">
          <DrawerHeader className="border-b pb-4">
            <DrawerTitle>
              Edit Offering —{" "}
              <span className="font-mono text-sm">
                {offering.snapshotSubjectCode}
              </span>
            </DrawerTitle>
          </DrawerHeader>

          <form
            className="flex flex-col gap-6 p-4"
            onSubmit={(e) => e.preventDefault()}
          >
            {/* Teacher */}
            <FormSection title="Teacher">
              <form.Field name="teacherId">
                {(field) => (
                  <div className="grid w-full items-center gap-1.5">
                    <Label>Teacher</Label>
                    <SearchableSelect
                      options={teacherOptions}
                      value={
                        field.state.value != null
                          ? String(field.state.value)
                          : ""
                      }
                      onValueChange={(val) =>
                        field.handleChange(val ? Number(val) : null)
                      }
                      placeholder="Unassigned"
                      searchPlaceholder="Search teachers..."
                      emptyMessage="No teachers found."
                    />
                  </div>
                )}
              </form.Field>
            </FormSection>

            {/* Room */}
            <FormSection title="Room">
              <form.Field name="roomId">
                {(field) => (
                  <div className="grid w-full items-center gap-1.5">
                    <Label>Room</Label>
                    <SearchableSelect
                      options={roomOptions}
                      value={
                        field.state.value != null
                          ? String(field.state.value)
                          : ""
                      }
                      onValueChange={(val) =>
                        field.handleChange(val ? Number(val) : null)
                      }
                      placeholder="No room assigned"
                      searchPlaceholder="Search rooms..."
                      emptyMessage="No rooms found."
                    />
                  </div>
                )}
              </form.Field>
            </FormSection>

            {/* Schedule Parameters */}
            <FormSection title="Schedule Parameters">
              <form.Field
                name="daysPerWeek"
                validators={{
                  onBlur: z.number().int().min(1).max(7),
                  onSubmit: z.number().int().min(1).max(7),
                }}
              >
                {(field) => (
                  <FormField
                    field={field}
                    label="Days per Week"
                    type="number"
                    min={1}
                    max={7}
                    step={1}
                    required
                    onChange={(e) =>
                      field.handleChange(
                        e.target.value === ""
                          ? 0
                          : parseInt(e.target.value, 10),
                      )
                    }
                  />
                )}
              </form.Field>
              <form.Field
                name="hoursPerDay"
                validators={{
                  onBlur: z.number().min(0.5).max(24),
                  onSubmit: z.number().min(0.5).max(24),
                }}
              >
                {(field) => (
                  <FormField
                    field={field}
                    label="Hours per Day"
                    type="number"
                    min={0.5}
                    max={24}
                    step={0.5}
                    required
                    onChange={(e) =>
                      field.handleChange(
                        e.target.value === "" ? 0 : parseFloat(e.target.value),
                      )
                    }
                  />
                )}
              </form.Field>
            </FormSection>

            {/* Units Override */}
            <FormSection title="Subject Units">
              <form.Field name="subjectUnitsOverride">
                {(field) => (
                  <div className="grid w-full items-center gap-1.5">
                    <Label htmlFor={field.name}>
                      Units Override{" "}
                      <span className="text-xs text-muted-foreground">
                        (leave empty to use curriculum default:{" "}
                        {offering.snapshotUnits})
                      </span>
                    </Label>
                    <input
                      id={field.name}
                      type="number"
                      min={0.5}
                      step={0.5}
                      className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                      placeholder="e.g. 3"
                      value={
                        field.state.value != null
                          ? String(field.state.value)
                          : ""
                      }
                      onChange={(e) =>
                        field.handleChange(
                          e.target.value === ""
                            ? null
                            : parseFloat(e.target.value),
                        )
                      }
                      onBlur={field.handleBlur}
                    />
                  </div>
                )}
              </form.Field>
            </FormSection>

            {/* Capacity */}
            <FormSection title="Capacity">
              <form.Field name="maxNumberOfStudents">
                {(field) => (
                  <div className="grid w-full items-center gap-1.5">
                    <Label htmlFor={field.name}>
                      Max Students{" "}
                      <span className="text-xs text-muted-foreground">
                        (leave empty for no limit)
                      </span>
                    </Label>
                    <input
                      id={field.name}
                      type="number"
                      min={1}
                      step={1}
                      className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                      placeholder="No limit"
                      value={
                        field.state.value != null
                          ? String(field.state.value)
                          : ""
                      }
                      onChange={(e) =>
                        field.handleChange(
                          e.target.value === ""
                            ? null
                            : parseInt(e.target.value, 10),
                        )
                      }
                      onBlur={field.handleBlur}
                    />
                  </div>
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
                  {isSubmitting ? "Saving..." : "Save Changes"}
                </Button>
              )}
            </form.Subscribe>
            <Button variant="outline" onClick={handleClose}>
              Cancel
            </Button>
          </div>
        </DrawerContent>
      </Drawer>
    </>
  );
}
