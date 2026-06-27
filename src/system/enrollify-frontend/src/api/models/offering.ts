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

const CONFLICT_TYPES = [
  "TEACHER_DOUBLE_BOOKED",
  "ROOM_DOUBLE_BOOKED",
  "SECTION_OVERLAP",
  "DUPLICATE_DAY_IN_OFFERING",
  "TEACHER_OVERLOAD",
  "ROOM_CAPACITY_EXCEEDED",
  "TEACHER_NO_BREAK",
  "ADVISER_AS_TEACHER",
  "YEAR_LEVEL_MISMATCH",
  "OUTSIDE_OPERATING_HOURS",
  "SCHEDULE_COUNT_MISMATCH",
  "HOURS_MISMATCH",
  "NO_SCHEDULES",
  "NO_OFFERINGS",
  "ADVISER_NOT_ASSIGNED",
  "TEACHER_NOT_ASSIGNED",
  "ROOM_NOT_ASSIGNED",
  "DUPLICATE_SUBJECT_IN_SECTION",
  "ROOM_TYPE_MISMATCH",
  "CROSS_TERM_BOOKING",
  "DAYS_PER_WEEK_DEFAULT",
  "HOURS_PER_DAY_DEFAULT",
  "MAX_STUDENTS_AT_DEFAULT",
] as const;

export type ConflictType = (typeof CONFLICT_TYPES)[number];

const TYPE_SEVERITY: Record<ConflictType, "Error" | "Warning" | "Info"> = {
  TEACHER_DOUBLE_BOOKED: "Error",
  ROOM_DOUBLE_BOOKED: "Error",
  SECTION_OVERLAP: "Error",
  ADVISER_NOT_ASSIGNED: "Error",
  TEACHER_NOT_ASSIGNED: "Error",
  ROOM_NOT_ASSIGNED: "Error",
  NO_SCHEDULES: "Error",
  NO_OFFERINGS: "Error",
  TEACHER_OVERLOAD: "Warning",
  ROOM_CAPACITY_EXCEEDED: "Warning",
  DUPLICATE_DAY_IN_OFFERING: "Warning",
  TEACHER_NO_BREAK: "Warning",
  OUTSIDE_OPERATING_HOURS: "Warning",
  CROSS_TERM_BOOKING: "Warning",
  ROOM_TYPE_MISMATCH: "Warning",
  YEAR_LEVEL_MISMATCH: "Warning",
  ADVISER_AS_TEACHER: "Warning",
  SCHEDULE_COUNT_MISMATCH: "Warning",
  HOURS_MISMATCH: "Warning",
  DUPLICATE_SUBJECT_IN_SECTION: "Warning",
  DAYS_PER_WEEK_DEFAULT: "Info",
  HOURS_PER_DAY_DEFAULT: "Info",
  MAX_STUDENTS_AT_DEFAULT: "Info",
};

export const TYPE_LABELS: Record<ConflictType, string> = {
  TEACHER_DOUBLE_BOOKED: "Teacher Double-Booked",
  ROOM_DOUBLE_BOOKED: "Room Double-Booked",
  SECTION_OVERLAP: "Section Overlap",
  TEACHER_OVERLOAD: "Teacher Overload",
  ROOM_CAPACITY_EXCEEDED: "Room Capacity Exceeded",
  DUPLICATE_DAY_IN_OFFERING: "Duplicate Day in Offering",
  TEACHER_NO_BREAK: "Teacher No Break",
  OUTSIDE_OPERATING_HOURS: "Outside Operating Hours",
  CROSS_TERM_BOOKING: "Cross-Term Booking",
  ROOM_TYPE_MISMATCH: "Room Type Mismatch",
  YEAR_LEVEL_MISMATCH: "Year Level Mismatch",
  ADVISER_AS_TEACHER: "Adviser as Teacher",
  SCHEDULE_COUNT_MISMATCH: "Schedule Count Mismatch",
  HOURS_MISMATCH: "Hours Mismatch",
  DUPLICATE_SUBJECT_IN_SECTION: "Duplicate Subject in Section",
  ADVISER_NOT_ASSIGNED: "Adviser Not Assigned",
  TEACHER_NOT_ASSIGNED: "Teacher Not Assigned",
  ROOM_NOT_ASSIGNED: "Room Not Assigned",
  NO_SCHEDULES: "No Schedules",
  NO_OFFERINGS: "No Offerings",
  DAYS_PER_WEEK_DEFAULT: "Days Per Week Default",
  HOURS_PER_DAY_DEFAULT: "Hours Per Day Default",
  MAX_STUDENTS_AT_DEFAULT: "Max Students Default",
};

export const ConflictResultSchema = z
  .object({
    id: z.string().nullable().optional(),
    type: z.enum(CONFLICT_TYPES),
    severity: z.enum(["Error", "Warning", "Info"]).optional(),
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
  })
  .transform((data) => ({
    ...data,
    severity: (data.severity ?? TYPE_SEVERITY[data.type]) as "Error" | "Warning" | "Info",
  }));

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
