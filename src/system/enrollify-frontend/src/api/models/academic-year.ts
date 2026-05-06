import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";

export const AcademicTermSchema = z.object({
  id: z.number().optional(),
  termNumber: z.number().optional(),
  termName: z.string().optional(),
  academicYearId: z.number().optional(),
  startDate: z.string().nullish(),
  endDate: z.string().nullish(),
});

export const AcademicYearSchema = z
  .object({
    id: z.number().optional(),
    startDate: z.string().nullish(),
    endDate: z.string().nullish(),
    startYear: z.number().optional(),
    endYear: z.number().optional(),
    academicYearTitle: z.string().optional(),
    academicTerms: z.array(AcademicTermSchema).optional(),
  })
  .extend(AuditInfoSchema.shape);

export type AcademicYear = z.infer<typeof AcademicYearSchema>;
export type AcademicTerm = z.infer<typeof AcademicTermSchema>;
