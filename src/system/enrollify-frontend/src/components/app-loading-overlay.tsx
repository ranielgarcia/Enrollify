"use client";

import * as React from "react";

import { type VariantProps, cva } from "class-variance-authority";
import { Loader2 } from "lucide-react";

import { cn } from "@/lib/utils";

const overlayVariants = cva(
  "fixed inset-0 z-50 flex items-center justify-center bg-background/80 backdrop-blur-sm transition-all duration-200",
  {
    variants: {
      variant: {
        default: "bg-background/80",
        dark: "bg-background/90",
        light: "bg-white/80",
      },
    },
    defaultVariants: {
      variant: "default",
    },
  }
);

const spinnerVariants = cva("animate-spin text-primary", {
  variants: {
    size: {
      default: "h-8 w-8",
      sm: "h-6 w-6",
      lg: "h-12 w-12",
      xl: "h-16 w-16",
    },
  },
  defaultVariants: {
    size: "default",
  },
});

export interface OverlayLoaderProps
  extends
    React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof overlayVariants>,
    VariantProps<typeof spinnerVariants> {
  isLoading?: boolean;
  text?: string;
  spinnerClassName?: string;
}

const OverlayLoader = React.forwardRef<HTMLDivElement, OverlayLoaderProps>(
  (
    {
      className,
      variant,
      size,
      isLoading = true,
      text,
      spinnerClassName,
      ...props
    },
    ref
  ) => {
    if (!isLoading) return null;

    return (
      <div
        ref={ref}
        className={cn(overlayVariants({ variant }), className)}
        role="status"
        aria-live="polite"
        {...props}
      >
        <div className="flex flex-col items-center gap-4">
          <Loader2
            className={cn(spinnerVariants({ size }), spinnerClassName)}
          />
          {text && <p className="text-center font-medium">{text}</p>}
          <span className="sr-only">Loading</span>
        </div>
      </div>
    );
  }
);
OverlayLoader.displayName = "OverlayLoader";

export { OverlayLoader };
