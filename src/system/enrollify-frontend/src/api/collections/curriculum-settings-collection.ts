import { createAppSuspenseQueryOptions } from "@/hooks/create-query-options";
import {
  CurriculumSettingsSchema,
  type CurriculumSettings,
} from "../models/curriculum-settings";

const queryKeys = {
  CurriculumSettings: () => ["Curriculum-settings"],
};
export const getCurriculumSettingsQueryOptions = () =>
  createAppSuspenseQueryOptions({
    path: "/api/system-settings/curriculum-settings",
    options: {
      meta: { persist: true },
      queryKey: queryKeys.CurriculumSettings(),
      staleTime: 1000 * 60 * 5,
      gcTime: 1000 * 60 * 60 * 24,
      select: (settings): CurriculumSettings => {
        return CurriculumSettingsSchema.parse(settings);
      },
    },
  });
