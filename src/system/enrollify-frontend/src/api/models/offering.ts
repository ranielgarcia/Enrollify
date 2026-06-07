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
  id: z.string().nullable().optional(),
  type: z.enum([
    "TEACHER_DOUBLE_BOOKED",
    "ROOM_DOUBLE_BOOKED",
    "SECTION_OVERLAP",
    "TEACHER_OVERLOAD",
    "DUPLICATE_SUBJECT_IN_SECTION",
  ]),
  severity: z.enum(["Error", "Warning", "Info"]),
  message: z.string(),
  day: z.string().nullable().optional(),
  startTime: z.string().nullable().optional(),
  endTime: z.string().nullable().optional(),
  affectedOfferings: z
    .array(
      z.object({
        id: z.number().nullable().optional(),
        subject: z.object({ code: z.string(), title: z.string() }),
        section: z.object({ id: z.number(), name: z.string() }),
        room: z
          .object({ roomNumber: z.string(), building: z.string() })
          .nullable()
          .optional(),
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
