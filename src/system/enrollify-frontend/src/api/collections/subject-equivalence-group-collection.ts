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
  delete: (id: number) => [...queryKeys.all(), `delete`, id],
  addSubjects: (id: number) => [...queryKeys.all(), `add-subjects`, id],
  removeSubject: (id: number, subjectCode: string) => [
    ...queryKeys.all(),
    `remove-subject`,
    id,
    subjectCode,
  ],
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

// ** Create New **
export const createSubjectEquivalenceGroupOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/subject-equivalence-groups",
    mutationKey: queryKeys.create(),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("New Subject equivalence group created successfully");
      },
    },
  });

// ** Update **
export const updateSubjectEquivalenceGroupOptions = (id: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/subject-equivalence-groups/{id}",
    pathParams: { id: id },
    mutationKey: queryKeys.update(id),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Subject equivalence group updated successfully");
      },
    },
  });

// ** Delete **
export const deleteSubjectEquivalenceGroupOptions = (id: number) =>
  createMutationOptions({
    httpVerb: "delete",
    path: "/api/subject-equivalence-groups/{id}",
    pathParams: { id: id },
    mutationKey: queryKeys.delete(id),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Subject equivalence group deleted successfully");
      },
    },
  });

// ** Add subjects **
export const addSubjectsToEquivalenceGroupOptions = (id: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/subject-equivalence-groups/{id}/add-subjects",
    pathParams: { id },
    mutationKey: queryKeys.addSubjects(id),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Subjects added to equivalence group successfully");
      },
    },
  });

// ** Remove subject **
export const removeSubjectFromEquivalenceGroupOptions = (
  id: number,
  subjectCode: string,
) =>
  createMutationOptions({
    httpVerb: "delete",
    path: "/api/subject-equivalence-groups/{id}/remove-subject",
    pathParams: { id: id },
    mutationKey: queryKeys.removeSubject(id, subjectCode),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Subject removed from equivalence group successfully");
      },
    },
  });
