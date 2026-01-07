import { getCurriculumSettingsQueryOptions } from "@/api/collections/curriculum-settings-collection";
import { useSuspenseQuery } from "@tanstack/react-query";
import { useMemo } from "react";
import {
  SystemSettingsContext,
  type ISystemSettingsContext,
} from "./system-settings-context";

export const SystemSettingsProvider = ({
  children,
}: Readonly<{
  children: React.ReactNode;
}>): React.ReactElement => {
  const { data: curriculumSettings } = useSuspenseQuery(
    getCurriculumSettingsQueryOptions()
  );

  const contextValue = useMemo<ISystemSettingsContext>(
    () => ({
      curriculumSettings: curriculumSettings,
    }),
    [curriculumSettings]
  );

  return (
    <SystemSettingsContext.Provider value={contextValue}>
      {children}
    </SystemSettingsContext.Provider>
  );
};
