import { useState, Suspense } from "react";
import { CurriculumForm } from "./curriculum-form";
import { Button } from "@/components/ui/button";
import { Plus, BookOpen, OctagonAlert } from "lucide-react";

import { useNavigate, useParams } from "@tanstack/react-router";
import {
  getAllCurriculumsOptions,
  getCurriculumQueryOption,
} from "@/api/collections/curriculum-collection";
import CurriculumBasicDetails from "./curriculum-basic-details";
import { obfuscator } from "@/lib/obfuscator";
import { useQuery } from "@tanstack/react-query";
import MultiYearSubjectGridEditor from "./multi-year-subject-grid-editor";
import { OverlayLoader } from "@/components/app-loading-overlay";
import { CurriculumsTable } from "./curriculums-table";

function CurriculumContent() {
  const navigate = useNavigate();
  const { curriculumId } = useParams({ strict: false });

  const [showForm, setShowForm] = useState(false);

  const { data: curriculum, isPending: isLoadingCurriculum } = useQuery(
    getCurriculumQueryOption(obfuscator.decode(curriculumId ?? "").at(0))
  );

  const { data: curriculums } = useQuery(
    getAllCurriculumsOptions(curriculumId === undefined)
  );

  return (
    <main>
      <OverlayLoader
        isLoading={!!curriculumId && isLoadingCurriculum}
        text="Loading Curriculum"
        size="sm"
      />

      <div className="p-4 md:p-8">
        <div className="mb-8 flex flex-col md:flex-row md:items-center md:justify-between gap-4">
          <div className="mb-8">
            <h1 className="text-3xl font-bold text-foreground mb-2">
              Curriculum Builder
            </h1>
            <p className="text-muted-foreground">
              Design and manage multi-year academic curricula
            </p>
          </div>
          {!curriculum && !showForm && (
            <Button onClick={() => setShowForm(true)} className="gap-2">
              <Plus className="size-4" />
              Create New Curriculum
            </Button>
          )}
        </div>

        {showForm && (
          <div className="mb-8">
            <CurriculumForm
              onClose={() => setShowForm(false)}
              onSave={(curriculumId) => {
                const obfuscatedId = obfuscator.encode([curriculumId]);
                navigate({
                  to: "/portal/master-data/curriculum/{-$curriculumId}",
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
                  console.log("show form");
                  setShowForm(true);
                }}
                onClose={() => {
                  navigate({
                    to: "/portal/master-data/curriculum/{-$curriculumId}",
                    params: (prev) => ({
                      ...prev,
                      curriculumId: undefined,
                    }),
                  });
                }}
              />
            )}

            {/* Multi-Year Subject Grid */}
            <MultiYearSubjectGridEditor curriculumId={curriculum?.id} />
          </div>
        ) : curriculums && curriculums?.length > 0 ? (
          <CurriculumsTable
            curriculums={curriculums}
            onEdit={(item) => {
              const obfuscatedId = obfuscator.encode([item.id]);
              navigate({
                to: "/portal/master-data/curriculum/{-$curriculumId}",
                params: (prev) => ({ ...prev, curriculumId: obfuscatedId }),
              });
            }}
          />
        ) : (
          <div className="flex flex-col items-center justify-center py-32 bg-muted/20 rounded-xl border-2 border-dashed">
            {curriculumId && !curriculum ? (
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
      </div>
    </main>
  );
}

export default function CurriculumPage() {
  return (
    <Suspense fallback={null}>
      <CurriculumContent />
    </Suspense>
  );
}
