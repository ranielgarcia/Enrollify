import { createAppSuspenseQueryOptions } from "@/hooks/create-query-options";
import { useMutation, useSuspenseQuery } from "@tanstack/react-query";
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
      select: (colleges): College[] => {
        return colleges.map((c) => CollegeSchema.parse(c));
      },
    },
  });

export const useGetAllCollegesSuspense = () =>
  useSuspenseQuery(getAllCollegesOptions());

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

export const useCreateCollege = () => useMutation(createCollegeOptions());

// ** Update **

export const updateCollegeOptions = (collegeId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/colleges",
    params: {
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

export const useUpdateCollege = (collegeId: number) =>
  useMutation(updateCollegeOptions(collegeId));

// ** Delete **

export const deleteCollegeOptions = (collegeId: number) =>
  createMutationOptions({
    httpVerb: "delete",
    path: "/api/colleges",
    mutationKey: queryKeys.delete(collegeId),
    params: {
      id: collegeId,
    },
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("College deleted successfully");
      },
    },
  });

export const useDeleteCollege = (collegeId: number) =>
  useMutation(deleteCollegeOptions(collegeId));
