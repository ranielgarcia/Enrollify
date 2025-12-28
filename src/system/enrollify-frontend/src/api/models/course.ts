import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";
import { CollegeSchema } from "./college";

const CollegeSummarySchema = CollegeSchema.pick({
  id: true,
  name: true,
});

export const CourseSchema = z
  .object({
    id: z.number(),
    code: z.string(),
    name: z.string(),
    durationYears: z.number(),
    description: z.string(),
    college: CollegeSummarySchema.nullable(),
  })
  .extend(AuditInfoSchema.shape);

export type Course = z.infer<typeof CourseSchema>;
