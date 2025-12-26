import { formatDateTime, parseDateTime } from "@/lib/dateutils";
import z from "zod";

export const dateTransformer = z
  .string()
  .nullish()
  .transform((val) => formatDateTime(parseDateTime(val)));
