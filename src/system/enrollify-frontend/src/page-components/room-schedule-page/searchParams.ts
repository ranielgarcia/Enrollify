import { parseAsInteger, parseAsStringEnum } from "nuqs";

export const DAY_KEYS = ["MON", "TUE", "WED", "THU", "FRI", "SAT"] as const;
export type DayKey = (typeof DAY_KEYS)[number];

export const DAY_OPTIONS: { key: DayKey; label: string; short: string }[] = [
  { key: "MON", label: "Monday", short: "Mon" },
  { key: "TUE", label: "Tuesday", short: "Tue" },
  { key: "WED", label: "Wednesday", short: "Wed" },
  { key: "THU", label: "Thursday", short: "Thu" },
  { key: "FRI", label: "Friday", short: "Fri" },
  { key: "SAT", label: "Saturday", short: "Sat" },
];

export const searchParams = {
  dayOfWeek: parseAsStringEnum([...DAY_KEYS]).withDefault("MON"),
  buildingId: parseAsInteger,
  roomTypeId: parseAsInteger,
  collegeId: parseAsInteger,
  courseId: parseAsInteger,
};
