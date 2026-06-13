import { z } from "zod";

export const ValidationSummarySchema = z.object({
  totalOfferings: z.number().int().min(0),
  offeringsWithErrors: z.number().int().min(0),
  offeringsWithConflicts: z.number().int().min(0),
  missingTeacherCount: z.number().int().min(0),
  missingRoomCount: z.number().int().min(0),
  missingScheduleCount: z.number().int().min(0),
});

export type ValidationSummary = z.infer<typeof ValidationSummarySchema>;
