import { ThemeProvider } from "@/components/theming/theme-provider";
import { Link, Outlet } from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/react-router-devtools";

export const AppLayout = () => {
  return (
    <ThemeProvider defaultTheme="dark" storageKey="vite-ui-theme">
      <div className="p-2 flex gap-2">
        <Link to="/" className="[&.active]:font-bold">
          Home
        </Link>{" "}
        <Link to="/about" className="[&.active]:font-bold">
          About
        </Link>
        <Link to="/contactus" className="[&.active]:font-bold">
          Contact Us
        </Link>
      </div>
      <hr />
      <Outlet />
      <TanStackRouterDevtools />
    </ThemeProvider>
  );
};
