import { Siren, AlertTriangle, Info } from "lucide-react";

const SeverityConfig = {
  Error: {
    label: "Hard Conflicts",
    icon: Siren,
    textColor: "text-rose-500 mt-0.5 shrink-0",
    description: "These must be resolved before the schedule can be published.",
    iconClass: "text-rose-500",
    badgeVariant: "outline" as const,
    containerClass:
      "border-rose-200 bg-rose-50 dark:border-rose-900 dark:bg-rose-950/20",
  },
  Warning: {
    label: "Warnings",
    icon: AlertTriangle,
    textColor: "text-amber-500 mt-0.5 shrink-0",
    description: "These are advisory — review before publishing.",
    iconClass: "text-amber-600",
    badgeVariant: "secondary" as const,
    containerClass:
      "border-amber-200 bg-amber-50 dark:border-amber-900 dark:bg-amber-950/20",
  },
  Info: {
    label: "Information",
    icon: Info,
    textColor: "text-blue-600 dark:text-blue-500",
    description: "Additional information about this offering.",
    iconClass: "text-blue-500",
    badgeVariant: "outline" as const,
    containerClass: "border-blue-300/50 bg-blue-50 dark:bg-blue-950/20",
  },
} as const;

export default SeverityConfig;
