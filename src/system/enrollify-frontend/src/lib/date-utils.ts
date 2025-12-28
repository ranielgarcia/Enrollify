import { format, parseISO } from "date-fns";

export function formatDateTimeString(
  dateString?: string | null
): string | null {
  if (!dateString) return null;

  try {
    const date = parseISO(dateString);
    return format(date, "MM-dd-yyyy hh:mm:ss a");
  } catch {
    return null;
  }
}

export function formatDateTime(datetime?: Date | null): string | null {
  if (!datetime) return null;

  try {
    return format(datetime, "MM-dd-yyyy hh:mm:ss a");
  } catch {
    return null;
  }
}

export function parseDateTime(dateString?: string | null): Date | null {
  if (!dateString) return null;

  try {
    return parseISO(dateString);
  } catch {
    return null;
  }
}
