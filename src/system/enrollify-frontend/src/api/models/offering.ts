import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";
import { ClassScheduleSchema } from "./class-schedule";

const SubjectSummarySchema = z.object({
  id: z.number(),
  code: z.string(),
  title: z.string(),
  units: z.number(),
});

const TeacherSummarySchema = z.object({
  id: z.number(),
  firstName: z.string(),
  lastName: z.string(),
});

const RoomSummarySchema = z.object({
  id: z.number(),
  roomNumber: z.string(),
  building: z.object({ id: z.number(), name: z.string() }),
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
    subject: SubjectSummarySchema,
    teacher: TeacherSummarySchema,
    room: RoomSummarySchema,
    dayPattern: z.string().nullable(),
    daysPerWeek: z.number().nullable(),
    hoursPerDay: z.number().nullable(),
    maxNumberOfStudents: z.number().nullable(),
  })
  .extend(AuditInfoSchema.shape);

export const OfferingWithSchedulesSchema = OfferingSchema.extend({
  schedules: z.array(ClassScheduleSchema),
  conflicts: z.array(ConflictResultSchema).optional(),
  snapshotSubjectCode: z.string().optional(),
  snapshotSubjectTitle: z.string().optional(),
  snapshotUnits: z.number().optional(),
  snapshotIsElective: z.boolean().optional(),
  snapshotElectiveGroupName: z.string().nullable().optional(),
});

export type Offering = z.infer<typeof OfferingSchema>;
export type OfferingWithSchedules = z.infer<typeof OfferingWithSchedulesSchema>;
