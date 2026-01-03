import createQueryOptions from "@/hooks/create-query-options";
import { SubjectSchema, type Subject } from "../models/subject";
import { pagedResultSchema, type PagedResult } from "../models/paged-result";
import createMutationOptions from "@/hooks/create-mutation-options";
import { toast } from "sonner";

const queryKeys = {
  base: () => ["subjects"],
  paginated: (page: number, pageSize: number) => [
    ...queryKeys.base(),
    page,
    pageSize,
  ],
  create: () => [...queryKeys.base(), `create`],
  update: (subjectId: number) => [...queryKeys.base(), "update", subjectId],
  delete: (subjectId: number) => [...queryKeys.base(), "delete", subjectId],
};

const pagedSubjectsSchema = pagedResultSchema(SubjectSchema);

export const getAllSubjectsPaginatedOptions = (
  page: number,
  pageSize: number
) =>
  createQueryOptions({
    path: "/api/subjects/{page}/{pageSize}",
    pathParams: {
      page: page.toString(),
      pageSize: pageSize.toString(),
    },
    options: {
      queryKey: queryKeys.paginated(page, pageSize),
      select: (pagedResults): PagedResult<Subject> => {
        console.log("pagedResults", pagedResults, typeof pagedResults);
        // Handle empty response or string response
        if (
          !pagedResults ||
          (typeof pagedResults === "string" && pagedResults === "")
        ) {
          return {
            items: [],
            page,
            pageSize,
            totalCount: 0,
            totalPages: 0,
          };
        }
        const data =
          typeof pagedResults === "string"
            ? JSON.parse(pagedResults)
            : pagedResults;
        return pagedSubjectsSchema.parse(data);
      },
    },
  });

export const createSubjectOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/subjects",
    mutationKey: queryKeys.create(),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => {
        toast.success("Subject created successfully");
      },
    },
  });

export const updateSubjectOptions = (id: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/subjects",
    params: {
      id,
    },
    mutationKey: queryKeys.update(id),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => {
        toast.success("Subject updated successfully");
      },
    },
  });

export const deleteSubjectOptions = (id: number) =>
  createMutationOptions({
    httpVerb: "delete",
    path: "/api/subjects",
    params: {
      id,
    },
    mutationKey: queryKeys.delete(id),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => {
        toast.success("Subject deleted successfully");
      },
    },
  });
