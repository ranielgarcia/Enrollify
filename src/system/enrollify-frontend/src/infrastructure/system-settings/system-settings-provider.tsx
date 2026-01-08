import { useSuspenseQuery } from "@tanstack/react-query";
import { useMemo } from "react";
import {
  SystemSettingsContext,
  type ISystemSettingsContext,
} from "./system-settings-context";
import { getAcademicSettingsQueryOptions } from "@/api/collections/academic-settings-collection";

export const SystemSettingsProvider = ({
  children,
}: Readonly<{
  children: React.ReactNode;
}>): React.ReactElement => {
  const { data: academicSettings } = useSuspenseQuery(
    getAcademicSettingsQueryOptions()
  );

  const contextValue = useMemo<ISystemSettingsContext>(
    () => ({
      academicSettings,
    }),
    [academicSettings]
  );

  return (
    <SystemSettingsContext.Provider value={contextValue}>
      {children}
    </SystemSettingsContext.Provider>
  );
};
