import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";
import { RoomTypeSchema } from "./room-type";
import { BuildingSchema } from "./building";
import { CollegeSchema } from "./college";

export const RoomSchema = z
  .object({
    id: z.number(),
    RoomNumber: z.string(),
    Capacity: z.number(),
    RoomType: RoomTypeSchema,
    Building: BuildingSchema,
    College: CollegeSchema,
  })
  .extend(AuditInfoSchema.shape);

export type Room = z.infer<typeof RoomSchema>;
