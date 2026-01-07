import z from "zod";
import { AuditInfoSchema } from "./audit-info";
import { CourseSchema } from "./course";

const CourseSummarySchema = CourseSchema.pick({
  id: true,
  name: true,
  code: true,
});

const CurriculumStatusSchema = z.object({
  value: z.number(),
  name: z.string(),
});

export const CurriculumSchema = z
  .object({
    id: z.number(),
    course: CourseSummarySchema,
    effectiveYear: z.number(),
    version: z.string(),
    status: CurriculumStatusSchema,
    description: z.string(),
    approvedDate: z.date().nullable(),
  })
  .extend(AuditInfoSchema.shape);

export type Curriculum = z.infer<typeof CurriculumSchema>;
