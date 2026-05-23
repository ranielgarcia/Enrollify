import type { AcademicCoreSettings } from "@/api/models/academic-core-settings";
import type { AcademicYear } from "@/api/models/academic-year";
import React from "react";

export interface EnrollmentContextValue {
  academicCoreSettings: AcademicCoreSettings;
  activeAcademicYear: AcademicYear | null;
  academicYears: AcademicYear[];
  selectedAcademicYearSlug?: string;
  selectedAcademicYear: AcademicYear | null;
  setSelectedAcademicYearSlug: (slug: string | null) => void;
}

export const defaultEnrollmentContextValue: EnrollmentContextValue = {
  academicCoreSettings: {
    academicTermSystem: 0,
    maximumAllowableYearLevel: 0,
    yearLevelOptions: [],
  },
  activeAcademicYear: null,
  academicYears: [],
  selectedAcademicYearSlug: undefined,
  selectedAcademicYear: null,
  setSelectedAcademicYearSlug: () => {},
};

export const EnrollmentContext = React.createContext<EnrollmentContextValue>(
  defaultEnrollmentContextValue,
);

export const useEnrollmentContext = () => React.use(EnrollmentContext);
