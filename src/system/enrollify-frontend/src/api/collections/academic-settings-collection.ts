import createAppQueryOptions from "@/hooks/create-query-options";
import {
  AcademicCoreSettingsSchema,
  type AcademicCoreSettings,
} from "../models/academic-core-settings";

const queryKeys = {
  AcademicCoreSettings: () => ["Academic-core-settings"],
};

export const getAcademicCoreSettingsQueryOptions = () =>
  createAppQueryOptions({
    path: "/api/system-settings/academic-core-settings",
    options: {
      // persist: true enables localStorage persistence across page reloads
      meta: { persist: true },
      queryKey: queryKeys.AcademicCoreSettings(),
      // staleTime: 10 minutes - how long before data is considered stale
      staleTime: 1000 * 60 * 10,
      // gcTime: 1 hours - how long to keep unused data in cache memory
      gcTime: 1000 * 60 * 60,
      select: (settings): AcademicCoreSettings => {
        return AcademicCoreSettingsSchema.parse(settings);
      },
    },
  });
