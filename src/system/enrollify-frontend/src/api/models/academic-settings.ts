import z from "zod";

export const AcademicSettingsSchema = z.object({
  academicSystem: z.number(),
});

export type AcademicSettings = z.infer<typeof AcademicSettingsSchema>;
