import { z } from "zod";
import { BasicUserInfoSchema } from "./basic-user-info";
import { dateTransformer } from "./date-transformer";

export const AuditInfoSchema = z.object({
  createdBy: BasicUserInfoSchema.nullish(),
  createdAt: dateTransformer,
  updatedBy: BasicUserInfoSchema.nullish(),
  updatedAt: dateTransformer,
  deletedBy: BasicUserInfoSchema.nullish(),
  deletedAt: dateTransformer,
  isActive: z.boolean(),
});

export type AuditInfo = z.infer<typeof AuditInfoSchema>;
