import type { Table } from "@tanstack/react-table";
import { ArrowUp, CheckCircle2, Download, Trash2, X } from "lucide-react";
import * as React from "react";
// import { toast } from "sonner";
import {
  ActionBar,
  ActionBarClose,
  ActionBarGroup,
  ActionBarItem,
  ActionBarSelection,
  ActionBarSeparator,
} from "@/components/ui/action-bar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import type { Subject } from "@/api/models/subject";

interface SubjecsTableActionBarProps {
  table: Table<Subject>;
}

export function SubjectsTableActionBar({ table }: SubjecsTableActionBarProps) {
  const rows = table.getFilteredSelectedRowModel().rows;

  const onOpenChange = React.useCallback(
    (open: boolean) => {
      if (!open) {
        table.toggleAllRowsSelected(false);
      }
    },
    [table],
  );

  //   const onTaskUpdate = React.useCallback(
  //     (
  //       field: "status" | "priority",
  //       value: Task["status"] | Task["priority"],
  //     ) => {
  //       async function update() {
  //         const { error } = await updateTasks({
  //           ids: rows.map((row) => row.original.id),
  //           [field]: value,
  //         });

  //         if (error) {
  //           toast.error(error);
  //           return;
  //         }
  //         toast.success("Tasks updated");
  //       }
  //       update();
  //     },
  //     [rows],
  //   );

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const exportTableToCSV = (table: any, obj: any) => console.log(table, obj);

  const onTaskExport = React.useCallback(() => {
    exportTableToCSV(table, {
      excludeColumns: ["select", "actions"],
      onlySelected: true,
    });
  }, [table]);

  const onTaskDelete = React.useCallback(() => {
    async function remove() {
      console.log("ye");
    }
    remove();
  }, [rows, table]);

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
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <ActionBarItem>
              <CheckCircle2 />
              Status
            </ActionBarItem>
          </DropdownMenuTrigger>
          <DropdownMenuContent>
            <DropdownMenuItem
              className="capitalize"
              onClick={() => console.log("clicked")}
            >
              Test
            </DropdownMenuItem>
            {/* {tasks.status.enumValues.map((status) => (
              <DropdownMenuItem
                key={status}
                className="capitalize"
                onClick={() => onTaskUpdate("status", status)}
              >
                {status}
              </DropdownMenuItem>
            ))} */}
          </DropdownMenuContent>
        </DropdownMenu>
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <ActionBarItem>
              <ArrowUp />
              Priority
            </ActionBarItem>
          </DropdownMenuTrigger>
          <DropdownMenuContent>
            {/* {tasks.priority.enumValues.map((priority) => (
              <DropdownMenuItem
                key={priority}
                className="capitalize"
                onClick={() => onTaskUpdate("priority", priority)}
              >
                {priority}
              </DropdownMenuItem>
            ))} */}
            <DropdownMenuItem
              className="capitalize"
              onClick={() => console.log("clicked")}
            >
              Test
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
        <ActionBarItem onClick={onTaskExport}>
          <Download />
          Export
        </ActionBarItem>
        <ActionBarItem variant="destructive" onClick={onTaskDelete}>
          <Trash2 />
          Delete
        </ActionBarItem>
      </ActionBarGroup>
    </ActionBar>
  );
}
