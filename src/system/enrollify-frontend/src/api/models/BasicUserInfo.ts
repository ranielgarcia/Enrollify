import { z } from "zod";

export const BasicUserInfoSchema = z.object({
  id: z.number().optional(),
  email: z.string().optional(),
  firstName: z.string().optional(),
  lastName: z.string().optional(),
});

export type BasicUserInfo = z.infer<typeof BasicUserInfoSchema>;
