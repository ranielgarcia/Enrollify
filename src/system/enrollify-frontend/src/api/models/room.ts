import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";
import { RoomTypeSchema } from "./room-type";
import { BuildingSchema } from "./building";

const RoomTypeSummarySchema = RoomTypeSchema.pick({
  id: true,
  name: true,
});

const BuildingSummarySchema = BuildingSchema.pick({
  id: true,
  name: true,
});

export const RoomSchema = z
  .object({
    id: z.number(),
    roomNumber: z.string(),
    capacity: z.number(),
    roomType: RoomTypeSummarySchema,
    building: BuildingSummarySchema,
  })
  .extend(AuditInfoSchema.shape);

export type Room = z.infer<typeof RoomSchema>;
