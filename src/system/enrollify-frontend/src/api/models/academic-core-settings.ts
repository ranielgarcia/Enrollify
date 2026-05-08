import z from "zod";

export const AcademicCoreSettingsSchema = z.object({
  academicTermSystem: z.number(),
});

export type AcademicCoreSettings = z.infer<typeof AcademicCoreSettingsSchema>;
