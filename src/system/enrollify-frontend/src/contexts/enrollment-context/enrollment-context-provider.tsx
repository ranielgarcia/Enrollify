import React, { useEffect } from "react";
import {
  EnrollmentContext,
  type EnrollmentContextValue,
} from "./enrollment-context";
import { getAcademicSettingsQueryOptions } from "@/api/collections/academic-settings-collection";
import { useQuery } from "@tanstack/react-query";
import { defaultSystemSettingsContext } from "@/infrastructure/system-settings/system-settings-context";
import { useMsal } from "@azure/msal-react";
import { getAcademicYearTimeLineWindowOptions } from "@/api/collections/academic-year-collection";
import { useQueryState } from "nuqs";

export const EnrollmentContextProvider = ({
  children,
}: Readonly<{
  children: React.ReactNode;
}>): React.ReactElement => {
  const { accounts } = useMsal();
  const isAuthenticated = accounts.length > 0;

  const [selectedAcademicYearSlug, setSelectedAcademicYearSlug] = useQueryState<
    string | null
  >("academicYear", {
    parse: (value) => {
      return value;
    },
    serialize: (value) => value ?? "",
    defaultValue: null,
    clearOnDefault: true,
    shallow: false,
    eq: (a, b) => (!a && !b) || a === b,
  });

  const { data: academicSettings } = useQuery({
    ...getAcademicSettingsQueryOptions(),
    enabled: isAuthenticated, // Only fetch when user is authenticated
  });

  const { data: academicYearTimelineWindow } = useQuery({
    ...getAcademicYearTimeLineWindowOptions({
      IncludeFutureYears: true,
      IncludePastYears: true,
      NumberOfFutureYears: 2,
      NumberOfPastYears: 2,
    }),
    enabled: isAuthenticated, // Only fetch when user is authenticated
  });

  const academicYears = React.useMemo(() => {
    return [
      ...(academicYearTimelineWindow?.current
        ? [academicYearTimelineWindow.current]
        : []),
      ...(academicYearTimelineWindow?.future ?? []),
      ...(academicYearTimelineWindow?.previous ?? []),
    ];
  }, [academicYearTimelineWindow]);

  // Only initialize the slug when none is set yet — don't overwrite a user's manual selection
  useEffect(() => {
    if (
      selectedAcademicYearSlug == null &&
      academicYearTimelineWindow?.current
    ) {
      setSelectedAcademicYearSlug(
        academicYearTimelineWindow.current.academicYearSlug,
      );
    }
  }, [
    academicYearTimelineWindow,
    selectedAcademicYearSlug,
    setSelectedAcademicYearSlug,
  ]);

  const selectedAcademicYear = React.useMemo(() => {
    return (
      academicYears.find(
        (y) => y.academicYearSlug === selectedAcademicYearSlug,
      ) ?? null
    );
  }, [academicYears, selectedAcademicYearSlug]);

  const contextValue = React.useMemo<EnrollmentContextValue>(
    () => ({
      academicSettings:
        academicSettings ?? defaultSystemSettingsContext.academicSettings,
      activeAcademicYear: academicYearTimelineWindow?.current ?? null,
      academicYears,
      selectedAcademicYearSlug: selectedAcademicYearSlug,
      selectedAcademicYear,
      setSelectedAcademicYearSlug: setSelectedAcademicYearSlug,
    }),
    [
      academicSettings,
      academicYearTimelineWindow,
      academicYears,
      selectedAcademicYearSlug,
      selectedAcademicYear,
      setSelectedAcademicYearSlug,
    ],
  );

  return (
    <EnrollmentContext.Provider value={contextValue}>
      {children}
    </EnrollmentContext.Provider>
  );
};
