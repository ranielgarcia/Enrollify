import { z } from "zod";

export const SectionStatsSchema = z.object({
  totalDraft: z.number().int().min(0),
  totalOpen: z.number().int().min(0),
  totalCancelled: z.number().int().min(0),
  sectionsWithUnresolvedErrors: z.number().int().min(0),
  sectionsWithConflicts: z.number().int().min(0),
  unscheduledCount: z.number().int().min(0),
});

export type SectionStats = z.infer<typeof SectionStatsSchema>;
