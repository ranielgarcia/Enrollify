import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";
import { SubjectSummarySchema } from "./subject";

const TeacherDepartmentSchema = z.object({
  id: z.number().optional(),
  code: z.string().optional(),
  name: z.string().optional(),
});

export const TeacherSchema = z
  .object({
    id: z.number(),
    firstName: z.string(),
    lastName: z.string(),
    middleName: z.string().default(""),
    teacherIdentifier: z.string(),
    email: z.string(),
    phoneNumber: z.string(),
    department: TeacherDepartmentSchema.optional(),
    academicTitle: z.string().nullable().optional(),
    qualification: z.string().nullable().optional(),
    specialization: z.string().nullable().optional(),
    officeLocation: z.string().nullable().optional(),
    officeHours: z.string().nullable().optional(),
    biography: z.string().nullable().optional(),
    subjects: z.array(SubjectSummarySchema).optional(),
  })
  .extend(AuditInfoSchema.shape);

export type Teacher = z.infer<typeof TeacherSchema>;
