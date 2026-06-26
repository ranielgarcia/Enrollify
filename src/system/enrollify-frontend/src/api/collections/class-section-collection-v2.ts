import createQueryOptions from "@/hooks/create-query-options";
import createMutationOptions from "@/hooks/create-mutation-options";
import type { paths as ApiPaths } from "@/api/generated/api";
import type { ClassSectionV2 } from "../models/class-section-v2";
import type { SectionStats } from "../models/section-stats";
import type { PagedResult } from "../models/paged-result";
import { toast } from "sonner";
import type {
  ExtendedColumnFilter,
  ExtendedColumnSort,
} from "@/types/data-table";
import {
  generateMockClassSections,
  generateMockSectionStats,
} from "@/lib/mock-data/sections-mock-data";

const queryKeys = {
  base: () => ["class-sections-v2"],
  bySectionId: (sectionId: number) => [
    ...queryKeys.base(),
    "section",
    sectionId,
  ],
  stats: (collegeId: number) => [...queryKeys.base(), "stats", collegeId],
  list: (collegeId: number) => [...queryKeys.base(), "list", collegeId],
  filter: (
    collegeId: number,
    page: number,
    pageSize: number,
    filters: ExtendedColumnFilter<ClassSectionV2>[],
    sort: ExtendedColumnSort<ClassSectionV2>[],
    joinOperator: string,
  ) => [
    ...queryKeys.base(),
    "filter",
    collegeId,
    page,
    pageSize,
    filters,
    sort,
    joinOperator,
  ],
  offerings: (sectionId: number) => [
    ...queryKeys.base(),
    sectionId,
    "offerings",
  ],
  bulk: () => [...queryKeys.base(), "bulk"],
};

interface BulkOpenClassSectionsResultDto {
  status: "Success" | "PartialSuccess" | "Failed";
  totalRequested: number;
  succeeded: number;
  failed: number;
  errors: Record<string, string>;
}

export const getClassSectionsStatsOptions = (collegeId: number) =>
  createQueryOptions({
    path: "/api/scheduling/colleges/{collegeId}/stats",
    pathParams: { collegeId },
    options: {
      enabled: !!collegeId,
      queryKey: queryKeys.stats(collegeId),
      staleTime: 1000 * 60 * 5,
      select: (): SectionStats => {
        // Mock response
        return generateMockSectionStats();
      },
    },
  });

export const getClassSectionsListOptions = (collegeId: number) =>
  createQueryOptions({
    path: "/api/scheduling/colleges/{collegeId}/class-sections",
    pathParams: { collegeId },
    options: {
      enabled: !!collegeId,
      queryKey: queryKeys.list(collegeId),
      staleTime: 1000 * 60 * 5,
      select: (): ClassSectionV2[] => {
        // Mock response
        return generateMockClassSections(collegeId, 30);
      },
    },
  });

export const filterClassSectionsPaginatedOptions = (
  collegeId: number,
  page: number,
  pageSize: number,
  filters: ExtendedColumnFilter<ClassSectionV2>[] = [],
  sort: ExtendedColumnSort<ClassSectionV2>[] = [],
  joinOperator: string = "and",
) => {
  return createQueryOptions({
    path: "/api/scheduling/colleges/{collegeId}/class-sections",
    pathParams: { collegeId },
    options: {
      enabled: !!collegeId && !!page && !!pageSize,
      queryKey: queryKeys.filter(
        collegeId,
        page,
        pageSize,
        filters,
        sort,
        joinOperator,
      ),
      staleTime: 1000 * 60 * 2,
      select: (): PagedResult<ClassSectionV2> => {
        // Mock paginated response
        const allSections = generateMockClassSections(collegeId, 50);
        const startIndex = (page - 1) * pageSize;
        const endIndex = startIndex + pageSize;
        const pagedSections = allSections.slice(startIndex, endIndex);

        return {
          data: pagedSections,
          pageNumber: page,
          pageSize: pageSize,
          totalPages: Math.ceil(allSections.length / pageSize),
          totalRecords: allSections.length,
          hasNextPage: endIndex < allSections.length,
          hasPreviousPage: page > 1,
        };
      },
    },
  });
};

export const getOfferingDetailsOptions = (sectionId: number) =>
  createQueryOptions({
    path: "/api/class-sections/{sectionId}/offerings",
    pathParams: { sectionId },
    options: {
      enabled: !!sectionId,
      queryKey: queryKeys.offerings(sectionId),
      staleTime: 1000 * 60 * 5,
      select: () => {
        // Mock response: return empty offering details
        return {
          offerings: [],
          conflicts: [],
        };
      },
    },
  });

// Mutations (stubs for Phase 1)

export const openClassSectionMutationOptions = () =>
  createMutationOptions({
    path: "/api/class-sections/{sectionId}/open",
    method: "POST",
    options: {
      onSuccess: () => {
        toast.success("Section opened for enrollment");
        queryKeys.base();
      },
      onError: (error: unknown) => {
        toast.error("Failed to open section");
        console.error(error);
      },
    },
  });

export const cancelClassSectionMutationOptions = () =>
  createMutationOptions({
    path: "/api/class-sections/{sectionId}/cancel",
    method: "POST",
    options: {
      onSuccess: () => {
        toast.success("Section cancelled");
        queryKeys.base();
      },
      onError: (error: unknown) => {
        toast.error("Failed to cancel section");
        console.error(error);
      },
    },
  });

export const bulkOpenSectionsMutationOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    // Path type resolved after API regeneration — matches POST /api/scheduling/class-sections/bulk/open
    path: "/api/scheduling/class-sections/bulk/open" as keyof ApiPaths,
    mutationKey: queryKeys.bulk(),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: (data) => {
        const result = data as BulkOpenClassSectionsResultDto;
        if (result.status === "Success") {
          toast.success("All sections opened for enrollment");
        } else if (result.status === "PartialSuccess") {
          toast.warning(`${result.succeeded} of ${result.totalRequested} sections opened`, {
            description: `${result.failed} section(s) failed. Check action history for details.`,
          });
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

// TODO: Replace toast summary with action history (notifications) so users can
// review per-section results of all bulk operations in a dedicated UI.

export const bulkCancelSectionsMutationOptions = () =>
  createMutationOptions({
    path: "/api/scheduling/colleges/class-sections/bulk/cancel",
    method: "POST",
    options: {
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
    path: "/api/scheduling/colleges/class-sections/bulk/assign-adviser",
    method: "POST",
    options: {
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

export default {
  queryKeys,
  getClassSectionsStatsOptions,
  getClassSectionsListOptions,
  filterClassSectionsPaginatedOptions,
  getOfferingDetailsOptions,
  openClassSectionMutationOptions,
  cancelClassSectionMutationOptions,
  bulkOpenSectionsMutationOptions,
  bulkCancelSectionsMutationOptions,
  bulkAssignAdviserMutationOptions,
};
