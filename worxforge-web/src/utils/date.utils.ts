import { format, formatDistanceToNow, isValid, parseISO } from "date-fns";

export function formatDate(isoDate: string): string {
  const date = parseISO(isoDate);
  if (!isValid(date)) {
    return "";
  }

  return format(date, "MMM d, yyyy");
}

export function formatRelativeTime(isoDate: string): string {
  const date = parseISO(isoDate);
  if (!isValid(date)) {
    return "";
  }

  return formatDistanceToNow(date, { addSuffix: true });
}


