import {
  OfferingWithSchedulesSchema,
  type OfferingWithSchedules,
} from "../models/offering";
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

export const getOfferingsBySectionOptions = (sectionId: number) =>
  // @ts-ignore - path will be registered in api.ts when backend is implemented
  createQueryOptions({
    path: "/api/offerings" as any,
    params: { sectionId } as any,
    options: {
      queryKey: queryKeys.bySectionId(sectionId),
      staleTime: 1000 * 60 * 2,
      select: (data): OfferingWithSchedules[] => {
        if (!data || (typeof data === "string" && data === "")) return [];
        const parsed = typeof data === "string" ? JSON.parse(data) : data;
        return (parsed as unknown[]).map((item) =>
          OfferingWithSchedulesSchema.parse(item),
        );
      },
    },
  });

export const createOfferingOptions = (sectionId: number) =>
  // @ts-ignore - path will be registered in api.ts when backend is implemented
  createMutationOptions({
    httpVerb: "post",
    path: "/api/offerings" as any,
    mutationKey: queryKeys.create(),
    options: {
      meta: { invalidateQueries: [queryKeys.base(), queryKeys.bySectionId(sectionId)] },
      onSuccess: () => toast.success("Subject offering assigned successfully"),
    },
  });

export const updateOfferingOptions = (id: number) =>
  // @ts-ignore - path will be registered in api.ts when backend is implemented
  createMutationOptions({
    httpVerb: "put",
    path: "/api/offerings/{id}" as any,
    pathParams: { id } as any,
    mutationKey: queryKeys.update(id),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => toast.success("Offering updated successfully"),
    },
  });

export const deleteOfferingOptions = (id: number) =>
  // @ts-ignore - path will be registered in api.ts when backend is implemented
  createMutationOptions({
    httpVerb: "delete",
    path: "/api/offerings/{id}" as any,
    pathParams: { id } as any,
    mutationKey: queryKeys.delete(id),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => toast.success("Offering removed successfully"),
    },
  });

export const createScheduleRowOptions = (offeringId: number) =>
  // @ts-ignore - path will be registered in api.ts when backend is implemented
  createMutationOptions({
    httpVerb: "post",
    path: "/api/offerings/{id}/schedules" as any,
    pathParams: { id: offeringId } as any,
    mutationKey: queryKeys.createSchedule(offeringId),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => toast.success("Schedule row added successfully"),
    },
  });

export const deleteScheduleRowOptions = (
  offeringId: number,
  scheduleId: number,
) =>
  // @ts-ignore - path will be registered in api.ts when backend is implemented
  createMutationOptions({
    httpVerb: "delete",
    path: "/api/offerings/{id}/schedules/{scheduleId}" as any,
    pathParams: { id: offeringId, scheduleId } as any,
    mutationKey: queryKeys.deleteSchedule(offeringId, scheduleId),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => toast.success("Schedule row removed successfully"),
    },
  });
