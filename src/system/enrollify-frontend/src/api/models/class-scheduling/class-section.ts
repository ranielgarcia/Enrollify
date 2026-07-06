import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";
import { OfferingSchema } from "./offering";
import { dateTransformer } from "../date-transformer";
import {
  ValidationSeverities,
  type ValidationSeverityName,
} from "../validation-severity";
import { ValidationSummarySchema } from "../validation-summary";

const SeveritySchema = z.object({
  value: z.enum(ValidationSeverities),
  name: z.enum(
    Object.keys(ValidationSeverities) as [
      ValidationSeverityName,
      ...ValidationSeverityName[],
    ],
  ),
});

const ValidationMessageSchema = z.object({
  severity: SeveritySchema,
  code: z.string(),
  message: z.string(),
  computedAt: dateTransformer,
});

const OfferingValidationMessageSchema = ValidationMessageSchema.extend({
  offeringId: z.number(),
});

export type OfferingValidationMessage = z.infer<
  typeof OfferingValidationMessageSchema
>;

const ValidationMessagesSchema = z.object({
  classSectionId: z.number(),
  classSectionValidationMessages: z.array(ValidationMessageSchema),
  offeringsValidationMessages: z.record(
    z.string(), // class section subject offering id as string
    z.array(OfferingValidationMessageSchema),
  ),
});

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
    fullName: z.string(),
    course: CourseSummarySchema,
    curriculum: CurriculumSummarySchema.optional().nullable(),
    academicTerm: AcademicTermSummarySchema,
    cohortAcademicYear: CohortAcademicYearSchema.optional().nullable(),
    adviser: AdviserSummarySchema.optional().nullable(),
    status: ClassSectionStatusSchema,
    validationSummary: ValidationSummarySchema.optional().nullable(),
  })
  .extend(AuditInfoSchema.shape);

export const ClassSectionWithOfferingsSchema = ClassSectionSchema.extend({
  offerings: z.array(OfferingSchema),
  validationMessages: ValidationMessagesSchema.optional().nullable(),
});

export type ClassSection = z.infer<typeof ClassSectionSchema>;
export type ClassSectionWithOfferings = z.infer<
  typeof ClassSectionWithOfferingsSchema
>;
export type ValidationMessage = z.infer<typeof ValidationMessageSchema>;
export type ValidationMessages = z.infer<typeof ValidationMessagesSchema>;

// -----------------------------------------

export const ClassSectionMinimalSchema = z.object({
  id: z.number(),
  name: z.string(),
  sectionCode: z.string().max(1).optional(),
  intendedYearLevel: z.number().int().min(1).max(6),
  fullName: z.string(),
  courseId: z.number(),
  adviser: AdviserSummarySchema.optional().nullable(),
  status: ClassSectionStatusSchema,
  validationSummary: ValidationSummarySchema.optional().nullable(),
});

export type ClassSectionMinimal = z.infer<typeof ClassSectionMinimalSchema>;

const CourseWithClassSectionsSchema = z.object({
  id: z.number(),
  code: z.string(),
  name: z.string(),
  classSections: z.array(ClassSectionMinimalSchema).optional().nullable(),
});

export type CourseWithClassSections = z.infer<
  typeof CourseWithClassSectionsSchema
>;

export const CollegeCoursesWithClassSectionsSchema = z.object({
  id: z.number(),
  name: z.string(),
  description: z.string(),
  code: z.string(),
  coursesWithClassSections: z.array(CourseWithClassSectionsSchema),
});

export type CollegeCoursesWithClassSections = z.infer<
  typeof CollegeCoursesWithClassSectionsSchema
>;

export const BulkStateChangeClassSectionsResultSchema = z.object({
  status: z.enum(["Success", "PartialSuccess", "Failed"]),
  totalRequested: z.number(),
  succeeded: z.number(),
  failed: z.number(),
  errors: z.record(z.string(), z.string()),
});

export type BulkStateChangeClassSectionsResult = z.infer<
  typeof BulkStateChangeClassSectionsResultSchema
>;
