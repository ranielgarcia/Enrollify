import { parseAsInteger, parseAsString } from "nuqs";
import { getFiltersStateParser, getSortingStateParser } from "@/lib/parsers";
import type { ClassSection } from "@/api/models/class-scheduling/class-section";

export type QuickFilter =
  | "all"
  | "draft"
  | "open"
  | "cancelled"
  | "needs-attention";

export const searchParams = {
  collegeId: parseAsInteger,
  page: parseAsInteger.withDefault(1),
  perPage: parseAsInteger.withDefault(10),
  view: parseAsString.withDefault("card"),
  quickFilter: parseAsString.withDefault("all"),
  filters: getFiltersStateParser<ClassSection>().withDefault([]),
  sort: getSortingStateParser<ClassSection>().withDefault([]),
  joinOperator: parseAsString.withDefault("and"),
};
