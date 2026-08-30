import { z } from "zod";

/**
 * Read models for the room-schedule grid (GET /api/rooms/schedule).
 * These mirror the backend RoomScheduleDto family. Because the endpoint returns
 * OkOrNotFoundApiResult<RoomScheduleDto>, NSwag does not emit a typed 200 body,
 * so the response shape is defined and validated here via Zod.
 */

export const RoomScheduleConflictSchema = z.object({
  offeringId: z.number(),
  sectionId: z.number(),
  sectionName: z.string(),
  subjectCode: z.string(),
  teacherName: z.string().nullable().optional(),
  startTime: z.string(),
  endTime: z.string(),
});
export type RoomScheduleConflict = z.infer<typeof RoomScheduleConflictSchema>;

export const RoomScheduleOfferingSchema = z.object({
  offeringId: z.number(),
  scheduleId: z.number(),
  sectionId: z.number(),
  sectionName: z.string(),
  courseId: z.number(),
  courseCode: z.string(),
  subjectId: z.number(),
  subjectCode: z.string(),
  subjectTitle: z.string(),
  teacherId: z.number().nullable().optional(),
  teacherName: z.string().nullable().optional(),
  roomId: z.number().nullable().optional(),
  dayOfWeek: z.string(),
  startTime: z.string(),
  endTime: z.string(),
  conflicts: z.array(RoomScheduleConflictSchema).default([]),
  hasConflict: z.boolean().default(false),
});
export type RoomScheduleOffering = z.infer<typeof RoomScheduleOfferingSchema>;

export const RoomScheduleRoomSchema = z.object({
  id: z.number(),
  roomNumber: z.string(),
  capacity: z.number(),
  roomTypeName: z.string().nullable().optional(),
  buildingId: z.number().nullable().optional(),
  buildingName: z.string().nullable().optional(),
  collegeId: z.number().nullable().optional(),
  collegeCode: z.string().nullable().optional(),
  offerings: z.array(RoomScheduleOfferingSchema).default([]),
  hasConflicts: z.boolean().default(false),
});
export type RoomScheduleRoom = z.infer<typeof RoomScheduleRoomSchema>;

export const RoomScheduleStatsSchema = z.object({
  totalRooms: z.number(),
  roomsInUse: z.number(),
  totalOfferings: z.number(),
  unassignedOfferings: z.number(),
  totalConflicts: z.number(),
});
export type RoomScheduleStats = z.infer<typeof RoomScheduleStatsSchema>;

export const RoomScheduleSchema = z.object({
  academicTermId: z.number(),
  dayOfWeek: z.string(),
  rooms: z.array(RoomScheduleRoomSchema).default([]),
  unassignedOfferings: z.array(RoomScheduleOfferingSchema).default([]),
  stats: RoomScheduleStatsSchema,
});
export type RoomSchedule = z.infer<typeof RoomScheduleSchema>;
