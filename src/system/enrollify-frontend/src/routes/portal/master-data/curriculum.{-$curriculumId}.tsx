import CurriculumPage from "@/page-components/curriculum-builder-page";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute, redirect } from "@tanstack/react-router";
import { z } from "zod";

const curriculumParamsSchema = z.object({
  curriculumId: z.string().min(1, "Curriculum ID cannot be empty").optional(),
});

export const Route = createFileRoute(
  "/portal/master-data/curriculum/{-$curriculumId}"
)({
  params: {
    parse: (params) => curriculumParamsSchema.parse(params),
    stringify: (params) => params,
  },
  component: CurriculumPage,
  loader: ({ params }): RouteLoaderData => ({
    crumb: params.curriculumId
      ? `Curriculum Builder (${params.curriculumId})`
      : "Curriculum Builder",
  }),
  beforeLoad: async ({ context: { authorization } }): Promise<void> => {
    // Wait for authorization to be ready before making authorization decisions
    // If not ready, don't redirect - allow the page to load and handle auth later
    if (!authorization?.isReady) {
      return;
    }

    const canAccess = await authorization.checkPolicy("canViewCourses");
    if (!canAccess) {
      throw redirect({
        to: "/portal/home",
      });
    }
  },
});
