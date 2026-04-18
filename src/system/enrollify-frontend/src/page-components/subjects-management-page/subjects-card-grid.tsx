import type { PagedResult } from "@/api/models/paged-result";
import type { Subject } from "@/api/models/subject";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import {
  BookOpen,
  Edit2,
  Trash2,
  ChevronLeft,
  ChevronRight,
} from "lucide-react";
import { useTablePermissions } from "@/hooks/use-table-permissions";

interface SubjectsCardGridProps {
  pagedSubjects: PagedResult<Subject>;
  onEdit: (subject: Subject) => void;
  onDelete: (subject: Subject) => void;
  onPreviousPage: () => void;
  onNextPage: () => void;
}

export function SubjectsCardGrid({
  pagedSubjects,
  onEdit,
  onDelete,
  onPreviousPage,
  onNextPage,
}: SubjectsCardGridProps) {
  const { canUpdate, canDelete } = useTablePermissions("canUpdateSubject", "canDeleteSubject");

  const subjects= pagedSubjects?.items ?? [];

  return (
    <div className="space-y-6">
      {/* Grid of Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5 gap-4">
        {subjects.map((subject) => (
          <Card
            key={subject.id}
            className="hover:border-accent transition-colors"
          >
            <CardContent className="pt-6">
              <div className="flex items-start justify-between">
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 mb-2">
                    <BookOpen className="size-4 text-accent shrink-0" />
                    <span className="text-sm font-bold text-accent truncate">
                      {subject.code}
                    </span>
                  </div>
                  <h3 className="font-semibold text-foreground text-lg line-clamp-2">
                    {subject.title}
                  </h3>
                  <p className="text-sm text-muted-foreground mt-1 line-clamp-2">
                    {subject.description || "No description"}
                  </p>
                  <div className="mt-4 flex flex-wrap items-center gap-2">
                    <span className="text-xs bg-accent/10 text-accent px-2 py-1 rounded-full font-medium">
                      {Number(subject.units).toFixed(1)} Units
                    </span>
                    {/* Placeholder for equivalence badge - will show when data is available */}
                    {/* <Badge variant="outline" className="text-xs cursor-pointer hover:bg-muted" onClick={() => onViewEquivalents?.(subject)}>
                      <Link2 className="size-3 mr-1" />
                      2 equivalents
                    </Badge> */}
                  </div>
                </div>
                <div className="flex flex-col gap-2 ml-2">
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => onEdit(subject)}
                    disabled={!canUpdate}
                    className="hover:bg-blue-500/10 text-blue-600 hover:text-blue-700"
                  >
                    <Edit2 className="size-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => onDelete(subject)}
                    disabled={!canDelete}
                    className="text-destructive hover:bg-destructive/10"
                  >
                    <Trash2 className="size-4" />
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Empty State */}
      {subjects.length === 0 && (
        <div className="text-center py-12 text-muted-foreground">
          <BookOpen className="size-12 mx-auto mb-4 opacity-50" />
          <p>No subjects found</p>
        </div>
      )}

      {/* Pagination */}
      <div className="flex items-center justify-between border-t pt-4">
        <p className="text-sm text-muted-foreground">
          Page {pagedSubjects?.page ?? 1} of {pagedSubjects?.totalPages ?? 1}
          {" · "}
          {pagedSubjects?.totalCount ?? 0} total subjects
        </p>
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={onPreviousPage}
            disabled={(pagedSubjects?.page ?? 1) <= 1}
          >
            <ChevronLeft className="size-4 mr-1" />
            Previous
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={onNextPage}
            disabled={
              (pagedSubjects?.page ?? 1) >= (pagedSubjects?.totalPages ?? 1)
            }
          >
            Next
            <ChevronRight className="size-4 ml-1" />
          </Button>
        </div>
      </div>
    </div>
  );
}
