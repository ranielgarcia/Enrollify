import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";
import { ValidationSummarySchema } from "./validation-summary";
import { ClassSectionStatusSchema } from "./class-section";

const CourseSummarySchema = z.object({
  id: z.number(),
  code: z.string(),
  name: z.string(),
});

const AdviserSummarySchema = z.object({
  id: z.number(),
  firstName: z.string(),
  lastName: z.string(),
  email: z.string().optional(),
});

const AcademicTermSummarySchema = z.object({
  id: z.number(),
  termNumber: z.number(),
  termName: z.string(),
});

const CurriculumSummarySchema = z.object({
  id: z.number(),
  version: z.string(),
});

const CohortAcademicYearSchema = z.object({
  id: z.number(),
  academicYearTitle: z.string(),
});

export const ClassSectionV2Schema = z
  .object({
    id: z.number(),
    name: z.string(),
    sectionCode: z.string().max(1).optional(),
    intendedYearLevel: z.number().int().min(1).max(6),
    fullName: z.string(),
    course: CourseSummarySchema,
    curriculum: CurriculumSummarySchema.optional().nullable(),
    academicTerm: AcademicTermSummarySchema,
    cohortAcademicYear: CohortAcademicYearSchema.optional().nullable(),
    adviser: AdviserSummarySchema.optional().nullable(),
    status: ClassSectionStatusSchema,
    unresolvedErrorsCount: z.number().int().min(0),
    isEligibleForOpenEnrollment: z.boolean(),
    validationSummary: ValidationSummarySchema,
  })
  .extend(AuditInfoSchema.shape);

export type ClassSectionV2 = z.infer<typeof ClassSectionV2Schema>;
