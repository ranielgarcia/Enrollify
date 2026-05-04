interface DataTableColumnActionsHeaderProps {
  label: string;
}

export function DataTableColumnActionsHeader({
  label,
}: DataTableColumnActionsHeaderProps) {
  return (
    <div className="uppercase tracking-wider text-muted-foreground text-xs">
      {label}
    </div>
  );
}
