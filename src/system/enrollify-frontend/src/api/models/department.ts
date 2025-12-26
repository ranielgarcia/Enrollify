import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";
import { CollegeSchema } from "./college";

const CollegeSummarySchema = CollegeSchema.pick({
  id: true,
  name: true,
});

export const DepartmentSchema = z
  .object({
    id: z.number(),
    code: z.string(),
    name: z.string(),
    chairperson: z.string(),
    description: z.string(),
    college: CollegeSummarySchema,
  })
  .extend(AuditInfoSchema.shape);

export type Department = z.infer<typeof DepartmentSchema>;
