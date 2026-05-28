import createMutationOptions from "@/hooks/create-mutation-options";
import createQueryOptions from "@/hooks/create-query-options";
import {
  ClassSectionWithOfferingsSchema,
  type ClassSectionWithOfferings,
} from "@/api/models/class-section";
import { toast } from "sonner";

const queryKeys = {
  base: () => ["sections"],
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
