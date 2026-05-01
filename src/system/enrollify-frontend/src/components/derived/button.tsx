import * as React from "react";
import {
  Button as PrimitiveButton,
  buttonVariants,
} from "@/components/ui/button";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import type { VariantProps } from "class-variance-authority";
import type { TooltipContentProps } from "@radix-ui/react-tooltip";

type DerivedButtonProps = React.ComponentProps<"button"> &
  VariantProps<typeof buttonVariants> & {
    asChild?: boolean;
    tooltip?: React.ReactNode;
    tooltipSide?: TooltipContentProps["side"];
    tooltipSideOffset?: number;
    tooltipClassName?: string;
  };

function DerivedButton({
  tooltip,
  tooltipSide = "top",
  tooltipSideOffset,
  tooltipClassName,
  ...buttonProps
}: DerivedButtonProps) {
  if (!tooltip) {
    return <PrimitiveButton {...buttonProps} />;
  }

  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <PrimitiveButton {...buttonProps} />
      </TooltipTrigger>
      <TooltipContent
        side={tooltipSide}
        sideOffset={tooltipSideOffset}
        className={tooltipClassName}
      >
        {tooltip}
      </TooltipContent>
    </Tooltip>
  );
}

export { DerivedButton };
