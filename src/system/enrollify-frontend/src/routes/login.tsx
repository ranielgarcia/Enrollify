import { Button } from "@/components/ui/button";
import { handleLogin } from "@/infrastructure/authentication/msal";
import { createFileRoute, redirect } from "@tanstack/react-router";
import z from "zod";

const loginSearchParamsSchema = z.object({
  redirect: z.string().optional(),
});

export const Route = createFileRoute("/login")({
  validateSearch: loginSearchParamsSchema,
  beforeLoad: ({ context: { msal }, search }) => {
    const activeAccount = msal?.instance.getActiveAccount();

    if (search.redirect && activeAccount !== null) {
      throw redirect({
        to: search.redirect,
      });
    }
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
