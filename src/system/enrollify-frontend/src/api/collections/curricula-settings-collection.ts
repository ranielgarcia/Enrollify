import { createAppSuspenseQueryOptions } from "@/hooks/create-query-options";
import {
  CurriculaSettingsSchema,
  type CurriculaSettings,
} from "../models/curricula-settings";

const queryKeys = {
  curriculaSettings: () => ["curricula-settings"],
};
export const getCurriculaSettingsQueryOptions = () =>
  createAppSuspenseQueryOptions({
    path: "/api/system-settings/curricula-settings",
    options: {
      meta: { persist: true },
      queryKey: queryKeys.curriculaSettings(),
      staleTime: 1000 * 60 * 5,
      gcTime: 1000 * 60 * 60 * 24,
      select: (settings): CurriculaSettings => {
        return CurriculaSettingsSchema.parse(settings);
      },
    },
  });
