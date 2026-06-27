import { useState } from "react";
import { CalendarDays } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  HoverCard,
  HoverCardContent,
  HoverCardTrigger,
} from "@/components/ui/hover-card";
import type { ClassSectionMinimal } from "@/api/models/class-scheduling/class-section";

import { SectionCardMiniSchedule } from "../section-card-mini-schedule";

interface SchedulePeekProps {
  section: ClassSectionMinimal;
  onViewDetails: (section: ClassSectionMinimal) => void;
}

/**
 * HoverCard-driven preview of a section's weekly schedule. Lazy-fetches
 * offering details only after the card opens (mini-schedule component reads
 * `isHovered` to gate its query).
 */
export function SchedulePeek({ section, onViewDetails }: SchedulePeekProps) {
  const [open, setOpen] = useState(false);

  return (
    <HoverCard
      open={open}
      onOpenChange={setOpen}
      openDelay={300}
      closeDelay={100}
    >
      <HoverCardTrigger asChild>
        <Button
          type="button"
          variant="ghost"
          size="sm"
          aria-label="Preview weekly schedule"
          className="gap-1.5 text-xs text-muted-foreground hover:text-foreground"
        >
          <CalendarDays className="size-3.5" />
          Preview
        </Button>
      </HoverCardTrigger>
      <HoverCardContent
        side="top"
        align="end"
        className="w-auto p-3"
        onOpenAutoFocus={(e) => e.preventDefault()}
      >
        <SectionCardMiniSchedule
          section={section}
          isHovered={open}
          onViewDetails={() => onViewDetails(section)}
        />
      </HoverCardContent>
    </HoverCard>
  );
}
