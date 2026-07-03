import { Component, type ReactNode } from "react";
import { AlertCircle } from "lucide-react";

import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";

interface SectionsErrorBoundaryProps {
  children: ReactNode;
  onReset?: () => void;
}

interface SectionsErrorBoundaryState {
  hasError: boolean;
  error?: Error;
}

export class SectionsErrorBoundary extends Component<
  SectionsErrorBoundaryProps,
  SectionsErrorBoundaryState
> {
  constructor(props: SectionsErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): SectionsErrorBoundaryState {
    return { hasError: true, error };
  }

  handleReset = () => {
    this.setState({ hasError: false, error: undefined });
    this.props.onReset?.();
  };

  render() {
    if (this.state.hasError) {
      return (
        <Alert variant="destructive" className="my-4">
          <AlertCircle className="size-4" />
          <AlertTitle>Failed to load sections</AlertTitle>
          <AlertDescription className="mt-2 space-y-3">
            <p className="text-sm">
              {this.state.error?.message ??
                "An unexpected error occurred while loading class sections."}
            </p>
            <Button
              size="sm"
              variant="outline"
              onClick={this.handleReset}
              className="gap-2"
            >
              Retry
            </Button>
          </AlertDescription>
        </Alert>
      );
    }

    return this.props.children;
  }
}
