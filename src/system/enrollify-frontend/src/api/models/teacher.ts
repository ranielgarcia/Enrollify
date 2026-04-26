import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";
import { DepartmentSchema } from "./department";
// import { SubjectSchema } from "./subject";

const TeacherDepartmentSchema = DepartmentSchema.pick({
  id: true,
  name: true,
});

// const TeacherSubjectSchema = SubjectSchema.pick({
//   id: true,
//   code: true,
//   title: true,
// });

export const TeacherSchema = z
  .object({
    id: z.number(),
    firstName: z.string(),
    lastName: z.string(),
    middleName: z.string(),
    email: z.string(),
    phoneNumber: z.string(),
    department: TeacherDepartmentSchema,
    academicTitle: z.string(),
    qualification: z.string(),
    specialization: z.string(),
    officeLocation: z.string(),
    officeHours: z.string(),
    biography: z.string().optional(),
    profilePicture: z.string().optional(),
    // subjects: z.array(TeacherSubjectSchema).optional(),
  })
  .extend(AuditInfoSchema.shape);

export type Teacher = z.infer<typeof TeacherSchema>;
