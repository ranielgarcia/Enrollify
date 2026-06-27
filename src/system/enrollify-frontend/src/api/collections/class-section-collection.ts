import type {
  ExtendedColumnFilter,
  ExtendedColumnSort,
} from "@/types/data-table";
import createMutationOptions from "@/hooks/create-mutation-options";
import createQueryOptions from "@/hooks/create-query-options";
import {
  ClassSectionSchema,
  ClassSectionWithOfferingsSchema,
  CollegeCoursesWithClassSectionsSchema,
  type BulkStateChangeClassSectionsResult,
  type ClassSection,
  type ClassSectionWithOfferings,
  type CollegeCoursesWithClassSections,
} from "@/api/models/class-scheduling/class-section";
import { pagedResultSchema, type PagedResult } from "@/api/models/paged-result";
import { toast } from "sonner";
import {
  OfferingSchema,
  type Offering,
} from "../models/class-scheduling/offering";

const queryKeys = {
  base: () => ["sections"],
  filter: (
    page: number,
    pageSize: number,
    academicYearId: number,
    filters: ExtendedColumnFilter<ClassSection>[],
    sort: ExtendedColumnSort<ClassSection>[],
    joinOperator: string,
  ) => [
    ...queryKeys.base(),
    "filter",
    page,
    pageSize,
    academicYearId,
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
  collegeCoursesClassSchedulingStats: (collegeId?: number) => [
    ...queryKeys.base(),
    "collegeCoursesClassSchedulingStats",
    collegeId ?? 0,
  ],
  collegeCoursesWithClassSectionsForScheduling: (collegeId: number) => [
    ...queryKeys.base(),
    "collegeCoursesWithClassSectionsForScheduling",
    collegeId,
  ],
  bulkOpen: () => [...queryKeys.base(), "open-class-sections"],
  bulkCancel: () => [...queryKeys.base(), "cancel-class-sections"],
  bulkAssignAdviser: () => [
    ...queryKeys.base(),
    "assign-adviser-class-sections",
  ],
  offerings: (sectionId: number) => [
    ...queryKeys.base(),
    "class-section-subject-offerings",
    sectionId,
  ],
};

const pagedSectionsSchema = pagedResultSchema(ClassSectionSchema);

export const filterClassSectionsPaginatedOptions = (
  page: number,
  pageSize: number,
  academicYearId: number | null,
  filters: ExtendedColumnFilter<ClassSection>[],
  sort: ExtendedColumnSort<ClassSection>[],
  joinOperator: string,
) => {
  // Handle invalid or missing academic year ID
  if (!academicYearId || academicYearId <= 0) {
    return createQueryOptions({
      path: "/api/scheduling/class-sections/filter/{page}/{pageSize}",
      pathParams: { page, pageSize },
      params: {},
      options: {
        enabled: false,
        queryKey: [
          ...queryKeys.base(),
          "filter",
          page,
          pageSize,
          "no-academic-year",
          filters,
          sort,
          joinOperator,
        ],
        staleTime: 1000 * 60 * 2,
        select: (): PagedResult<ClassSection> => ({
          items: [],
          page,
          pageSize,
          totalCount: 0,
          totalPages: 0,
        }),
      },
    });
  }

  return createQueryOptions({
    path: "/api/scheduling/class-sections/filter/{page}/{pageSize}",
    pathParams: { page, pageSize },
    params: {
      Filters: filters.length ? JSON.stringify(filters) : undefined,
      Sort: sort.length ? JSON.stringify(sort) : undefined,
      JoinOperator: joinOperator,
      AcademicYearId: academicYearId,
    },
    options: {
      enabled: true,
      queryKey: queryKeys.filter(
        page,
        pageSize,
        academicYearId,
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
};

export const getOfferingDetailsForClassSectionOptions = (sectionId: number) =>
  createQueryOptions({
    path: "/api/scheduling/class-sections/{sectionId}/subject-offerings",
    pathParams: { sectionId },
    options: {
      enabled: !!sectionId,
      queryKey: queryKeys.offerings(sectionId),
      staleTime: 1000 * 60 * 5,
      select: (data): Offering[] => OfferingSchema.array().parse(data),
    },
  });

export const getSectionWithOfferingsOptions = (sectionId: number) =>
  createQueryOptions({
    path: "/api/scheduling/class-sections/{id}",
    pathParams: { id: sectionId },
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
    path: "/api/scheduling/class-sections/bulk-initialize",
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
    path: "/api/scheduling/class-sections",
    mutationKey: queryKeys.create(),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => toast.success("Class section created successfully"),
    },
  });

export const updateClassSectionOptions = (id: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/scheduling/class-sections/{id}",
    pathParams: { id },
    mutationKey: queryKeys.update(id),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => toast.success("Class section updated successfully"),
    },
  });

// Transition mutations — PUT /api/scheduling/class-sections/{id}/[action]

export const openClassSectionOptions = (sectionId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/scheduling/class-sections/{id}/open",
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
    path: "/api/scheduling/class-sections/{id}/lock",
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
    path: "/api/scheduling/class-sections/{id}/activate",
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
    path: "/api/scheduling/class-sections/{id}/complete",
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
    path: "/api/scheduling/class-sections/{id}/cancel",
    pathParams: { id: sectionId },
    mutationKey: queryKeys.transition(sectionId, "cancel"),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => toast.success("Section cancelled"),
    },
  });

