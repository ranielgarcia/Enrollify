import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";

export const RoomTypeSchema = z
  .object({
    id: z.number(),
    name: z.string(),
    description: z.string(),
  })
  .extend(AuditInfoSchema.shape);

export type RoomType = z.infer<typeof RoomTypeSchema>;
