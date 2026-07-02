import { Siren, AlertTriangle, Info } from "lucide-react";

const SeverityConfig = {
  Error: {
    label: "Hard Conflicts",
    icon: Siren,
    textColor: "text-rose-500 mt-0.5 shrink-0",
    description: "These must be resolved before the schedule can be published.",
  },
  Warning: {
    label: "Warnings",
    icon: AlertTriangle,
    textColor: "text-amber-500 mt-0.5 shrink-0",
    description: "These are advisory — review before publishing.",
  },
  Info: {
    label: "Information",
    icon: Info,
    textColor: "text-blue-600 dark:text-blue-500",
    description: "Additional information about this offering.",
  },
} as const;

export default SeverityConfig;
