import { z } from "zod";
import { RoomTypeSchema } from "./room-type";
import { AuditInfoSchema } from "@/api/models/audit-info";

const RoomTypeSummarySchema = RoomTypeSchema.pick({
  id: true,
  name: true,
});

export const SubjectSchema = z
  .object({
    id: z.number(),
    code: z.string(),
    title: z.string(),
    units: z.number(),
    description: z.string(),
    preferRoomType: RoomTypeSummarySchema,
  })
  .extend(AuditInfoSchema.shape);

export type Subject = z.infer<typeof SubjectSchema>;
