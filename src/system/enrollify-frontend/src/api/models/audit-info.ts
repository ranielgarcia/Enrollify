import { z } from "zod";
import { BasicUserInfoSchema } from "./basic-user-info";
import { dateTransformer } from "./date-transformer";

export const AuditInfoSchema = z.object({
  createdByUser: BasicUserInfoSchema.nullish(),
  createdAt: dateTransformer,
  updatedByUser: BasicUserInfoSchema.nullish(),
  updatedAt: dateTransformer,
  deletedByUser: BasicUserInfoSchema.nullish(),
  deletedAt: dateTransformer,
  isActive: z.boolean(),
});

export type AuditInfo = z.infer<typeof AuditInfoSchema>;
