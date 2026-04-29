import { parseAsInteger, parseAsString } from "nuqs";
import { getFiltersStateParser, getSortingStateParser } from "@/lib/parsers";
import type { Teacher } from "@/api/models/teacher";

export const searchParams = {
  page: parseAsInteger.withDefault(1),
  perPage: parseAsInteger.withDefault(10),
  filters: getFiltersStateParser<Teacher>().withDefault([]),
  sort: getSortingStateParser<Teacher>().withDefault([]),
  joinOperator: parseAsString.withDefault("and"),
};
