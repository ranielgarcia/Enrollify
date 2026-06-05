import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";
import { OfferingSchema } from "./offering";

const CourseSummarySchema = z.object({
  id: z.number(),
  code: z.string(),
  name: z.string(),
});

export const AcademicTermSummarySchema = z.object({
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

const AdviserSummarySchema = z.object({
  id: z.number(),
  firstName: z.string(),
  lastName: z.string(),
  email: z.string().optional(),
});

export const ClassSectionStatusEnum = {
  Draft: 1,
  Open: 2,
  Locked: 3,
  Active: 4,
  Completed: 5,
  Cancelled: 6,
} as const;

export type ClassSectionStatusValue =
  (typeof ClassSectionStatusEnum)[keyof typeof ClassSectionStatusEnum];

const ClassSectionStatusSchema = z.object({
  name: z.enum(["Draft", "Open", "Locked", "Active", "Completed", "Cancelled"]),
  value: z.number(),
  description: z.string().optional(),
});

export type ClassSectionStatus = z.infer<typeof ClassSectionStatusSchema>;

export const ClassSectionSchema = z
  .object({
    id: z.number(),
    name: z.string(),
    sectionCode: z.string().max(1).optional(),
    intendedYearLevel: z.number().int().min(1).max(6),
    course: CourseSummarySchema,
    curriculum: CurriculumSummarySchema.optional().nullable(),
    academicTerm: AcademicTermSummarySchema,
    cohortAcademicYear: CohortAcademicYearSchema.optional().nullable(),
    adviser: AdviserSummarySchema.optional().nullable(),
    status: ClassSectionStatusSchema,
    unresolvedErrorsCount: z.number().int().min(0),
    isEligibleForOpenEnrollment: z.boolean(),
  })
  .extend(AuditInfoSchema.shape);


  
export const ClassSectionWithOfferingsSchema = ClassSectionSchema.extend({
  offerings: z.array(OfferingSchema),
});

export type ClassSection = z.infer<typeof ClassSectionSchema>;
export type ClassSectionWithOfferings = z.infer<
  typeof ClassSectionWithOfferingsSchema
>;
