import createQueryOptions from "@/hooks/create-query-options";
import createMutationOptions from "@/hooks/create-mutation-options";
import { toast } from "sonner";
import { DepartmentSchema, type Department } from "../models/department";

const queryKeys = {
  all: () => ["departments"],
  create: () => [...queryKeys.all(), `create`],
  update: (departmentId: number) => [
    ...queryKeys.all(),
    "update",
    departmentId,
  ],
  delete: (departmentId: number) => [
    ...queryKeys.all(),
    "delete",
    departmentId,
  ],
};

export const getAllDepartmentsOptions = () =>
  createQueryOptions({
    path: "/api/departments",
    options: {
      queryKey: queryKeys.all(),
      staleTime: 1000 * 60 * 5,
      select: (departments): Department[] => {
        return departments.map((t) => DepartmentSchema.parse(t));
      },
    },
  });

// ** Create **
export const createDepartmentOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/departments",
    mutationKey: queryKeys.create(),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Department created successfully");
      },
    },
  });

// ** Update **
export const updateDepartmentOptions = (departmentId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/departments/{id}",
    pathParams: {
      id: departmentId,
    },
    mutationKey: queryKeys.update(departmentId),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Department updated successfully");
      },
    },
  });

// ** Delete **
export const deleteDepartmentOptions = (departmentId: number) =>
  createMutationOptions({
    httpVerb: "delete",
    path: "/api/departments/{id}",
    pathParams: {
      id: departmentId,
    },
    mutationKey: queryKeys.delete(departmentId),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Department deleted successfully");
      },
    },
  });
