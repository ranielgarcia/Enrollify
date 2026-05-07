import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";

export const AcademicTermSchema = z.object({
  id: z.number(),
  termNumber: z.number().optional(),
  termName: z.string().optional(),
  academicYearId: z.number().optional(),
  startDate: z.string().nullish(),
  endDate: z.string().nullish(),
});

export const AcademicYearSchema = z
  .object({
    id: z.number(),
    startDate: z.string().nullish(),
    endDate: z.string().nullish(),
    startYear: z.number().optional(),
    endYear: z.number().optional(),
    academicYearTitle: z.string().optional(),
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
