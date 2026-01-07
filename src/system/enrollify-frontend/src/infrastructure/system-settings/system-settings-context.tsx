/* eslint-disable react-refresh/only-export-components */
import { createAcademicSystem } from "@/api/models/academic-system-enum";
import type { CurriculumSettings } from "@/api/models/curriculum-settings";
import React, { use } from "react";

export interface ISystemSettingsContext {
  curriculumSettings: CurriculumSettings;
}

export const defaultSystemSettingsContext: ISystemSettingsContext = {
  curriculumSettings: {
    academicSystem: {
      name: createAcademicSystem("Semester").name,
      value: createAcademicSystem("Semester").value,
    },
  } as CurriculumSettings,
};

export const SystemSettingsContext =
  React.createContext<ISystemSettingsContext>(defaultSystemSettingsContext);

export const useSystemSettingsContext = () => use(SystemSettingsContext);

export const SystemSettingsConsumer = SystemSettingsContext.Consumer;
