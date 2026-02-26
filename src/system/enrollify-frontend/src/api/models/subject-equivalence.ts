import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";
import { SubjectSchema } from "@/api/models/subject";

// Summary schema for subjects in equivalence context
const SubjectSummarySchema = SubjectSchema.pick({
  id: true,
  code: true,
  title: true,
  units: true,
});

export type SubjectSummaryInSubjectEquivalenceGroup = z.infer<
  typeof SubjectSummarySchema
>;

// Subject Equivalence Group
export const SubjectEquivalenceGroupSchema = z
  .object({
    id: z.number(),
    name: z.string(),
    subjects: z.array(SubjectSummarySchema).optional(),
  })
  .extend(AuditInfoSchema.shape);

export type SubjectEquivalenceGroup = z.infer<
  typeof SubjectEquivalenceGroupSchema
>;

// Subject Equivalence (link between subject and group)
export const SubjectEquivalenceSchema = z
  .object({
    id: z.number(),
    subjectId: z.number(),
    equivalenceGroupId: z.number(),
    subject: SubjectSummarySchema.optional(),
    equivalenceGroup: SubjectEquivalenceGroupSchema.optional(),
  })
  .extend(AuditInfoSchema.shape);

export type SubjectEquivalence = z.infer<typeof SubjectEquivalenceSchema>;
