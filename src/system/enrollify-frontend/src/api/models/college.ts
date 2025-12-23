import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";

export const CollegeSchema = z
  .object({
    id: z.number(),
    name: z.string(),
    description: z.string(),
    code: z.string(),
    dean: z.string(),
  })
  .extend(AuditInfoSchema.shape);

export type College = z.infer<typeof CollegeSchema>;
