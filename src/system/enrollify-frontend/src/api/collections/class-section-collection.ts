import createMutationOptions from "@/hooks/create-mutation-options";
import { toast } from "sonner";

const queryKeys = {
  base: () => ["sections"],
  bulkInitializeSections: () => [...queryKeys.base(), "bulk-initialize"],
};

export const bulkInitializeSectionsOptions = () =>
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
