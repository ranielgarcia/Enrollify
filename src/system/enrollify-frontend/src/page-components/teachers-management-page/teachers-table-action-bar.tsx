import type { Table } from "@tanstack/react-table";
import { Download, X } from "lucide-react";
import * as React from "react";
import {
  ActionBar,
  ActionBarClose,
  ActionBarGroup,
  ActionBarItem,
  ActionBarSelection,
  ActionBarSeparator,
} from "@/components/ui/action-bar";
import type { Teacher } from "@/api/models/teacher";

interface TeachersTableActionBarProps {
  table: Table<Teacher>;
}

export function TeachersTableActionBar({ table }: TeachersTableActionBarProps) {
  const rows = table.getFilteredSelectedRowModel().rows;

  const onOpenChange = React.useCallback(
    (open: boolean) => {
      if (!open) {
        table.toggleAllRowsSelected(false);
      }
    },
    [table],
  );

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const exportTableToCSV = (t: any, obj: any) => console.log(t, obj);

  const onExport = React.useCallback(() => {
    exportTableToCSV(table, {
      excludeColumns: ["select", "actions", "avatar"],
      onlySelected: true,
    });
  }, [table]);

  return (
    <ActionBar open={rows.length > 0} onOpenChange={onOpenChange}>
      <ActionBarSelection>
        <span className="font-medium">{rows.length}</span>
        <span>selected</span>
        <ActionBarSeparator />
        <ActionBarClose>
          <X />
        </ActionBarClose>
      </ActionBarSelection>
      <ActionBarSeparator />
      <ActionBarGroup>
        <ActionBarItem onClick={onExport}>
          <Download />
          Export
        </ActionBarItem>
      </ActionBarGroup>
    </ActionBar>
  );
}
