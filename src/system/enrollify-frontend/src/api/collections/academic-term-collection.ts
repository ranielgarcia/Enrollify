import { z } from "zod";
import createQueryOptions from "@/hooks/create-query-options";

export const AcademicTermOptionSchema = z.object({
  id: z.number(),
  displayLabel: z.string(),
  termNumber: z.number(),
  academicYearTitle: z.string(),
});
export type AcademicTermOption = z.infer<typeof AcademicTermOptionSchema>;

const queryKeys = {
  all: () => ["academic-terms"],
};

// Mock data — remove when backend exposes /api/academic-terms
const MOCK_ACADEMIC_TERMS: AcademicTermOption[] = [
  { id: 1, termNumber: 1, academicYearTitle: "2024-2025", displayLabel: "1st Semester 2024-2025" },
  { id: 2, termNumber: 2, academicYearTitle: "2024-2025", displayLabel: "2nd Semester 2024-2025" },
  { id: 3, termNumber: 3, academicYearTitle: "2024-2025", displayLabel: "Summer 2024-2025" },
  { id: 4, termNumber: 1, academicYearTitle: "2025-2026", displayLabel: "1st Semester 2025-2026" },
];

export const getAllAcademicTermsOptions = () =>
  // @ts-ignore - path will be registered in api.ts when backend is implemented
  createQueryOptions({
    path: "/api/academic-terms" as any,
    options: {
      queryKey: queryKeys.all(),
      staleTime: 1000 * 60 * 10,
      // Remove initialData when the backend is ready
      initialData: MOCK_ACADEMIC_TERMS as any,
      select: (data: any): AcademicTermOption[] => {
        if (!data || (typeof data === "string" && data === ""))
          return MOCK_ACADEMIC_TERMS;
        const parsed = typeof data === "string" ? JSON.parse(data) : data;
        return (parsed as unknown[]).map((t) => AcademicTermOptionSchema.parse(t));
      },
    },
  });
