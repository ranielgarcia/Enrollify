import {
  getActiveAcademicYearOptions,
  getFutureAcademicYearsOptions,
  getPreviousAcademicYearsOptions,
} from "@/api/collections/academic-year-collection";
import type { AcademicYear } from "@/api/models/academic-year";
import { ManagementPageLayout } from "@/components/page-layouts/management-page-layout";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { ModuleIcons } from "@/config/module-icons";
import { useCrudState } from "@/hooks/use-crud-state";
import { useSuspenseQuery } from "@tanstack/react-query";
import { CalendarDays, CalendarPlus, FolderClock, History } from "lucide-react";
import { useState } from "react";
import { ActiveAcademicYearTab } from "./active-academic-year-tab";
import { CreateAcademicYearForm } from "./create-academic-year-form";
import { PreviousAcademicYearsTab } from "./previous-academic-years-tab";
import { DeleteAcademicYearAlertDialog } from "./delete-academic-year-alert-dialog";
import { FutureAcademicYearsTab } from "./future-academic-years-tab";

const TAB_LABELS = {
  active: "Active Academic Year",
  create: "New Academic Year",
  future: "Future Academic Years",
  previous: "Previous Academic Years",
} as const;

export default function AcademicYearManagementPage() {
  const [activeTab, setActiveTab] = useState(
    () => sessionStorage.getItem("academic-year-tab") ?? "active",
  );

  const {
    entityToEdit,
    entityToDelete,
    handleEdit,
    handleDelete,
    handleDeleteDialogOpenChange,
    handleFormOpenChange,
  } = useCrudState<AcademicYear>();

  const { data: activeYear, isPending: isLoadingActiveYear } = useSuspenseQuery(
    getActiveAcademicYearOptions(),
  );

  const { data: previousYears, isPending: isLoadingPreviousYears } =
    useSuspenseQuery(getPreviousAcademicYearsOptions());

  const { data: futureYears, isPending: isLoadingFutureYears } =
    useSuspenseQuery(getFutureAcademicYearsOptions());

  const handleTabChange = (value: string) => {
    setActiveTab(value);
    // Don't persist "create" — edit context doesn't survive refresh
    if (value !== "create") {
      sessionStorage.setItem("academic-year-tab", value);
    } else {
      sessionStorage.removeItem("academic-year-tab");
    }
  };

  /** Called from the Active tab "Edit" button — pre-fills the create form */
  const handleEditActiveYear = () => {
    if (activeYear) handleEdit(activeYear);
    handleTabChange("create");
  };

  /** Called after successful form submission */
  const handleFormSuccess = () => {
    handleTabChange("active");
  };

  /** Cancel editing — clear state and go to active tab */
  const handleFormCancel = () => {
    handleTabChange("active");
    handleFormOpenChange(false);
  };

  return (
    <ManagementPageLayout
      title="Academic Year Management"
      description="Manage academic years and their terms (semesters)"
      icon={<ModuleIcons.academicYear className="size-9 text-primary" />}
      isLoading={
        isLoadingActiveYear || isLoadingPreviousYears || isLoadingFutureYears
      }
      createNewItemButton={null}
    >
      <Tabs
        value={activeTab}
        onValueChange={handleTabChange}
        className="min-h-0 flex-1 space-y-6"
      >
        <TabsList variant="line">
          <TabsTrigger value="active" className="gap-2">
            <CalendarDays className="size-4" />
            {TAB_LABELS.active}
          </TabsTrigger>
          <TabsTrigger value="create" className="gap-2">
            <CalendarPlus className="size-4" />
            {entityToEdit ? "Edit Academic Year" : TAB_LABELS.create}
          </TabsTrigger>
          <TabsTrigger value="future" className="gap-2">
            <FolderClock className="size-4" />
            {TAB_LABELS.future}
          </TabsTrigger>
          <TabsTrigger value="previous" className="gap-2">
            <History className="size-4" />
            {TAB_LABELS.previous}
          </TabsTrigger>
        </TabsList>

        {/* Tab 1 — Active Academic Year */}
        <TabsContent value="active" className="flex min-h-0 flex-col space-y-4">
          <ActiveAcademicYearTab
            activeYear={activeYear ?? null}
            onEdit={handleEditActiveYear}
            createTabLabel={TAB_LABELS.create}
          />
        </TabsContent>

        {/* Tab 2 — Create / Edit Form */}
        <TabsContent value="create" className="flex min-h-0 flex-col space-y-4">
          <CreateAcademicYearForm
            key={entityToEdit?.id ?? "new"}
            yearToEdit={entityToEdit}
            onSuccess={handleFormSuccess}
            onCancel={handleFormCancel}
          />
        </TabsContent>

        {/* Tab 3 — Future Academic Years */}
        <TabsContent value="future" className="flex min-h-0 flex-col space-y-4">
          <FutureAcademicYearsTab
            futureYears={futureYears ?? []}
            onDelete={handleDelete}
          />
        </TabsContent>

        {/* Tab 4 — Previous Academic Years */}
        <TabsContent
          value="previous"
          className="flex min-h-0 flex-col space-y-4"
        >
          <PreviousAcademicYearsTab previousYears={previousYears ?? []} />
        </TabsContent>
      </Tabs>

      <DeleteAcademicYearAlertDialog
        isOpen={!!entityToDelete}
        onOpenChange={handleDeleteDialogOpenChange}
        yearToDelete={entityToDelete}
      />
    </ManagementPageLayout>
  );
}
