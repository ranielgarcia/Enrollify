import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";

export const BuildingSchema = z
  .object({
    id: z.number(),
    name: z.string(),
    description: z.string(),
    address: z.string(),
  })
  .extend(AuditInfoSchema.shape);

export type Building = z.infer<typeof BuildingSchema>;
