import type { LucideIcon } from "lucide-react";
import {
  SchoolIcon,
  Castle,
  GraduationCap,
  BuildingIcon,
  DoorOpen,
  BookOpen,
  Scroll,
  BookUser,
  Bookmark,
  CalendarDays,
} from "lucide-react";

export const ModuleIcons = {
  colleges: SchoolIcon,
  departments: Castle,
  courses: GraduationCap,
  buildings: BuildingIcon,
  rooms: DoorOpen,
  roomTypes: Bookmark,
  subjects: BookOpen,
  curriculum: Scroll,
  teachers: BookUser,
  academicYear: CalendarDays,
} as const satisfies Record<string, LucideIcon>;
