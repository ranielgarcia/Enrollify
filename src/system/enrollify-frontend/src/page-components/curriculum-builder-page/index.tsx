import { useState, Suspense } from "react";
import { CurriculumForm } from "./curriculum-form";
import { Button } from "@/components/ui/button";
import { Plus, BookOpen, OctagonAlert, Scroll } from "lucide-react";

import { useNavigate, useParams } from "@tanstack/react-router";
import {
  getAllCurriculumsOptions,
  getCurriculumQueryOption,
} from "@/api/collections/curriculum-collection";
import CurriculumBasicDetails from "./curriculum-basic-details";
import { obfuscator } from "@/lib/obfuscator";
import { useQuery } from "@tanstack/react-query";
import MultiYearSubjectGridEditor from "./multi-year-subject-grid-editor";
import { CurriculumsTable } from "./curriculums-table";
import { ManagementPageLayout } from "@/components/management-page-layout";

function CurriculumContent() {
  const navigate = useNavigate();
  const { curriculumId } = useParams({ strict: false });

  const [showForm, setShowForm] = useState(false);

  const { data: curriculum, isPending: isLoadingCurriculum } = useQuery(
    getCurriculumQueryOption(obfuscator.decode(curriculumId ?? "").at(0)),
  );

  const { data: curriculums } = useQuery(
    getAllCurriculumsOptions(curriculumId === undefined),
  );

  return (
    <ManagementPageLayout
      title="Curriculum Builder"
      description="Design and manage multi-year academic curricula"
      icon={<Scroll />}
      createNewItemButton={
        !curriculum &&
        !showForm && (
          <Button
            onClick={() => setShowForm(true)}
            className="gap-2 bg-primary text-primary-foreground hover:bg-primary/90 cursor-pointer"
            size="sm"
          >
            <Plus className="size-4" />
            Create New Curriculum
          </Button>
        )
      }
      isLoading={!!curriculumId && isLoadingCurriculum}
    >
      {showForm && (
        <div className="mb-8">
          <CurriculumForm
            onClose={() => setShowForm(false)}
            onSave={(curriculumId) => {
              const obfuscatedId = obfuscator.encode([curriculumId]);
              navigate({
                to: "/portal/curriculum-and-scheduling/curriculum/{-$curriculumId}",
                params: (prev) => ({ ...prev, curriculumId: obfuscatedId }),
              });
              setShowForm(false);
            }}
            curriculumToUpdate={curriculum}
          />
        </div>
      )}

      {curriculum ? (
        <div className="space-y-8 animate-in fade-in duration-500">
          {!showForm && (
            <CurriculumBasicDetails
              curriculum={curriculum}
              onEditDetails={() => {
                setShowForm(true);
              }}
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
          )}

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
