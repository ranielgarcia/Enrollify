import { parseAsInteger, parseAsString } from "nuqs";
import { getFiltersStateParser, getSortingStateParser } from "@/lib/parsers";
import type { Subject } from "@/api/models/subject";

export const searchParams = {
  page: parseAsInteger.withDefault(1),
  perPage: parseAsInteger.withDefault(10),
  filters: getFiltersStateParser<Subject>().withDefault([]),
  sort: getSortingStateParser<Subject>().withDefault([]),
  joinOperator: parseAsString.withDefault("and"),
};
