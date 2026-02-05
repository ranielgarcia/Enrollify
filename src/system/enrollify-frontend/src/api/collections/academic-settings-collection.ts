import createAppQueryOptions, {
  createAppSuspenseQueryOptions,
} from "@/hooks/create-query-options";
import {
  AcademicSettingsSchema,
  type AcademicSettings,
} from "../models/academic-settings";

const queryKeys = {
  AcademicSettings: () => ["Academic-settings"],
};

export const getAcademicSettingsQueryOptions = () =>
  createAppQueryOptions({
    path: "/api/system-settings/academic-settings",
    options: {
      meta: { persist: true },
      queryKey: queryKeys.AcademicSettings(),
      staleTime: 1000 * 60 * 5,
      gcTime: 1000 * 60 * 60 * 24,
      select: (settings): AcademicSettings => {
        return AcademicSettingsSchema.parse(settings);
      },
    },
  });

export const getAcademicSettingsSuspenseQueryOptions = () =>
  createAppSuspenseQueryOptions({
    path: "/api/system-settings/academic-settings",
    options: {
      meta: { persist: true },
      queryKey: queryKeys.AcademicSettings(),
      staleTime: 1000 * 60 * 5,
      gcTime: 1000 * 60 * 60 * 24,
      select: (settings): AcademicSettings => {
        return AcademicSettingsSchema.parse(settings);
      },
    },
  });
