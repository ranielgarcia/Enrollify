import createMutationOptions from "@/hooks/create-mutation-options";
import { toast } from "sonner";

const queryKeys = {
  base: () => ["sections"],
  bulkInitializeSections: () => [...queryKeys.base(), "bulk-initialize"],
  create: () => [...queryKeys.base(), "create"],
  update: (sectionId: number) => [...queryKeys.base(), "update", sectionId],
  delete: (sectionId: number) => [...queryKeys.base(), "delete", sectionId],
};

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
