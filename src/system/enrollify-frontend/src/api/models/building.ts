import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";
import { CollegeSchema } from "./college";

const CollegeSummarySchema = CollegeSchema.pick({
  id: true,
  name: true,
});

export const BuildingSchema = z
  .object({
    id: z.number(),
    name: z.string(),
    description: z.string(),
    address: z.string(),
    college: CollegeSummarySchema,
  })
  .extend(AuditInfoSchema.shape);

export type Building = z.infer<typeof BuildingSchema>;
