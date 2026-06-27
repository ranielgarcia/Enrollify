import { parseAsInteger, parseAsString } from "nuqs";
import { getFiltersStateParser, getSortingStateParser } from "@/lib/parsers";
import type { ClassSection } from "@/api/models/class-section";

export const searchParams = {
  collegeId: parseAsString,
  page: parseAsInteger.withDefault(1),
  perPage: parseAsInteger.withDefault(10),
  view: parseAsString.withDefault("card"),
  filters: getFiltersStateParser<ClassSection>().withDefault([]),
  sort: getSortingStateParser<ClassSection>().withDefault([]),
  joinOperator: parseAsString.withDefault("and"),
};
