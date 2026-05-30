import SectionDetailPage from "@/page-components/section-detail-page";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute, redirect } from "@tanstack/react-router";
import { z } from "zod";

const sectionParamsSchema = z.object({
  sectionId: z.string(),
});

export const Route = createFileRoute(
  "/portal/curriculum-and-scheduling/sections-details/$sectionId",
)({
  params: {
    parse: (params) => sectionParamsSchema.parse(params),
    stringify: (params) => params,
  },
  component: SectionDetailRoute,
  loader: (): RouteLoaderData => ({
    crumb: `Section Detail`,
  }),
  beforeLoad: async ({ context: { authorization } }): Promise<void> => {
    if (!authorization?.isReady) {
      return;
    }

    const canAccess = await authorization.checkPolicy("canViewClassSections");
    if (!canAccess) {
      throw redirect({
        to: "/portal/home",
      });
    }
  },
});

function SectionDetailRoute() {
  const params = Route.useParams();
  const sectionId = params.sectionId ?? "";
  return <SectionDetailPage sectionId={parseInt(sectionId, 10)} />;
}
