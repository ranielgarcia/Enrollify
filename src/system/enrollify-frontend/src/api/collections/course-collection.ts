import createQueryOptions from "@/hooks/create-query-options";
import { CourseSchema, type Course } from "../models/course";
import createMutationOptions from "@/hooks/create-mutation-options";
import { toast } from "sonner";

const queryKeys = {
  all: () => ["courses"],
  create: () => [...queryKeys.all(), `create`],
  update: (courseId: number) => [...queryKeys.all(), "update", courseId],
  delete: (courseId: number) => [...queryKeys.all(), "delete", courseId],
};

export const getAllCoursesOptions = () =>
  createQueryOptions({
    path: "/api/courses",
    options: {
      queryKey: queryKeys.all(),
      staleTime: 1000 * 60 * 5,
      select: (courses): Course[] => {
        return courses.map((t) => CourseSchema.parse(t));
      },
    },
  });

// ** Create **
export const createCourseOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/courses",
    mutationKey: queryKeys.create(),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Course created successfully");
      },
    },
  });

// ** Update **
export const updateCourseOptions = (courseId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: `/api/courses`,
    params: {
      id: courseId,
    },
    mutationKey: queryKeys.update(courseId),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Course updated successfully");
      },
    },
  });

// ** Delete **
export const deleteCourseOptions = (courseId: number) =>
  createMutationOptions({
    httpVerb: "delete",
    path: `/api/courses`,
    params: {
      id: courseId,
    },
    mutationKey: queryKeys.delete(courseId),
    options: {
      meta: { invalidateQueries: [queryKeys.all()] },
      onSuccess: () => {
        toast.success("Course deleted successfully");
      },
    },
  });