export const getClassSectionsStatsOptions = (collegeId?: number) =>
  createQueryOptions({
    path: "/api/scheduling/colleges/{collegeId}/stats",
    pathParams: { collegeId: collegeId ?? 0 },
    options: {
      enabled: !!collegeId,
      queryKey: queryKeys.collegeCoursesClassSchedulingStats(collegeId),
      staleTime: 1000 * 60 * 5,
      select: (data): object => {
        return data;
      },
    },
  });

export const getCollegeCoursesWithClassSectionsForSchedulingOptions = (
  collegeId: number,
) =>
  createQueryOptions({
    path: "/api/scheduling/colleges/{collegeId}/class-sections",
    pathParams: { collegeId },
    options: {
      enabled: !!collegeId,
      queryKey:
        queryKeys.collegeCoursesWithClassSectionsForScheduling(collegeId),
      staleTime: 1000 * 60 * 5,
      select: (data): CollegeCoursesWithClassSections => {
        return CollegeCoursesWithClassSectionsSchema.parse(data);
      },
    },
  });

export const bulkOpenSectionsMutationOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/scheduling/class-sections/bulk/open",
    mutationKey: queryKeys.bulkOpen(),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: (data) => {
        const result = data as BulkStateChangeClassSectionsResult;
        if (result.status === "Success") {
          toast.success("All sections opened for enrollment");
        } else if (result.status === "PartialSuccess") {
          toast.warning(
            `${result.succeeded} of ${result.totalRequested} sections opened`,
            {
              description: `${result.failed} section(s) failed. Check action history for details.`,
            },
          );
        } else {
          toast.error("Failed to open any sections");
        }
      },
      onError: (error) => {
        toast.error("Failed to open sections");
        console.error(error);
      },
    },
  });

export const bulkCancelSectionsMutationOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/scheduling/class-sections/bulk/cancel",
    mutationKey: queryKeys.bulkCancel(),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => {
        toast.success("Sections cancelled");
        queryKeys.base();
      },
      onError: (error: unknown) => {
        toast.error("Failed to cancel sections");
        console.error(error);
      },
    },
  });

export const bulkAssignAdviserMutationOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/scheduling/class-sections/bulk/assign-adviser",
    mutationKey: queryKeys.bulkAssignAdviser(),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => {
        toast.success("Adviser assigned to sections");
        queryKeys.base();
      },
      onError: (error: unknown) => {
        toast.error("Failed to assign adviser");
        console.error(error);
      },
    },
  });
