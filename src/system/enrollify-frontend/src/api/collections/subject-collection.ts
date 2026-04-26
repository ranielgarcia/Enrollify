import createQueryOptions from "@/hooks/create-query-options";
import { SubjectSchema, type Subject } from "../models/subject";
import { pagedResultSchema, type PagedResult } from "../models/paged-result";
import createMutationOptions from "@/hooks/create-mutation-options";
import { toast } from "sonner";
import type {
  ExtendedColumnFilter,
  ExtendedColumnSort,
} from "@/types/data-table";

const queryKeys = {
  base: () => ["subjects"],
  paginated: (page: number, pageSize: number) => [
    ...queryKeys.base(),
    page,
    pageSize,
  ],
  filter: (
    page: number,
    pageSize: number,
    filters: ExtendedColumnFilter<Subject>[],
    sort: ExtendedColumnSort<Subject>[],
    joinOperator: string,
  ) => [
    ...queryKeys.base(),
    "search",
    page,
    pageSize,
    filters,
    sort,
    joinOperator,
  ],
  search: (page: number, pageSize: number, searchTerm?: string | null) => [
    ...queryKeys.base(),
    "search",
    page,
    pageSize,
    searchTerm,
  ],
  create: () => [...queryKeys.base(), `create`],
  update: (subjectId: number) => [...queryKeys.base(), "update", subjectId],
  delete: (subjectId: number) => [...queryKeys.base(), "delete", subjectId],
};

const pagedSubjectsSchema = pagedResultSchema(SubjectSchema);

export const getAllSubjectsMinimalOptions = () =>
  createQueryOptions({
    path: "/api/subjects",
    options: {
      queryKey: queryKeys.base(),
      staleTime: 1000 * 60 * 2,
      select: (subjects): Subject[] => {
        return subjects.map((s) => SubjectSchema.parse(s));
      },
    },
  });

export const searchSubjectsPaginatedOptions = (
  page: number,
  pageSize: number,
  searchTerm?: string | null,
  enabled: boolean = false,
) =>
  createQueryOptions({
    path: "/api/subjects/search/{page}/{pageSize}",
    pathParams: {
      page: page,
      pageSize: pageSize,
    },
    params: {
      SearchTerm: searchTerm,
    },
    options: {
      enabled: !!page && !!pageSize && enabled,
      queryKey: queryKeys.search(page, pageSize, searchTerm),
      staleTime: 1000 * 60 * 2,
      select: (pagedResults): PagedResult<Subject> => {
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

export const filterSubjectsPaginatedOptions = (
  page: number,
  pageSize: number,
  filters: ExtendedColumnFilter<Subject>[],
  sort: ExtendedColumnSort<Subject>[],
  joinOperator: string,
) =>
  createQueryOptions({
    path: "/api/subjects/filter/{page}/{pageSize}",
    pathParams: {
      page: page,
      pageSize: pageSize,
    },
    params: {
      Filters: filters.length ? JSON.stringify(filters) : undefined,
      Sort: sort.length ? JSON.stringify(sort) : undefined,
      JoinOperator: joinOperator,
    },
    options: {
      // enabled: !!page && !!pageSize && enabled,
      queryKey: queryKeys.filter(page, pageSize, filters, sort, joinOperator),
      staleTime: 1000 * 60 * 2,
      select: (pagedResults): PagedResult<Subject> => {
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
    path: "/api/subjects/{id}",
    pathParams: {
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
    path: "/api/subjects/{id}",
    pathParams: {
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
