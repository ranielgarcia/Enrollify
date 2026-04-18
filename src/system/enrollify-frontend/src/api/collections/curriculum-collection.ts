import createQueryOptions from "@/hooks/create-query-options";
import {
  CurriculumSchema,
  CurriculumWithSubjectsSchema,
  type Curriculum,
  type CurriculumWithSubjects,
} from "../models/curriculum";
import createMutationOptions from "@/hooks/create-mutation-options";
import { toast } from "sonner";

const queryKeys = {
  all: () => ["curriculums"],
  single: (id: number) => [...queryKeys.all(), id],
  createDraft: () => [...queryKeys.all(), `create-draft`],
  update: (id: number) => [...queryKeys.all(), `update`, id],
};

export const getAllCurriculumsOptions = (enabled: boolean) =>
  createQueryOptions({
    path: "/api/curriculums",
    options: {
      enabled,
      queryKey: queryKeys.all(),
      staleTime: 1000 * 60 * 5,
      select: (curriculums): Curriculum[] => {
        return curriculums.map((c) => CurriculumSchema.parse(c));
      },
    },
  });

// curriculumId is obfuscated, that is why it is type string
export const getCurriculumQueryOption = (curriculumId?: number) =>
  createQueryOptions({
    path: "/api/curriculums/{curriculumId}",
    pathParams: {
      curriculumId: curriculumId?.toString() ?? "",
    },
    options: {
      enabled: !!curriculumId,
      queryKey: queryKeys.single(curriculumId ?? 0),
      select: (curriculum): CurriculumWithSubjects | null => {
        if (!curriculum) return null;
        return CurriculumWithSubjectsSchema.parse(curriculum);
      },
    },
  });

// ** Create Draft Curriculum **
export const createDraftCurriculumOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/curriculums",
    mutationKey: queryKeys.createDraft(),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Draft Curriculum created successfully");
      },
    },
  });

// ** Update Curriculum **
export const updateCurriculumOptions = (curriculumId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/curriculums",
    params: {
      id: curriculumId,
    },
    mutationKey: queryKeys.update(curriculumId),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Curriculum updated successfully");
      },
    },
  });

// ** Save Curriculum Content **
export const saveCurriculumContentOptions = (curriculumId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/curriculums/{curriculumId}/save-content",
    pathParams: {
      curriculumId: curriculumId,
    },
    mutationKey: queryKeys.update(curriculumId),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Curriculum content saved successfully");
      },
    },
  });
