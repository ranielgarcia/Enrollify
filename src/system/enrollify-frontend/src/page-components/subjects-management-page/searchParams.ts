import { parseAsInteger } from "nuqs";

export const searchParams = {
  page: parseAsInteger.withDefault(1),
  perPage: parseAsInteger.withDefault(10),
};
