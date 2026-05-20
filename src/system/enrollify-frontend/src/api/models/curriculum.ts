import z from "zod";
import { AuditInfoSchema } from "./audit-info";
import { CourseSchema } from "./course";
import { dateTransformer } from "./date-transformer";

export const CurriculumStatusEnum = {
  Draft: 1,
  Active: 2,
  PhaseOut: 3,
  Archived: 4,
} as const;

type CurriculumStatusName = keyof typeof CurriculumStatusEnum;

const CourseSummarySchema = CourseSchema.pick({
  id: true,
  name: true,
});

const CurriculumStatusSchema = z.object({
  value: z.enum(CurriculumStatusEnum),
  name: z.enum(
    Object.keys(CurriculumStatusEnum) as [
      CurriculumStatusName,
      ...CurriculumStatusName[],
    ],
  ),
});

// sample:
// curriculum.status.value === CurriculumStatusEnum.Active  // 2 === 2 ✓
// curriculum.status.name === "Active"                      // "Active" === "Active" ✓

export const CurriculumSchema = z
  .object({
    id: z.number(),
    course: CourseSummarySchema,
    effectiveYear: z.number(),
    version: z.string(),
    status: CurriculumStatusSchema,
    description: z.string(),
    approvedDate: dateTransformer,
  })
  .extend(AuditInfoSchema.shape);

export type Curriculum = z.infer<typeof CurriculumSchema>;

const subjectSummarySchema = z.object({
  id: z.number(),
  code: z.string(),
  title: z.string(),
  units: z.number(),
});

const subjectPrerequisiteSchema = z.object({
  prerequisiteCurriculumSubjectId: z.number(),
  minimumGrade: z.number().nullable(),
});

const curriculumSubjectSchema = z.object({
  id: z.number(),
  subjectId: z.number(),
  yearLevel: z.number(),
  termNumber: z.number(),
  isElective: z.boolean(),
  electiveGroupName: z.string().nullable(),
  unitsOverride: z.number().nullable(),
  subject: subjectSummarySchema,
  prerequisites: z.array(subjectPrerequisiteSchema),
});

export const CurriculumWithSubjectsSchema = z.object({
  id: z.number(),
  course: CourseSummarySchema,
  effectiveYear: z.number(),
  version: z.string(),
  status: CurriculumStatusSchema,
  description: z.string(),
  approvedDate: dateTransformer,
  curriculumSubjects: z.array(curriculumSubjectSchema),
});

export type CurriculumWithSubjects = z.infer<
  typeof CurriculumWithSubjectsSchema
>;
