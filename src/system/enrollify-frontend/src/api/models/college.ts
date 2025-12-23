import { z } from "zod";
import { AuditInfoSchema, AuditInfoInputSchema } from "@/api/models/audit-info";

export const CollegeSchema = AuditInfoInputSchema.extend({
  id: z.number(),
  name: z.string(),
  description: z.string(),
  code: z.string(),
  dean: z.string(),
}).transform((data) => ({
  id: data.id,
  name: data.name,
  description: data.description,
  code: data.code,
  dean: data.dean,
  ...AuditInfoSchema.parse(data),
}));

export type College = z.infer<typeof CollegeSchema>;
