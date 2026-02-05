import { useQuery } from "@tanstack/react-query";
import { useMemo } from "react";
import {
  SystemSettingsContext,
  defaultSystemSettingsContext,
  type ISystemSettingsContext,
} from "./system-settings-context";
import { getAcademicSettingsQueryOptions } from "@/api/collections/academic-settings-collection";
import { useMsal } from "@azure/msal-react";

export const SystemSettingsProvider = ({
  children,
}: Readonly<{
  children: React.ReactNode;
}>): React.ReactElement => {
  const { accounts } = useMsal();
  const isAuthenticated = accounts.length > 0;

  const { data: academicSettings } = useQuery({
    ...getAcademicSettingsQueryOptions(),
    enabled: isAuthenticated, // Only fetch when user is authenticated
  });

  const contextValue = useMemo<ISystemSettingsContext>(
    () => ({
      academicSettings:
        academicSettings ?? defaultSystemSettingsContext.academicSettings,
    }),
    [academicSettings],
  );

  return (
    <SystemSettingsContext.Provider value={contextValue}>
      {children}
    </SystemSettingsContext.Provider>
  );
};
