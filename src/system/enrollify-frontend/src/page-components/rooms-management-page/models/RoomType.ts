import { z } from "zod";
import { AuditInfoSchema, AuditInfoInputSchema } from "@/api/models/AuditInfo";

export const RoomTypeSchema = AuditInfoInputSchema.extend({
  id: z.number(),
  name: z.string(),
  description: z.string(),
}).transform((data) => ({
  id: data.id,
  name: data.name,
  description: data.description,
  ...AuditInfoSchema.parse(data),
}));

export type RoomType = z.infer<typeof RoomTypeSchema>;
