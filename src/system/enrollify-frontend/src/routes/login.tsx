import { Button } from "@/components/ui/button";
import { handleLogin } from "@/infrastructure/authentication/msal";
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/login")({
  beforeLoad: ({ context: { msal } }) => {
    const activeAccount = msal?.instance.getActiveAccount();
    if (activeAccount !== null) {
      throw redirect({
        to: "/portal/home",
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
