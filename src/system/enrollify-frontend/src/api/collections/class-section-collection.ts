import type {
  ExtendedColumnFilter,
  ExtendedColumnSort,
} from "@/types/data-table";
import createMutationOptions from "@/hooks/create-mutation-options";
import createQueryOptions from "@/hooks/create-query-options";
import {
  ClassSectionSchema,
  ClassSectionWithOfferingsSchema,
  type ClassSection,
  type ClassSectionWithOfferings,
} from "@/api/models/class-section";
import { pagedResultSchema, type PagedResult } from "@/api/models/paged-result";
import { toast } from "sonner";

const queryKeys = {
  base: () => ["sections"],
  filter: (
    page: number,
    pageSize: number,
    academicTermIds: number[],
    filters: ExtendedColumnFilter<ClassSection>[],
    sort: ExtendedColumnSort<ClassSection>[],
    joinOperator: string,
  ) => [
    ...queryKeys.base(),
    "filter",
    page,
    pageSize,
    academicTermIds,
    filters,
    sort,
    joinOperator,
  ],
  detail: (sectionId: number) => [...queryKeys.base(), "detail", sectionId],
  bulkInitializeSections: () => [...queryKeys.base(), "bulk-initialize"],
  create: () => [...queryKeys.base(), "create"],
  update: (sectionId: number) => [...queryKeys.base(), "update", sectionId],
  delete: (sectionId: number) => [...queryKeys.base(), "delete", sectionId],
  transition: (sectionId: number, action: string) => [
    ...queryKeys.base(),
    sectionId,
    action,
  ],
};

const pagedSectionsSchema = pagedResultSchema(ClassSectionSchema);

export const filterClassSectionsPaginatedOptions = (
  page: number,
  pageSize: number,
  academicTermIds: number[],
  filters: ExtendedColumnFilter<ClassSection>[],
  sort: ExtendedColumnSort<ClassSection>[],
  joinOperator: string,
) =>
  createQueryOptions({
    path: "/api/class-sections/filter/{page}/{pageSize}",
    pathParams: { page, pageSize },
    params: {
      Filters: filters.length ? JSON.stringify(filters) : undefined,
      Sort: sort.length ? JSON.stringify(sort) : undefined,
      JoinOperator: joinOperator,
      AcademicTermIds: academicTermIds.length
        ? JSON.stringify(academicTermIds)
        : undefined,
    },
    options: {
      queryKey: queryKeys.filter(
        page,
        pageSize,
        academicTermIds,
        filters,
        sort,
        joinOperator,
      ),
      staleTime: 1000 * 60 * 2,
      select: (pagedResults): PagedResult<ClassSection> => {
        if (
          !pagedResults ||
          (typeof pagedResults === "string" && pagedResults === "")
        ) {
          return { items: [], page, pageSize, totalCount: 0, totalPages: 0 };
        }
        const data =
          typeof pagedResults === "string"
            ? JSON.parse(pagedResults)
            : pagedResults;
        return pagedSectionsSchema.parse(data);
      },
    },
  });

// @ts-ignore - path will be registered in api.ts when backend GET endpoint is implemented
export const getSectionWithOfferingsOptions = (sectionId: number) =>
  createQueryOptions({
    path: "/api/class-sections/{id}" as any,
    pathParams: { id: sectionId } as any,
    options: {
      queryKey: queryKeys.detail(sectionId),
      staleTime: 1000 * 60 * 2,
      select: (data): ClassSectionWithOfferings =>
        ClassSectionWithOfferingsSchema.parse(data),
    },
  });

export const bulkInitializeClassSectionsOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/class-sections/bulk-initialize",
    mutationKey: queryKeys.bulkInitializeSections(),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () =>
        toast.success("Class sections bulk initialized successfully"),
    },
  });

export const createNewClassSectionsOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/class-sections",
    mutationKey: queryKeys.create(),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => toast.success("Class section created successfully"),
    },
  });

// Transition mutations — PUT /api/class-sections/{id}/[action]

export const openClassSectionOptions = (sectionId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/class-sections/{id}/open",
    pathParams: { id: sectionId },
    mutationKey: queryKeys.transition(sectionId, "open"),
    options: {
      meta: { invalidateQueries: [queryKeys.detail(sectionId)] },
      onSuccess: () => toast.success("Section opened for enrollment"),
    },
  });

export const lockClassSectionOptions = (sectionId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/class-sections/{id}/lock",
    pathParams: { id: sectionId },
    mutationKey: queryKeys.transition(sectionId, "lock"),
    options: {
      meta: { invalidateQueries: [queryKeys.detail(sectionId)] },
      onSuccess: () => toast.success("Enrollment locked"),
    },
  });

export const activateClassSectionOptions = (sectionId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/class-sections/{id}/activate",
    pathParams: { id: sectionId },
    mutationKey: queryKeys.transition(sectionId, "activate"),
    options: {
      meta: { invalidateQueries: [queryKeys.detail(sectionId)] },
      onSuccess: () => toast.success("Section activated"),
    },
  });

export const completeClassSectionOptions = (sectionId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/class-sections/{id}/complete",
    pathParams: { id: sectionId },
    mutationKey: queryKeys.transition(sectionId, "complete"),
    options: {
      meta: { invalidateQueries: [queryKeys.detail(sectionId)] },
      onSuccess: () => toast.success("Section completed"),
    },
  });

export const cancelClassSectionOptions = (sectionId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/class-sections/{id}/cancel",
    pathParams: { id: sectionId },
    mutationKey: queryKeys.transition(sectionId, "cancel"),
    options: {
      meta: { invalidateQueries: [queryKeys.detail(sectionId)] },
      onSuccess: () => toast.success("Section cancelled"),
    },
  });
