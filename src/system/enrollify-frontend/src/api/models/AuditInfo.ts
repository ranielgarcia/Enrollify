import { z } from "zod";
import { BasicUserInfoSchema } from "./BasicUserInfo";
import { parseDateTime } from "@/lib/dateutils";

const dateTransformer = z
  .string()
  .nullish()
  .transform((val) => parseDateTime(val));

export const AuditInfoInputSchema = z.object({
  createdByUser: BasicUserInfoSchema.nullish(),
  createdAt: z.string().nullish(),
  updatedByUser: BasicUserInfoSchema.nullish(),
  updatedAt: z.string().nullish(),
  deletedByUser: BasicUserInfoSchema.nullish(),
  deletedAt: z.string().nullish(),
  isActive: z.boolean(),
});

export const AuditInfoSchema = z
  .object({
    createdByUser: BasicUserInfoSchema.nullish(),
    createdAt: dateTransformer,
    updatedByUser: BasicUserInfoSchema.nullish(),
    updatedAt: dateTransformer,
    deletedByUser: BasicUserInfoSchema.nullish(),
    deletedAt: dateTransformer,
    isActive: z.boolean(),
  })
  .transform((data) => ({
    createdBy: data.createdByUser,
    createdAt: data.createdAt,
    updatedBy: data.updatedByUser,
    updatedAt: data.updatedAt,
    deletedBy: data.deletedByUser,
    deletedAt: data.deletedAt,
    isActive: data.isActive,
  }));

export type AuditInfo = z.infer<typeof AuditInfoSchema>;
