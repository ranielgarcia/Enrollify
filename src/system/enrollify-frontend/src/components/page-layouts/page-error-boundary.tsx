import { Button } from "@/components/ui/button";
import {
  Card,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { AlertCircle } from "lucide-react";
import { useNavigate } from "@tanstack/react-router";

interface PageErrorBoundaryProps {
  error: Error;
  reset?: () => void;
}

export function PageErrorBoundary({ error, reset }: PageErrorBoundaryProps) {
  const navigate = useNavigate();

  return (
    <div className="flex min-h-[400px] items-center justify-center p-4">
      <Card className="w-full max-w-md text-center">
        <CardHeader className="space-y-4">
          <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-destructive/10">
            <AlertCircle className="h-8 w-8 text-destructive" />
          </div>
          <CardTitle className="text-2xl font-bold">
            Something went wrong
          </CardTitle>
          <CardDescription className="text-base">
            {error.message || "An unexpected error occurred. Please try again."}
          </CardDescription>
        </CardHeader>
        <CardFooter className="justify-center gap-2">
          {reset && (
            <Button onClick={reset} variant="default">
              Try Again
            </Button>
          )}
          <Button
            variant="outline"
            onClick={() => navigate({ to: "/portal/home" })}
          >
            Go Home
          </Button>
        </CardFooter>
      </Card>
    </div>
  );
}
