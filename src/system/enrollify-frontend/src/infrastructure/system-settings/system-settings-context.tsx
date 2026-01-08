/* eslint-disable react-refresh/only-export-components */
import type { AcademicSettings } from "@/api/models/academic-settings";
import React, { use } from "react";

export interface ISystemSettingsContext {
  academicSettings: AcademicSettings;
}

export const defaultSystemSettingsContext: ISystemSettingsContext = {
  academicSettings: {
    academicSystem: 0,
  } as AcademicSettings,
};

export const SystemSettingsContext =
  React.createContext<ISystemSettingsContext>(defaultSystemSettingsContext);

export const useSystemSettingsContext = () => use(SystemSettingsContext);

export const SystemSettingsConsumer = SystemSettingsContext.Consumer;
