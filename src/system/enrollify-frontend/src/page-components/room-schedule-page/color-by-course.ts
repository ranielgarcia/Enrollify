/**
 * Deterministic color assignment for offering blocks, keyed by courseId.
 * Conflicts override the course color with a destructive/red treatment.
 * Class strings are fully spelled out so Tailwind does not purge them.
 */

export interface BlockColor {
  block: string;
  accent: string;
  dot: string;
}

const PALETTE: BlockColor[] = [
  {
    block:
      "bg-sky-100 border-sky-300 text-sky-900 dark:bg-sky-950 dark:border-sky-800 dark:text-sky-100",
    accent: "bg-sky-500",
    dot: "bg-sky-500",
  },
  {
    block:
      "bg-emerald-100 border-emerald-300 text-emerald-900 dark:bg-emerald-950 dark:border-emerald-800 dark:text-emerald-100",
    accent: "bg-emerald-500",
    dot: "bg-emerald-500",
  },
  {
    block:
      "bg-violet-100 border-violet-300 text-violet-900 dark:bg-violet-950 dark:border-violet-800 dark:text-violet-100",
    accent: "bg-violet-500",
    dot: "bg-violet-500",
  },
  {
    block:
      "bg-amber-100 border-amber-300 text-amber-900 dark:bg-amber-950 dark:border-amber-800 dark:text-amber-100",
    accent: "bg-amber-500",
    dot: "bg-amber-500",
  },
  {
    block:
      "bg-rose-100 border-rose-300 text-rose-900 dark:bg-rose-950 dark:border-rose-800 dark:text-rose-100",
    accent: "bg-rose-500",
    dot: "bg-rose-500",
  },
  {
    block:
      "bg-cyan-100 border-cyan-300 text-cyan-900 dark:bg-cyan-950 dark:border-cyan-800 dark:text-cyan-100",
    accent: "bg-cyan-500",
    dot: "bg-cyan-500",
  },
  {
    block:
      "bg-indigo-100 border-indigo-300 text-indigo-900 dark:bg-indigo-950 dark:border-indigo-800 dark:text-indigo-100",
    accent: "bg-indigo-500",
    dot: "bg-indigo-500",
  },
  {
    block:
      "bg-teal-100 border-teal-300 text-teal-900 dark:bg-teal-950 dark:border-teal-800 dark:text-teal-100",
    accent: "bg-teal-500",
    dot: "bg-teal-500",
  },
  {
    block:
      "bg-fuchsia-100 border-fuchsia-300 text-fuchsia-900 dark:bg-fuchsia-950 dark:border-fuchsia-800 dark:text-fuchsia-100",
    accent: "bg-fuchsia-500",
    dot: "bg-fuchsia-500",
  },
  {
    block:
      "bg-lime-100 border-lime-300 text-lime-900 dark:bg-lime-950 dark:border-lime-800 dark:text-lime-100",
    accent: "bg-lime-500",
    dot: "bg-lime-500",
  },
  {
    block:
      "bg-orange-100 border-orange-300 text-orange-900 dark:bg-orange-950 dark:border-orange-800 dark:text-orange-100",
    accent: "bg-orange-500",
    dot: "bg-orange-500",
  },
  {
    block:
      "bg-blue-100 border-blue-300 text-blue-900 dark:bg-blue-950 dark:border-blue-800 dark:text-blue-100",
    accent: "bg-blue-500",
    dot: "bg-blue-500",
  },
];

export const CONFLICT_COLOR: BlockColor = {
  block:
    "bg-destructive/15 border-destructive/50 text-destructive dark:text-destructive-foreground",
  accent: "bg-destructive",
  dot: "bg-destructive",
};

export function getCourseColor(courseId: number): BlockColor {
  const index = Math.abs(courseId) % PALETTE.length;
  return PALETTE[index] ?? PALETTE[0]!;
}

export function getBlockColor(
  courseId: number,
  hasConflict: boolean,
): BlockColor {
  return hasConflict ? CONFLICT_COLOR : getCourseColor(courseId);
}
