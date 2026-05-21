import z from "zod";

export const YearLevelOption = z.object({
  value: z.number(),
  label: z.string(),
});

export const AcademicCoreSettingsSchema = z.object({
  academicTermSystem: z.number(),
  maximumAllowableYearLevel: z.number(),
  yearLevelOptions: z.array(YearLevelOption),
});

export type AcademicCoreSettings = z.infer<typeof AcademicCoreSettingsSchema>;
