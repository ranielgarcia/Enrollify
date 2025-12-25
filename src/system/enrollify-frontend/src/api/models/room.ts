import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";
import { RoomTypeSchema } from "./room-type";
import { BuildingSchema } from "./building";
import { CollegeSchema } from "./college";

const RoomTypeSummarySchema = RoomTypeSchema.pick({
  id: true,
  name: true,
});

const BuildingSummarySchema = BuildingSchema.pick({
  id: true,
  name: true,
});

const CollegeSummarySchema = CollegeSchema.pick({
  id: true,
  name: true,
});
export const RoomSchema = z
  .object({
    id: z.number(),
    RoomNumber: z.string(),
    Capacity: z.number(),
    RoomType: RoomTypeSummarySchema,
    Building: BuildingSummarySchema,
    College: CollegeSummarySchema,
  })
  .extend(AuditInfoSchema.shape);

export type Room = z.infer<typeof RoomSchema>;
