import { format, isValid, parseISO } from "date-fns";

export function formatDate(isoDate: string): string {
  const date = parseISO(isoDate);
  if (!isValid(date)) {
    return "";
  }

  return format(date, "MMM d, yyyy");
}

