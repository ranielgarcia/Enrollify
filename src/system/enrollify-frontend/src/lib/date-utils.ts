import { format, parseISO } from "date-fns";

export function formatDateTime(datetime?: Date | null): string | null {
  if (!datetime) return null;

  try {
    return format(datetime, "dd MMM yyyy, hh:mm a");
  } catch {
    return null;
  }
}

export function formatDate(dateStr?: string | null): string {
  if (!dateStr) return "—";
  try {
    return format(parseISO(dateStr), "MMMM d, yyyy");
  } catch {
    return dateStr;
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
