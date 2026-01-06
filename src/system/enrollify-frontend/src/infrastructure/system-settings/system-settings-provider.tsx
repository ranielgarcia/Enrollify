import { getCurriculaSettingsQueryOptions } from "@/api/collections/curricula-settings-collection";
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
  const { data: curriculaSettings } = useSuspenseQuery(
    getCurriculaSettingsQueryOptions()
  );

  const contextValue = useMemo<ISystemSettingsContext>(
    () => ({
      curricularSettings: curriculaSettings,
    }),
    [curriculaSettings]
  );

  return (
    <SystemSettingsContext.Provider value={contextValue}>
      {children}
    </SystemSettingsContext.Provider>
  );
};
