import { formatDateTime, parseDateTime } from "@/lib/date-utils";
import z from "zod";

export const dateTransformer = z
  .string()
  .nullish()
  .transform((val) => formatDateTime(parseDateTime(val)));
