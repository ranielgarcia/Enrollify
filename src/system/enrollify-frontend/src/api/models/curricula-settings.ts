import z from "zod";
import {
  AcademicSystemEnum,
  type AcademicSystem,
} from "./academic-system-enum";

const academicSystemNames = Object.keys(AcademicSystemEnum) as [
  string,
  ...string[],
];
const academicSystemValues = Object.values(AcademicSystemEnum) as [
  number,
  ...number[],
];

const AcademicSystemSchema = z
  .object({
    value: z
      .enum(academicSystemValues.map(String) as [string, ...string[]])
      .transform(Number)
      .pipe(z.number()),
    name: z.enum(academicSystemNames),
  })
  .refine(
    (data) =>
      AcademicSystemEnum[data.name as keyof typeof AcademicSystemEnum] ===
      data.value,
    { message: "Academic system name and value do not match" }
  ) as z.ZodType<AcademicSystem>;

export const CurriculaSettingsSchema = z.object({
  academicSystem: AcademicSystemSchema,
});

export type CurriculaSettings = z.infer<typeof CurriculaSettingsSchema>;
