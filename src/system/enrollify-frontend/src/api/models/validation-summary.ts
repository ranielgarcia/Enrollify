import { z } from "zod";

// Make them optional and nullable so that we can handle cases where the API returns null or undefined for these fields.
// This is important for robust error handling and to avoid runtime errors when accessing these properties.
export const ValidationSummarySchema = z.object({
  OFFERINGS_COUNT: z.number().int().min(0).optional().nullable(),
  OFFERING_WITH_ISSUE_COUNT: z.number().int().min(0).optional().nullable(),
  OFFERING_MISSING_TEACHER_COUNT: z.number().int().min(0).optional().nullable(),
  OFFERING_NO_SCHEDULE_COUNT: z.number().int().min(0).optional().nullable(),
  OFFERING_MISSING_ROOM_COUNT: z.number().int().min(0).optional().nullable(),
  TOTAL_VALIDATION_ISSUES_ACROSS_OFFERINGS_COUNT: z
    .number()
    .int()
    .min(0)
    .optional()
    .nullable(),
});

export type ValidationSummary = z.infer<typeof ValidationSummarySchema>;
