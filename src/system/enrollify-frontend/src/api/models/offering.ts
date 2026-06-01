import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";
import { ClassScheduleSchema } from "./class-schedule";

const TeacherSummarySchema = z.object({
  id: z.number(),
  firstName: z.string(),
  lastName: z.string(),
  email: z.string().optional(),
});

const RoomSummarySchema = z.object({
  id: z.number(),
  roomNumber: z.string(),
  building: z.object({
    name: z.string(),
  }),
});

export const ConflictResultSchema = z.object({
  id: z.string().optional(),
  type: z.enum([
    "TEACHER_DOUBLE_BOOKED",
    "ROOM_DOUBLE_BOOKED",
    "SECTION_OVERLAP",
    "TEACHER_OVERLOAD",
  ]),
  severity: z.enum(["error", "warning", "info"]),
  message: z.string(),
  day: z.string().optional(),
  startTime: z.string().optional(),
  endTime: z.string().optional(),
  affectedOfferings: z
    .array(
      z.object({
        id: z.number(),
        subject: z.object({ code: z.string(), title: z.string() }),
        section: z.object({ name: z.string() }),
      }),
    )
    .optional(),
});

export type ConflictResult = z.infer<typeof ConflictResultSchema>;

export const OfferingSchema = z
  .object({
    id: z.number(),
    classSectionId: z.number(),
    subjectId: z.number(),
    snapshotSubjectCode: z.string().optional(),
    snapshotSubjectTitle: z.string().optional(),
    snapshotUnits: z.number().optional(),
    snapshotIsElective: z.boolean().optional(),
    snapshotElectiveGroupName: z.string().nullable().optional(),
    subjectUnitsOverride: z.number().nullable().optional(),
    effectiveUnits: z.number().optional(),
    daysPerWeek: z.number().optional(),
    hoursPerDay: z.number().optional(),
    maxNumberOfStudents: z.number().nullable().optional(),
    isFullyScheduled: z.boolean().optional(),
    teacher: TeacherSummarySchema.nullable().optional(),
    room: RoomSummarySchema.nullable().optional(),
    schedules: z.array(ClassScheduleSchema).optional(),
    conflicts: z.array(ConflictResultSchema).optional(),
  })
  .extend(AuditInfoSchema.shape);

export type Offering = z.infer<typeof OfferingSchema>;
export type OfferingWithSchedules = Offering;
