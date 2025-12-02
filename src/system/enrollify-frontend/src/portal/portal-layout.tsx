import { loginRequest } from "@/infrastructure/auth/authConfig";
import { InteractionType } from "@azure/msal-browser";
import {
  MsalAuthenticationTemplate,
  type MsalAuthenticationResult,
} from "@azure/msal-react";
import { Link } from "@tanstack/react-router";

function ErrorComponent({ error }: MsalAuthenticationResult) {
  return <p>An Error Occurred: {error?.message}</p>;
}

function LoadingComponent() {
  return <p>Authentication in progress...</p>;
}

export const AdminPortalLayout = () => {
  return (
    <MsalAuthenticationTemplate
      authenticationRequest={loginRequest}
      interactionType={InteractionType.Popup}
      errorComponent={ErrorComponent}
      loadingComponent={LoadingComponent}
    >
      <div className="p-2 flex gap-2">
        <Link to="/" className="[&.active]:font-bold">
          Home
        </Link>{" "}
        <Link to="/portal" className="[&.active]:font-bold">
          Portal
        </Link>
      </div>
    </MsalAuthenticationTemplate>
  );
};
