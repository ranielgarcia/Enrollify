import { format, parseISO } from "date-fns";

export function formatDateTime(datetime?: Date | null): string | null {
  if (!datetime) return null;

  try {
    return format(datetime, "dd MMM yyyy, hh:mm a");
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
