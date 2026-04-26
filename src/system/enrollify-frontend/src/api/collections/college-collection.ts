import { createAppSuspenseQueryOptions } from "@/hooks/create-query-options";
import { CollegeSchema, type College } from "../models/college";
import createMutationOptions from "@/hooks/create-mutation-options";
import { toast } from "sonner";

const queryKeys = {
  all: () => ["colleges"],
  create: () => [...queryKeys.all(), `create`],
  update: (collegeId: number) => [...queryKeys.all(), `update`, collegeId],
  delete: (collegeId: number) => [...queryKeys.all(), `delete`, collegeId],
};

// ** Get all **
export const getAllCollegesOptions = () =>
  createAppSuspenseQueryOptions({
    path: "/api/colleges",
    options: {
      queryKey: queryKeys.all(),
      staleTime: 1000 * 60 * 5,
      select: (colleges): College[] => {
        return colleges.map((c) => CollegeSchema.parse(c));
      },
    },
  });

// ** Create **
export const createCollegeOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/colleges",
    mutationKey: queryKeys.create(),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("College created successfully");
      },
    },
  });

// ** Update **
export const updateCollegeOptions = (collegeId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/colleges/{id}",
    pathParams: {
      id: collegeId,
    },
    mutationKey: queryKeys.update(collegeId),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("College updated successfully");
      },
    },
  });

// ** Delete **
export const deleteCollegeOptions = (collegeId: number) =>
  createMutationOptions({
    httpVerb: "delete",
    path: "/api/colleges/{id}",
    mutationKey: queryKeys.delete(collegeId),
    pathParams: {
      id: collegeId,
    },
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("College deleted successfully");
      },
    },
  });
