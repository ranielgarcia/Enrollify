import type { Offering } from "@/api/models/offering";
import type { ValidationMessages } from "@/api/models/class-section";
import { OfferingCard } from "./offering-card";
import { LayoutGrid } from "lucide-react";

interface OfferingsTabProps {
  sectionId: number;
  offerings: Offering[];
  validationMessages?: ValidationMessages;
}

export function OfferingsTab({
  sectionId,
  offerings,
  validationMessages,
}: OfferingsTabProps) {
  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">
          {offerings.length > 0
            ? `${offerings.length} subject offering${offerings.length > 1 ? "s" : ""} assigned to this section`
            : "No offerings configured yet"}
        </p>
      </div>

      {offerings.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-16 gap-4 text-center">
          <div className="rounded-full bg-muted p-4">
            <LayoutGrid className="size-8 text-muted-foreground" />
          </div>
          <div>
            <p className="text-base font-semibold">No offerings yet</p>
            <p className="text-sm text-muted-foreground mt-1">
              Offerings are derived from the section's curriculum. Initialize
              the section to load offerings automatically.
            </p>
          </div>
        </div>
      ) : (
        <div className="space-y-3">
          {offerings.map((o) => (
            <OfferingCard
              key={o.id}
              offering={o}
              sectionId={sectionId}
              validationMessages={
                validationMessages?.offeringsValidationMessages[
                  String(o.id)
                ] ?? []
              }
            />
          ))}
        </div>
      )}
    </div>
  );
}
