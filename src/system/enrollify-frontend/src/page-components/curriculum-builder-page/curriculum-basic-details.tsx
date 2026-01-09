import type { Curriculum } from "@/api/models/curriculum";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";

interface CurriculumBasicDetailsProps {
  curriculum: Curriculum;
  onEditDetails: () => void;
  onClose: () => void;
}

export default function CurriculumBasicDetails({
  curriculum,
  onEditDetails,
  onClose,
}: CurriculumBasicDetailsProps) {
  return (
    <Card className="border-accent bg-accent/5">
      <CardContent className="pt-6">
        <div className="flex flex-wrap gap-6 items-center">
          <div>
            <p className="text-xs uppercase tracking-wider text-muted-foreground mb-1 font-semibold">
              Course
            </p>
            <p className="font-bold">{curriculum.course.name}</p>
          </div>
          <div>
            <p className="text-xs uppercase tracking-wider text-muted-foreground mb-1 font-semibold">
              Status
            </p>
            <Badge variant="outline" className="bg-background">
              {curriculum.status.name}
            </Badge>
          </div>
          <div>
            <p className="text-xs uppercase tracking-wider text-muted-foreground mb-1 font-semibold">
              Version
            </p>
            <p className="font-bold">{curriculum.version}</p>
          </div>
          <div>
            <p className="text-xs uppercase tracking-wider text-muted-foreground mb-1 font-semibold">
              Effective Year
            </p>
            <p className="font-bold">{curriculum.effectiveYear}</p>
          </div>
          <div className="flex-1">
            <p className="text-xs uppercase tracking-wider text-muted-foreground mb-1 font-semibold">
              Description
            </p>
            <p className="text-sm italic">
              {curriculum.description || "No description provided."}
            </p>
          </div>
          <Button variant="outline" size="sm" onClick={onEditDetails}>
            Edit Details
          </Button>
          <Button variant="outline" size="sm" onClick={onClose}>
            Close
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
