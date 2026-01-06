import z from "zod";
import { AuditInfoSchema } from "./audit-info";
import { CourseSchema } from "./course";

const CourseSummarySchema = CourseSchema.pick({
  id: true,
  name: true,
  code: true,
});

const CurriculaStatusSchema = z.object({
  value: z.number(),
  name: z.string(),
});

export const CurriculaSchema = z
  .object({
    id: z.number(),
    course: CourseSummarySchema,
    effectiveYear: z.number(),
    version: z.string(),
    status: CurriculaStatusSchema,
    description: z.string(),
    approvedDate: z.date().nullable(),
  })
  .extend(AuditInfoSchema.shape);

export type Curricula = z.infer<typeof CurriculaSchema>;
