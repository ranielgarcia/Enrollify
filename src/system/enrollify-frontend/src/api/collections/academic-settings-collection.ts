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
      // persist: true enables localStorage persistence across page reloads
      meta: { persist: true },
      queryKey: queryKeys.AcademicSettings(),
      // staleTime: 10 minutes - how long before data is considered stale
      staleTime: 1000 * 60 * 10,
      // gcTime: 1 hours - how long to keep unused data in cache memory
      gcTime: 1000 * 60 * 60,
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
      // staleTime: 10 minutes - how long before data is considered stale
      staleTime: 1000 * 60 * 10,
      // gcTime: 24 hours - how long to keep unused data in cache memory
      gcTime: 1000 * 60 * 60 * 24,
      select: (settings): AcademicSettings => {
        return AcademicSettingsSchema.parse(settings);
      },
    },
  });
