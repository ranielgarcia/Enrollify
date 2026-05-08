import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";
import { OfferingWithSchedulesSchema } from "./offering";

const CourseSummarySchema = z.object({
  id: z.number(),
  code: z.string(),
  name: z.string(),
});

export const AcademicTermSummarySchema = z.object({
  id: z.number(),
  termNumber: z.number(),
  name: z.string(),
});

const TeacherSummarySchema = z.object({
  id: z.number(),
  firstName: z.string(),
  lastName: z.string(),
});

export const ClassSectionSchema = z
  .object({
    id: z.number(),
    name: z.string(),
    yearLevel: z.number().int().min(1).max(6),
    studentCapacity: z.number().int().min(1),
    course: CourseSummarySchema,
    academicTerm: AcademicTermSummarySchema,
    adviser: TeacherSummarySchema,
  })
  .extend(AuditInfoSchema.shape);

export const ClassSectionWithOfferingsSchema = ClassSectionSchema.extend({
  offerings: z.array(OfferingWithSchedulesSchema),
});

export type ClassSection = z.infer<typeof ClassSectionSchema>;
export type ClassSectionWithOfferings = z.infer<
  typeof ClassSectionWithOfferingsSchema
>;
