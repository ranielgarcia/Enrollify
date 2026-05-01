import { Suspense } from "react";
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

function CurriculumContent() {
  const navigate = useNavigate();
  const { curriculumId } = useParams({ strict: false });

  const { isFormOpen, handleEdit, handleFormOpenChange } =
    useCrudState<CurriculumWithSubjects>();

  const { data: curriculum, isPending: isLoadingCurriculum } = useQuery(
    getCurriculumQueryOption(obfuscator.decode(curriculumId ?? "").at(0)),
  );

  const { data: curriculums } = useQuery(
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
        <div className="flex flex-col items-center justify-center py-32 bg-muted/20 rounded-xl border-2 border-dashed">
          {curriculumId && !isLoadingCurriculum ? (
            <>
              <OctagonAlert className="size-16 text-muted-foreground/20 mb-4" />
              <h2 className="text-xl font-semibold text-foreground">
                Curriculum Not Found
              </h2>
              <p className="text-muted-foreground max-w-sm text-center mt-2">
                Click the button above to start designing a new academic
                curriculum.
              </p>
            </>
          ) : (
            <>
              <BookOpen className="size-16 text-muted-foreground/20 mb-4" />
              <h2 className="text-xl font-semibold text-foreground">
                No Curriculum Started
              </h2>
              <p className="text-muted-foreground max-w-sm text-center mt-2">
                Click the button above to start designing a new academic
                curriculum.
              </p>
            </>
          )}
        </div>
      )}
    </ManagementPageLayout>
  );
}

export default function CurriculumPage() {
  return (
    <Suspense fallback={null}>
      <CurriculumContent />
    </Suspense>
  );
}
