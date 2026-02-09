import createQueryOptions from "@/hooks/create-query-options";
import createMutationOptions from "@/hooks/create-mutation-options";
import { toast } from "sonner";
import {
  SubjectEquivalenceGroupSchema,
  type SubjectEquivalenceGroup,
} from "../models/subject-equivalence";

const queryKeys = {
  all: () => ["subject-equivalence-groups"],
  create: () => [...queryKeys.all(), `create-draft`],
  update: (id: number) => [...queryKeys.all(), `update`, id],
};

export const getAllSubjectEquivalenceGroupsOptions = (enabled: boolean) =>
  createQueryOptions({
    path: "/api/subject-equivalence-groups",
    options: {
      enabled,
      queryKey: queryKeys.all(),
      select: (groups): SubjectEquivalenceGroup[] => {
        return groups.map((g) => SubjectEquivalenceGroupSchema.parse(g));
      },
    },
  });
