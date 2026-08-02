import { createAppSuspenseQueryOptions } from "@/hooks/create-query-options";
import {
  RoomScheduleSchema,
  type RoomSchedule,
} from "@/api/models/class-scheduling/room-schedule";

export interface RoomScheduleFilters {
  academicTermId: number;
  dayOfWeek: string;
  buildingId?: number | null;
  roomTypeId?: number | null;
  collegeId?: number | null;
  courseId?: number | null;
}

export const roomScheduleQueryKeys = {
  all: () => ["room-schedule"] as const,
  forFilters: (filters: RoomScheduleFilters) =>
    [...roomScheduleQueryKeys.all(), filters] as const,
};

/**
 * Suspense query options for the room-schedule grid.
 * The response body is not strongly typed by the generated client
 * (OkOrNotFoundApiResult<T> emits no 200 schema), so the payload is validated
 * and typed through RoomScheduleSchema in the select callback.
 */
export const getRoomScheduleOptions = (filters: RoomScheduleFilters) =>
  createAppSuspenseQueryOptions({
    path: "/api/rooms/schedule",
    params: {
      AcademicTermId: filters.academicTermId,
      DayOfWeek: filters.dayOfWeek,
      BuildingId: filters.buildingId ?? undefined,
      RoomTypeId: filters.roomTypeId ?? undefined,
      CollegeId: filters.collegeId ?? undefined,
      CourseId: filters.courseId ?? undefined,
    },
    options: {
      queryKey: roomScheduleQueryKeys.forFilters(filters),
      staleTime: 1000 * 30,
      select: (data): RoomSchedule => RoomScheduleSchema.parse(data),
    },
  });
