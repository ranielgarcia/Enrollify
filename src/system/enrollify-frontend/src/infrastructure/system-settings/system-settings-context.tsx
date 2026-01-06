/* eslint-disable react-refresh/only-export-components */
import { createAcademicSystem } from "@/api/models/academic-system-enum";
import type { CurriculaSettings } from "@/api/models/curricula-settings";
import React, { use } from "react";

export interface ISystemSettingsContext {
  curricularSettings: CurriculaSettings;
}

export const defaultSystemSettingsContext: ISystemSettingsContext = {
  curricularSettings: {
    academicSystem: {
      name: createAcademicSystem("Semester").name,
      value: createAcademicSystem("Semester").value,
    },
  } as CurriculaSettings,
};

export const SystemSettingsContext =
  React.createContext<ISystemSettingsContext>(defaultSystemSettingsContext);

export const useSystemSettingsContext = () => use(SystemSettingsContext);

export const SystemSettingsConsumer = SystemSettingsContext.Consumer;
