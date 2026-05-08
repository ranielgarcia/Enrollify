import { z } from "zod";

export const DayOfWeekEnum = z.enum([
  "MON",
  "TUE",
  "WED",
  "THU",
  "FRI",
  "SAT",
  "SUN",
]);
export type DayOfWeek = z.infer<typeof DayOfWeekEnum>;

export const ClassScheduleSchema = z.object({
  id: z.number(),
  classSectionSubjectOfferingId: z.number(),
  dayOfWeek: DayOfWeekEnum,
  startTime: z.string().regex(/^\d{2}:\d{2}$/),
  endTime: z.string().regex(/^\d{2}:\d{2}$/),
  isActive: z.boolean(),
});

export type ClassSchedule = z.infer<typeof ClassScheduleSchema>;
