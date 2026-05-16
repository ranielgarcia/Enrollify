import { filterSectionsPaginatedOptions } from "@/api/collections/class-section-collection";
import { getAllCoursesOptions } from "@/api/collections/course-collection";
import { getAllCurriculumsOptions } from "@/api/collections/curriculum-collection";
import { filterTeachersPaginatedOptions } from "@/api/collections/teacher-collection";
import type { ClassSection } from "@/api/models/class-section";
import { ManagementPageLayout } from "@/components/page-layouts/management-page-layout";
import { ModuleIcons } from "@/config/module-icons";
import { useCrudState } from "@/hooks/use-crud-state";
import { useDebounce } from "@/hooks/use-debounce";
import { useSuspenseQuery } from "@tanstack/react-query";
import { useQueryStates } from "nuqs";
import { SectionFormDrawer } from "./section-form-drawer";
import { BulkInitializeSectionsDrawer } from "./bulk-initialize-sections-drawer";
import { SectionsTable } from "./sections-table";
import { DeleteSectionAlertDialog } from "./delete-section-alert-dialog";
import { searchParams } from "./searchParams";
import { useState } from "react";
import { useEnrollmentContext } from "@/contexts/enrollment-context/enrollment-context";

export default function SectionsManagementPage() {
  const [{ page, perPage, filters, sort, joinOperator }] =
    useQueryStates(searchParams);

  const { selectedAcademicYear } = useEnrollmentContext();

  const {
    isFormOpen,
    entityToEdit: sectionToEdit,
    entityToDelete: sectionToDelete,
    handleEdit,
    handleDelete,
    handleFormOpenChange,
    handleDeleteDialogOpenChange,
  } = useCrudState<ClassSection>();

  const [isBulkInitializeOpen, setIsBulkInitializeOpen] = useState(false);

  const currentPage = page ? Number(page) : 1;
  const currentPageSize = perPage ? Number(perPage) : 10;

  const debouncedFilters = useDebounce(filters, 600);
  const debouncedSort = useDebounce(sort, 600);

  const { data: pagedSections } = useSuspenseQuery(
    filterSectionsPaginatedOptions(
      currentPage,
      currentPageSize,
      debouncedFilters,
      debouncedSort,
      joinOperator,
    ),
  );

  const { data: courses } = useSuspenseQuery(getAllCoursesOptions());
  const { data: curricula } = useSuspenseQuery(getAllCurriculumsOptions(true));
  const { data: teachers } = useSuspenseQuery(
    filterTeachersPaginatedOptions(1, 10, [], [], "and"),
  );

  // const { data: academicYearTimeline } = useSuspenseQuery(
  //   getAcademicYearTimeLineWindowOptions({
  //     IncludeFutureYears: true,
  //     IncludePastYears: true,
  //     NumberOfFutureYears: 2,
  //     NumberOfPastYears: 5,
  //   }),
  // );

  // const futureAcademicYearsOptions: SearchableSelectOption[] =
  //   academicYearTimeline?.future?.map((year) => ({
  //     value: year.id.toString(),
  //     label: year.academicYearTitle ?? "<invalid academic year title...>",
  //   })) ?? [];
  // const previousAcademicYearsOptions: SearchableSelectOption[] =
  //   academicYearTimeline?.previous?.map((year) => ({
  //     value: year.id.toString(),
  //     label: year.academicYearTitle ?? "<invalid academic year title...>",
  //   })) ?? [];
  // const currentAcademicYearOption: SearchableSelectOption | null =
  //   academicYearTimeline?.current
  //     ? {
  //         value: academicYearTimeline.current.id.toString(),
  //         label: `${academicYearTimeline.current.academicYearTitle ?? "<invalid academic year title...>"} (active)`,
  //       }
  //     : null;

  // const academicYearOptions = [
  //   ...futureAcademicYearsOptions,
  //   ...previousAcademicYearsOptions,
  // ];
  // if (currentAcademicYearOption) {
  //   academicYearOptions.push(currentAcademicYearOption);
  // }

  {
    /* <SearchableSelect
            options={academicYearOptions}
            value={field.state.value?.toString() ?? ""}
            onValueChange={(val) => {
              const parsed = Number(val);
              field.handleChange(Number.isNaN(parsed) ? val : parsed);
              onValueChange?.(val);
            }}
            name={field.name}
            placeholder={placeholder}
            searchPlaceholder={searchPlaceholder}
            emptyMessage={emptyMessage}
            disabled={disabled}
            aria-describedby={describedBy}
          /> */
  }

  return (
    <ManagementPageLayout
      title={`Class Sections of ${selectedAcademicYear?.academicYearTitle ?? "<invalid academic year...>"}`}
      description="Manage class sections, assign subjects, and build weekly schedules"
      icon={<ModuleIcons.sections />}
      createNewItemButton={
        <div className="flex gap-2">
          <BulkInitializeSectionsDrawer
            isOpen={isBulkInitializeOpen}
            setIsOpen={setIsBulkInitializeOpen}
            onOpenChange={setIsBulkInitializeOpen}
            courses={courses ?? []}
            curricula={curricula ?? []}
            academicTerms={[]}
          />
          <SectionFormDrawer
            key={sectionToEdit?.id ?? "new"}
            courses={courses ?? []}
            teachers={teachers?.items ?? []}
            academicTerms={[]}
            onOpenChange={handleFormOpenChange}
            sectionToUpdate={sectionToEdit}
            isOpen={isFormOpen}
            setIsOpen={handleFormOpenChange}
          />
        </div>
      }
    >
      <SectionsTable
        pagedSections={pagedSections}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />

      <DeleteSectionAlertDialog
        sectionToDelete={sectionToDelete}
        isOpen={!!sectionToDelete}
        onOpenChange={handleDeleteDialogOpenChange}
      />
    </ManagementPageLayout>
  );
}
