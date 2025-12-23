import { getAllCollegesOptions } from "@/api/collections/college-collection";
import CollegesPage from "@/page-components/colleges-management-page";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/portal/master-data/colleges")({
  beforeLoad: async ({ context: { queryClient } }): Promise<void> => {
    await queryClient.prefetchQuery(getAllCollegesOptions());
  },
  loader: (): RouteLoaderData => ({
    crumb: "Colleges",
  }),
  component: CollegesPage,
});
