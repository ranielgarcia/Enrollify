import createQueryOptions from "@/hooks/create-query-options";
import { CurriculumSchema, type Curriculum } from "../models/curriculum";
import createMutationOptions from "@/hooks/create-mutation-options";
import { toast } from "sonner";

const queryKeys = {
  all: () => ["curriculums"],
  single: (id: number) => [...queryKeys.all(), id],
  createDraft: () => [...queryKeys.all(), `create-draft`],
};

// curriculumId is obfuscated, that is why it is type string
export const getCurriculumQueryOption = (curriculumId?: string) =>
  createQueryOptions({
    path: "/api/curriculums/{curriculumId}",
    pathParams: {
      curriculumId: curriculumId!,
    },
    options: {
      enabled: !!curriculumId,
      queryKey: queryKeys.all(),
      select: (curriculum): Curriculum | null => {
        if (!curriculum) return null;
        console.log(curriculum);
        return CurriculumSchema.parse(curriculum);
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
