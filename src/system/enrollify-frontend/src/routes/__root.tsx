import { createRootRoute } from "@tanstack/react-router";
import { AppLayout } from "@/app/app-layout";

const RootLayout = () => {
  return <AppLayout />;
};

export const Route = createRootRoute({ component: RootLayout });
