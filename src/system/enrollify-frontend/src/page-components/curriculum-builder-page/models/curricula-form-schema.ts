import z from "zod";

export const curriculaFormSchema = z.object({
  courseId: z.number().min(1, "Course is required"),
  effectiveYear: z
    .number()
    .min(2020, "Effective Year must be a valid year")
    .max(
      new Date().getFullYear() + 1,
      "Effective Year cannot be in the distant future"
    ),
  version: z.string().min(1, "Version Identifier is required"),
  description: z.string().optional(),
});

export type CurriculaFormData = z.infer<typeof curriculaFormSchema>;
