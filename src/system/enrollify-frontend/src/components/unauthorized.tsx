import { ShieldAlert } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  useNavigate,
  type NavigateOptions,
  type RegisteredRouter,
} from "@tanstack/react-router";

interface UnauthorizedProps {
  buttonLabel: string;
  message?: string;
  backOptions?: NavigateOptions<RegisteredRouter, string, string>;
  redirectOptions?: NavigateOptions<RegisteredRouter, string, string>;
}

const DEFAULT_MESSAGE =
  "Your current role does not have the necessary permissions to view, create, update, or delete content on this page.";

export function Unauthorized({
  buttonLabel,
  message = DEFAULT_MESSAGE,
  backOptions = { to: ".." },
  redirectOptions = { to: "/" },
}: UnauthorizedProps) {
  const navigate = useNavigate();

  return (
    <div className="flex min-h-screen items-center justify-center p-4">
      <Card className="w-full max-w-md text-center">
        <CardHeader className="space-y-4">
          <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-destructive/10">
            <ShieldAlert className="h-8 w-8 text-destructive" />
          </div>
          <CardTitle className="text-2xl font-bold">Access Denied</CardTitle>
          <CardDescription className="text-base">
            You do not have permission to access this resource.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-2 text-sm text-muted-foreground">
          <p>{message}</p>
          <p>
            If you believe this is an error, please contact your system
            administrator to request the appropriate access rights.
          </p>
        </CardContent>
        <CardFooter className="flex justify-center gap-2">
          <Button variant="outline" onClick={() => navigate(backOptions)}>
            Go Back
          </Button>
          <Button onClick={() => navigate(redirectOptions)}>
            {buttonLabel}
          </Button>
        </CardFooter>
      </Card>
    </div>
  );
}

// Usage:
// Route with parameters
// {
//   <Unauthorized
//   redirectOptions={{
//     to: "/portal/master-data/rooms/$tab",
//     params: { tab: "list" }
//   }}
// />

// With search params
/* <Unauthorized 
  redirectOptions={{ 
    to: "/portal", 
    search: { filter: "active" } 
  }} 
/> */
