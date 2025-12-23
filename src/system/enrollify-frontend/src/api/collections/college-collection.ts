import createAppQueryOptions from "@/hooks/create-query-options";
import { useQuery } from "@tanstack/react-query";
import { CollegeSchema, type College } from "../models/college";

const queryKeys = {
  all: () => ["colleges"],
  create: () => [...queryKeys.all(), `create`],
  update: () => [...queryKeys.all(), `update`],
  delete: (collegeId?: number) => [...queryKeys.all(), `college-${collegeId}`],
};

export const getAllCollegesOptions = () =>
  createAppQueryOptions({
    path: "/api/colleges",
    options: {
      queryKey: queryKeys.all(),
      select: (colleges): College[] => {
        return colleges.map((c) => CollegeSchema.parse(c));
      },
    },
  });

export const useGetAllColleges = () => useQuery(getAllCollegesOptions());
