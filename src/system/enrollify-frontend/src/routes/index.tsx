import { createFileRoute } from "@tanstack/react-router";
import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from "@azure/msal-react";

export const Route = createFileRoute("/")({
  component: Index,
});

function Index() {
  return (
    <>
      <AuthenticatedTemplate>
        <h2>Logged in</h2>
      </AuthenticatedTemplate>
      <UnauthenticatedTemplate>
        <div>Please sign in to access the application.</div>
      </UnauthenticatedTemplate>
    </>
  );
}
