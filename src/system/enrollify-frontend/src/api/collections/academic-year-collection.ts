import createAppQueryOptions from "@/hooks/create-query-options";
import createMutationOptions from "@/hooks/create-mutation-options";
import {
  AcademicYearTimelineSchema,
  type AcademicYearTimeline,
} from "@/api/models/academic-year";
import { toast } from "sonner";

const queryKeys = {
  base: () => ["academic-years"],
  active: () => [...queryKeys.base(), "active"],
  future: () => [...queryKeys.base(), "future"],
  previous: () => [...queryKeys.base(), "previous"],
  create: () => [...queryKeys.base(), "create"],
  update: (id: number) => [...queryKeys.base(), "update", id],
  delete: (id: number) => [...queryKeys.base(), "delete", id],
};

interface getAcademicYearTimeLineWindowProps {
  IncludeFutureYears: boolean;
  IncludePastYears: boolean;
  NumberOfFutureYears: number;
  NumberOfPastYears: number;
}

export const getAcademicYearTimeLineWindowOptions = ({
  IncludeFutureYears,
  IncludePastYears,
  NumberOfFutureYears,
  NumberOfPastYears,
}: getAcademicYearTimeLineWindowProps) =>
  createAppQueryOptions({
    path: "/api/academic-year-and-term/timeline-window",
    params: {
      IncludeFutureYears,
      IncludePastYears,
      NumberOfFutureYears,
      NumberOfPastYears,
    },
    options: {
      queryKey: queryKeys.active(),
      staleTime: 1000 * 60 * 5,
      select: (data): AcademicYearTimeline | null => {
        if (!data || (typeof data === "string" && data === "")) return null;
        return AcademicYearTimelineSchema.parse(data);
      },
    },
  });

export const createAcademicYearOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/academic-year-and-term",
    mutationKey: queryKeys.create(),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => toast.success("Academic year initiated successfully"),
    },
  });

export const updateAcademicYearOptions = (id: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/academic-year-and-term/{id}",
    pathParams: { id },
    mutationKey: queryKeys.update(id),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => toast.success("Academic year updated successfully"),
    },
  });

export const deleteAcademicYearOptions = (id: number) =>
  createMutationOptions({
    httpVerb: "delete",
    path: "/api/academic-year-and-term/{id}",
    pathParams: { id },
    mutationKey: queryKeys.delete(id),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => toast.success("Academic year deleted successfully"),
    },
  });
