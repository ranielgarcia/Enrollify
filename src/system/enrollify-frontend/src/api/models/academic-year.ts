import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";

export const AcademicTermSchema = z.object({
  id: z.number(),
  termNumber: z.number(),
  termName: z.string(),
  academicYearId: z.number(),
  startDate: z.string(),
  endDate: z.string(),
});

export const AcademicYearSchema = z
  .object({
    id: z.number(),
    startDate: z.string().nullish(),
    endDate: z.string().nullish(),
    startYear: z.number(),
    endYear: z.number(),
    academicYearTitle: z.string(),
    academicYearSlug: z.string(),
    academicTerms: z.array(AcademicTermSchema).optional(),
  })
  .extend(AuditInfoSchema.shape);

export const AcademicYearTimelineSchema = z.object({
  previous: z.array(AcademicYearSchema).optional(),
  current: AcademicYearSchema.nullable().optional(),
  future: z.array(AcademicYearSchema).optional(),
});

export type AcademicYear = z.infer<typeof AcademicYearSchema>;
export type AcademicTerm = z.infer<typeof AcademicTermSchema>;
export type AcademicYearTimeline = z.infer<typeof AcademicYearTimelineSchema>;
