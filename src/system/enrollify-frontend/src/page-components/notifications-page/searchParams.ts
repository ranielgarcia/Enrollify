import { parseAsInteger, parseAsString } from "nuqs";

export const searchParams = {
  page: parseAsInteger.withDefault(1),
  perPage: parseAsInteger.withDefault(10),
  category: parseAsString.withDefault("all"),
  severity: parseAsString.withDefault("all"),
  readState: parseAsString.withDefault("all"),
  search: parseAsString.withDefault(""),
};
