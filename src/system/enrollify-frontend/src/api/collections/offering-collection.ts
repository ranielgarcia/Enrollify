import { OfferingSchema, type Offering } from "../models/offering";
import createQueryOptions from "@/hooks/create-query-options";
import createMutationOptions from "@/hooks/create-mutation-options";
import { toast } from "sonner";

const queryKeys = {
  base: () => ["offerings"],
  bySectionId: (sectionId: number) => [
    ...queryKeys.base(),
    "section",
    sectionId,
  ],
  detail: (id: number) => [...queryKeys.base(), "detail", id],
  schedules: (offeringId: number) => [
    ...queryKeys.base(),
    offeringId,
    "schedules",
  ],
  create: () => [...queryKeys.base(), "create"],
  update: (id: number) => [...queryKeys.base(), "update", id],
  delete: (id: number) => [...queryKeys.base(), "delete", id],
  createSchedule: (offeringId: number) => [
    ...queryKeys.base(),
    offeringId,
    "schedules",
    "create",
  ],
  deleteSchedule: (offeringId: number, scheduleId: number) => [
    ...queryKeys.base(),
    offeringId,
    "schedules",
    "delete",
    scheduleId,
  ],
};

const sectionDetailKey = (sectionId: number) => [
  "sections",
  "detail",
  sectionId,
];

export const getOfferingsBySectionOptions = (sectionId: number) =>
  createQueryOptions({
    path: "/api/subject-offerings",
    params: { SectionId: sectionId },
    options: {
      queryKey: queryKeys.bySectionId(sectionId),
      staleTime: 1000 * 60 * 2,
      select: (data): Offering[] => {
        if (!data || (typeof data === "string" && data === "")) return [];
        const parsed = typeof data === "string" ? JSON.parse(data) : data;
        return (parsed as unknown[]).map((item) => OfferingSchema.parse(item));
      },
    },
  });

export const createOfferingOptions = (sectionId: number) =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/subject-offerings",
    mutationKey: queryKeys.create(),
    options: {
      meta: {
        invalidateQueries: [
          queryKeys.base(),
          queryKeys.bySectionId(sectionId),
          sectionDetailKey(sectionId),
        ],
      },
      onSuccess: () => toast.success("Subject offering assigned successfully"),
    },
  });

export const updateOfferingOptions = (id: number, sectionId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/subject-offerings/{id}",
    pathParams: { id },
    mutationKey: queryKeys.update(id),
    options: {
      meta: {
        invalidateQueries: [
          queryKeys.base(),
          sectionDetailKey(sectionId),
        ],
      },
      onSuccess: () => toast.success("Offering updated successfully"),
    },
  });

export const createScheduleRowOptions = (
  offeringId: number,
  sectionId: number,
) =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/subject-offerings/{id}/schedules",
    pathParams: { id: offeringId },
    mutationKey: queryKeys.createSchedule(offeringId),
    options: {
      meta: {
        invalidateQueries: [
          queryKeys.base(),
          sectionDetailKey(sectionId),
        ],
      },
      onSuccess: () => toast.success("Schedule row added successfully"),
    },
  });

export const deleteScheduleRowOptions = (
  offeringId: number,
  scheduleId: number,
  sectionId: number,
) =>
  createMutationOptions({
    httpVerb: "delete",
    path: "/api/subject-offerings/{id}/schedules/{scheduleId}",
    pathParams: { id: offeringId, scheduleId },
    mutationKey: queryKeys.deleteSchedule(offeringId, scheduleId),
    options: {
      meta: {
        invalidateQueries: [
          queryKeys.base(),
          sectionDetailKey(sectionId),
        ],
      },
      onSuccess: () => toast.success("Schedule row removed successfully"),
    },
  });
