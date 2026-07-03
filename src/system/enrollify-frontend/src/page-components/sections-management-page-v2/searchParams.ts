import { parseAsInteger, parseAsString } from "nuqs";

export type QuickFilter = "all" | "draft" | "open" | "cancelled";

export const searchParams = {
  collegeId: parseAsInteger,
  view: parseAsString.withDefault("card"),
  quickFilter: parseAsString.withDefault("all"),
};
