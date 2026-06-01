import { BookOpen, OctagonAlert } from "lucide-react";

import { useNavigate, useParams } from "@tanstack/react-router";
import {
  getAllCurriculumsOptions,
  getCurriculumQueryOption,
} from "@/api/collections/curriculum-collection";
import CurriculumBasicDetails from "./curriculum-basic-details";
import { obfuscator } from "@/lib/obfuscator";
import { useQuery, useSuspenseQuery } from "@tanstack/react-query";
import MultiYearSubjectGridEditor from "./multi-year-subject-grid-editor";
import { CurriculumsTable } from "./curriculums-table";
import { ManagementPageLayout } from "@/components/page-layouts/management-page-layout";
import { useCrudState } from "@/hooks/use-crud-state";
import type { CurriculumWithSubjects } from "@/api/models/curriculum";
import { getAllCoursesOptions } from "@/api/collections/course-collection";
import { CurriculumFormDrawer } from "./curriculum-form-drawer";
import { ModuleIcons } from "@/config/module-icons";

export default function CurriculumPage() {
  const navigate = useNavigate();
  const { curriculumId } = useParams({ strict: false });

  const { isFormOpen, handleEdit, handleFormOpenChange } =
    useCrudState<CurriculumWithSubjects>();

  // TODO: Refactor to avoid fetching all curriculums when curriculumId is present. This is currently needed to determine whether to show the "not found" state,
  // but can be optimized by having a separate query that just checks for existence of the curriculum with the given ID.
  // https://github.com/ranielgarcia/Enrollify/issues/83
  const { data: curriculum, isPending: isLoadingCurriculum } = useQuery(
    getCurriculumQueryOption(obfuscator.decode(curriculumId ?? "").at(0)),
  );

  const { data: curriculums } = useSuspenseQuery(
    getAllCurriculumsOptions(curriculumId === undefined),
  );

  const { data: courses } = useSuspenseQuery(getAllCoursesOptions());

  return (
    <ManagementPageLayout
      title="Curriculum Builder"
      description="Design and manage multi-year academic curricula"
      icon={<ModuleIcons.curriculum />}
      createNewItemButton={
        <CurriculumFormDrawer
          key={curriculum?.id ?? "new"}
          courses={courses}
          onOpenChange={handleFormOpenChange}
          curriculumToUpdate={curriculum}
          isOpen={isFormOpen}
          setIsOpen={handleFormOpenChange}
          disabled={!!curriculum}
        />
      }
      isLoading={!!curriculumId && isLoadingCurriculum}
    >
      {curriculum ? (
        <div className="space-y-8 animate-in fade-in duration-500">
          <CurriculumBasicDetails
            curriculum={curriculum}
            onEditDetails={() => handleEdit(curriculum)}
            onClose={() => {
              navigate({
                to: "/portal/curriculum-and-scheduling/curriculum/{-$curriculumId}",
                params: (prev) => ({
                  ...prev,
                  curriculumId: undefined,
                }),
              });
            }}
          />

          {/* Multi-Year Subject Grid */}
          <MultiYearSubjectGridEditor curriculum={curriculum} />
        </div>
      ) : curriculums && curriculums?.length > 0 ? (
        <CurriculumsTable
          curriculums={curriculums}
          onEdit={(item) => {
            const obfuscatedId = obfuscator.encode([item.id]);
            navigate({
              to: "/portal/curriculum-and-scheduling/curriculum/{-$curriculumId}",
              params: (prev) => ({ ...prev, curriculumId: obfuscatedId }),
            });
          }}
        />
      ) : (
        <div className="flex flex-col items-center justify-center py-24 gap-5 text-center">
          {curriculumId && !isLoadingCurriculum ? (
            <>
              <div className="flex h-20 w-20 items-center justify-center rounded-2xl bg-muted/60 border border-dashed">
                <OctagonAlert className="h-9 w-9 text-muted-foreground/60" />
              </div>
              <div className="space-y-1.5">
                <h3 className="text-base font-semibold tracking-tight">
                  Curriculum Not Found
                </h3>
                <p className="text-sm text-muted-foreground max-w-xs">
                  The selected curriculum could not be found. Use the{" "}
                  <strong className="text-foreground font-medium">
                    New Curriculum
                  </strong>{" "}
                  button to create one.
                </p>
              </div>
            </>
          ) : (
            <>
              <div className="flex h-20 w-20 items-center justify-center rounded-2xl bg-muted/60 border border-dashed">
                <BookOpen className="h-9 w-9 text-muted-foreground/60" />
              </div>
              <div className="space-y-1.5">
                <h3 className="text-base font-semibold tracking-tight">
                  No Curriculum Started
                </h3>
                <p className="text-sm text-muted-foreground max-w-xs">
                  There are no curricula yet. Use the{" "}
                  <strong className="text-foreground font-medium">
                    New Curriculum
                  </strong>{" "}
                  button above to design a new academic curriculum.
                </p>
              </div>
            </>
          )}
        </div>
      )}
    </ManagementPageLayout>
  );
}
