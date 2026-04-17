import type { ColumnDef, Table as TableType } from "@tanstack/react-table";
import { flexRender } from "@tanstack/react-table";
import { ChevronDown } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

interface DataTableProps<TData> {
  table: TableType<TData>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  columns: ColumnDef<TData, any>[];
  showSelectedRows?: boolean;
  showPaginationButtons?: boolean;
  /** Custom handler for previous page navigation (for manual pagination) */
  onPreviousPage?: () => void;
  /** Custom handler for next page navigation (for manual pagination) */
  onNextPage?: () => void;
  /** Current page number (1-indexed, for display purposes) */
  currentPage?: number;
  /** Total number of pages */
  totalPages?: number;
}

export function DataTable<TData>({
  table,
  columns,
  showSelectedRows = false,
  showPaginationButtons = false,
  onPreviousPage,
  onNextPage,
  currentPage,
  totalPages,
}: DataTableProps<TData>) {
  return (
    <div className="w-full min-w-0 space-y-4">
      <div className="flex items-center justify-between">
        <div className="flex min-w-0 flex-1 items-center gap-2">
          <Input
            placeholder="Search..."
            value={(table.getState().globalFilter as string) ?? ""}
            onChange={(event) => table.setGlobalFilter(event.target.value)}
            className="h-10 w-full max-w-sm"
            aria-label="Search table data"
          />
        </div>
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button
              variant="outline"
              className="ml-auto bg-transparent"
              aria-haspopup="true"
            >
              Columns <ChevronDown className="ml-2 size-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            {table
              .getAllColumns()
              .filter((column) => column.getCanHide())
              .map((column) => (
                <DropdownMenuCheckboxItem
                  key={column.id}
                  className="capitalize"
                  checked={column.getIsVisible()}
                  onCheckedChange={(value) => column.toggleVisibility(!!value)}
                >
                  {column.id}
                </DropdownMenuCheckboxItem>
              ))}
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
      <div className="w-full min-w-0 overflow-x-auto rounded-md border border-border">
        <Table className="table-fixed">
          <TableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow
                key={headerGroup.id}
                className="border-b bg-slate-50 hover:bg-slate-50"
              >
                {headerGroup.headers.map((header) => (
                  <TableHead
                    key={header.id}
                    className="px-4 py-3 text-xs font-semibold uppercase tracking-wider text-slate-600"
                  >
                    {header.isPlaceholder
                      ? null
                      : flexRender(
                          header.column.columnDef.header,
                          header.getContext(),
                        )}
                  </TableHead>
                ))}
              </TableRow>
            ))}
          </TableHeader>
          <TableBody>
            {table.getRowModel().rows?.length ? (
              table.getRowModel().rows.map((row) => (
                <TableRow
                  key={row.id}
                  data-state={row.getIsSelected() && "selected"}
                  aria-selected={row.getIsSelected()}
                  className="border-b hover:bg-slate-50/50 transition-colors"
                >
                  {row.getVisibleCells().map((cell) => (
                    <TableCell key={cell.id} className="px-4 py-3">
                      {flexRender(
                        cell.column.columnDef.cell,
                        cell.getContext(),
                      )}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell
                  colSpan={columns.length}
                  className="h-24 text-center text-muted-foreground"
                >
                  No results found.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </div>
      <div className="flex items-center justify-end space-x-2 py-4">
        {showSelectedRows && (
          <div className="text-muted-foreground flex-1 text-sm" role="status">
            {table.getFilteredSelectedRowModel().rows.length} of{" "}
            {table.getFilteredRowModel().rows.length} row(s) selected.
          </div>
        )}
        {showPaginationButtons && (
          <div className="flex items-center space-x-2">
            {currentPage !== undefined && totalPages !== undefined && (
              <span className="text-sm text-muted-foreground">
                Page {currentPage} of {totalPages}
              </span>
            )}
            <Button
              variant="outline"
              size="sm"
              onClick={onPreviousPage ?? (() => table.previousPage())}
              disabled={
                onPreviousPage
                  ? currentPage !== undefined && currentPage <= 1
                  : !table.getCanPreviousPage()
              }
            >
              Previous
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={onNextPage ?? (() => table.nextPage())}
              disabled={
                onNextPage
                  ? currentPage !== undefined &&
                    totalPages !== undefined &&
                    currentPage >= totalPages
                  : !table.getCanNextPage()
              }
            >
              Next
            </Button>
          </div>
        )}
      </div>
    </div>
  );
}
