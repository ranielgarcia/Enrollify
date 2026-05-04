import createAppQueryOptions from "@/hooks/create-query-options";
import createMutationOptions from "@/hooks/create-mutation-options";
import {
  AcademicYearSchema,
  type AcademicYear,
} from "@/api/models/academic-year";
import { toast } from "sonner";

const queryKeys = {
  base: () => ["academic-years"],
  active: () => [...queryKeys.base(), "active"],
  previous: () => [...queryKeys.base(), "previous"],
  create: () => [...queryKeys.base(), "create"],
  update: (id: number) => [...queryKeys.base(), "update", id],
  delete: (id: number) => [...queryKeys.base(), "delete", id],
};

export const getActiveAcademicYearOptions = () =>
  createAppQueryOptions({
    path: "/api/academic-years/active",
    options: {
      queryKey: queryKeys.active(),
      staleTime: 1000 * 60 * 5,
      select: (data): AcademicYear | null => {
        if (!data || (typeof data === "string" && data === "")) return null;
        return AcademicYearSchema.parse(data);
      },
    },
  });

export const getPreviousAcademicYearsOptions = () =>
  createAppQueryOptions({
    path: "/api/academic-years/previous",
    options: {
      queryKey: queryKeys.previous(),
      staleTime: 1000 * 60 * 5,
      select: (data): AcademicYear[] => {
        console.log(data);
        return data.map((ay) => AcademicYearSchema.parse(ay));
      },
    },
  });

// export const getActiveAcademicYearSuspenseOptions = () =>
//   createAppSuspenseQueryOptions({
//     path: "/api/academic-years/active",
//     options: {
//       queryKey: queryKeys.active(),
//       staleTime: 1000 * 60 * 5,
//       select: (data): AcademicYear | null => {
//         if (!data || (typeof data === "string" && data === "")) return null;
//         try {
//           return AcademicYearSchema.parse(data);
//         } catch {
//           return null;
//         }
//       },
//     },
//   });

export const createAcademicYearOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/academic-years",
    mutationKey: queryKeys.create(),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => toast.success("Academic year initiated successfully"),
    },
  });

export const updateAcademicYearOptions = (id: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/academic-years/{id}",
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
    path: "/api/academic-years/{id}",
    pathParams: { id },
    mutationKey: queryKeys.delete(id),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => toast.success("Academic year deleted successfully"),
    },
  });
