import { z } from "zod";

export const ValidationSummarySchema = z.object({
  OFFERINGS_COUNT: z.number().int().min(0),
  OFFERING_WITH_ISSUE_COUNT: z.number().int().min(0),
  OFFERING_MISSING_TEACHER_COUNT: z.number().int().min(0),
  OFFERING_NO_SCHEDULE_COUNT: z.number().int().min(0),
  OFFERING_MISSING_ROOM_COUNT: z.number().int().min(0),
});

export type ValidationSummary = z.infer<typeof ValidationSummarySchema>;
