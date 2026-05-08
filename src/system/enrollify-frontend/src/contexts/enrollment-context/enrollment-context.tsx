import type { AcademicSettings } from "@/api/models/academic-settings";
import type { AcademicYear } from "@/api/models/academic-year";
import React from "react";

export interface EnrollmentContextValue {
  academicSettings: AcademicSettings;
  activeAcademicYear: AcademicYear | null;
  academicYears: AcademicYear[];
  selectedAcademicYearSlug: string | null;
  selectedAcademicYear: AcademicYear | null;
  setSelectedAcademicYearSlug: (slug: string | null) => void;
}

export const defaultEnrollmentContextValue: EnrollmentContextValue = {
  academicSettings: {
    academicSystem: 0,
  },
  activeAcademicYear: null,
  academicYears: [],
  selectedAcademicYearSlug: null,
  selectedAcademicYear: null,
  setSelectedAcademicYearSlug: () => {},
};

export const EnrollmentContext = React.createContext<EnrollmentContextValue>(
  defaultEnrollmentContextValue,
);

export const useEnrollmentContext = () => React.use(EnrollmentContext);
