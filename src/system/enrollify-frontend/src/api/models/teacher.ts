import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";
import { CollegeSchema } from "./college";
import { DepartmentSchema } from "./department";
import { SubjectSchema } from "./subject";

const TeacherCollegeSchema = CollegeSchema.pick({
  id: true,
  name: true,
});

const TeacherDepartmentSchema = DepartmentSchema.pick({
  id: true,
  name: true,
});

const TeacherSubjectSchema = SubjectSchema.pick({
  id: true,
  code: true,
  title: true,
});

export const TeacherSchema = z
  .object({
    id: z.number(),
    profilePicture: z.string().optional(),
    firstName: z.string(),
    lastName: z.string(),
    middleName: z.string(),
    email: z.string(),
    phoneNumber: z.string(),
    college: TeacherCollegeSchema,
    academicTitle: z.string(),
    department: TeacherDepartmentSchema,
    qualification: z.string(),
    specialization: z.string(),
    officeLocation: z.string(),
    officeHours: z.string(),
    biography: z.string().optional(),
    subjects: z.array(TeacherSubjectSchema),
  })
  .extend(AuditInfoSchema.shape);

export type Teacher = z.infer<typeof TeacherSchema>;
