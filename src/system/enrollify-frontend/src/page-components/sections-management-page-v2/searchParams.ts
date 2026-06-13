import { parseAsInteger, parseAsString } from "nuqs";
import { getFiltersStateParser, getSortingStateParser } from "@/lib/parsers";
import type { ClassSectionV2 } from "@/api/models/class-section-v2";

export const searchParams = {
  collegeId: parseAsString,
  page: parseAsInteger.withDefault(1),
  perPage: parseAsInteger.withDefault(10),
  view: parseAsString.withDefault("card"),
  filters: getFiltersStateParser<ClassSectionV2>().withDefault([]),
  sort: getSortingStateParser<ClassSectionV2>().withDefault([]),
  joinOperator: parseAsString.withDefault("and"),
};
