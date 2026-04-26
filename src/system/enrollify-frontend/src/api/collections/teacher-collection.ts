import type {
  ExtendedColumnFilter,
  ExtendedColumnSort,
} from "@/types/data-table";
import { TeacherSchema, type Teacher } from "../models/teacher";
import { pagedResultSchema, type PagedResult } from "../models/paged-result";
import createQueryOptions from "@/hooks/create-query-options";
import createMutationOptions from "@/hooks/create-mutation-options";
import { toast } from "sonner";

const queryKeys = {
  base: () => ["teachers"],
  paginated: (page: number, pageSize: number) => [
    ...queryKeys.base(),
    page,
    pageSize,
  ],
  filter: (
    page: number,
    pageSize: number,
    filters: ExtendedColumnFilter<Teacher>[],
    sort: ExtendedColumnSort<Teacher>[],
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
  create: () => [...queryKeys.base(), `create`],
  update: (teacherId: number) => [...queryKeys.base(), "update", teacherId],
  delete: (teacherId: number) => [...queryKeys.base(), "delete", teacherId],
};

const pagedTeachersSchema = pagedResultSchema(TeacherSchema);

export const filterTeachersPaginatedOptions = (
  page: number,
  pageSize: number,
  filters: ExtendedColumnFilter<Teacher>[],
  sort: ExtendedColumnSort<Teacher>[],
  joinOperator: string,
) =>
  createQueryOptions({
    path: "/api/teachers/filter/{page}/{pageSize}",
    pathParams: {
      page: page,
      pageSize: pageSize,
    },
    params: {
      filters: filters.length ? JSON.stringify(filters) : undefined,
      sort: sort.length ? JSON.stringify(sort) : undefined,
      joinOperator,
    },
    options: {
      queryKey: queryKeys.filter(page, pageSize, filters, sort, joinOperator),
      staleTime: 1000 * 60 * 2,
      select: (pagedResults): PagedResult<Teacher> => {
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
        return pagedTeachersSchema.parse(data);
      },
    },
  });

export const createTeacherOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/teachers",
    mutationKey: queryKeys.create(),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => {
        toast.success("Teacher created successfully");
      },
    },
  });

export const updateTeacherOptions = (id: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/teachers/{id}",
    pathParams: {
      id,
    },
    mutationKey: queryKeys.update(id),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => {
        toast.success("Teacher updated successfully");
      },
    },
  });
