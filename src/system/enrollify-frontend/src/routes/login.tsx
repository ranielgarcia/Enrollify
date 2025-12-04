import { Button } from "@/components/ui/button";
import { handleLogin } from "@/infrastructure/auth/msal";
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/login")({
  beforeLoad: ({ context: { msal } }) => {
    const activeAccount = msal?.instance.getActiveAccount();
    if (activeAccount !== null) {
      throw redirect({
        to: "/portal/dashboard",
      });
    }
  },
  component: () => (
    <>
      <h2>Login page</h2>

      <Button onClick={handleLogin}>Login</Button>
    </>
  ),
});
