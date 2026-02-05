import * as React from "react";
import { Check, ChevronsUpDown, X } from "lucide-react";

import { cn } from "@/lib/utils";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

export type MultiSearchableSelectOption = {
  value: string;
  label: string;
};

interface MultiSearchableSelectProps {
  options: MultiSearchableSelectOption[];
  placeholder?: string;
  searchPlaceholder?: string;
  emptyMessage?: string;
  value?: string[];
  onValueChange?: (value: string[]) => void;
  disabled?: boolean;
  className?: string;
  name?: string;
  maxDisplay?: number;
}

export function MultiSearchableSelect({
  options,
  placeholder = "Select items",
  searchPlaceholder = "Search items...",
  emptyMessage = "No items found.",
  value,
  onValueChange,
  disabled = false,
  className,
  name,
  maxDisplay = 3,
}: MultiSearchableSelectProps) {
  const [open, setOpen] = React.useState(false);
  const [selected, setSelected] = React.useState<string[]>(value || []);
  const inputRef = React.useRef<HTMLDivElement>(null);
  const [inputWidth, setInputWidth] = React.useState<number>(0);

  React.useEffect(() => {
    if (value !== undefined) {
      setSelected(value);
    }
  }, [value]);

  React.useEffect(() => {
    if (inputRef.current) {
      // Set initial width
      setInputWidth(inputRef.current.offsetWidth);

      // Update width on resize
      const resizeObserver = new ResizeObserver((entries) => {
        for (const entry of entries) {
          setInputWidth(entry.contentRect.width);
        }
      });

      resizeObserver.observe(inputRef.current);

      return () => {
        if (inputRef.current) {
          resizeObserver.unobserve(inputRef.current);
        }
      };
    }
  }, []);

  const selectedOptions = options.filter((option) =>
    selected.includes(option.value)
  );

  const handleSelect = (value: string) => {
    const newSelected = selected.includes(value)
      ? selected.filter((item) => item !== value)
      : [...selected, value];

    setSelected(newSelected);
    onValueChange?.(newSelected);
  };

  const handleRemove = (value: string, e: React.MouseEvent) => {
    e.stopPropagation();
    const newSelected = selected.filter((item) => item !== value);
    setSelected(newSelected);
    onValueChange?.(newSelected);
  };

  const handleClearAll = (e: React.MouseEvent) => {
    e.stopPropagation();
    setSelected([]);
    onValueChange?.([]);
  };

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <div className={cn("relative", className)} ref={inputRef}>
          <div
            className={cn(
              "min-h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background",
              "flex flex-wrap gap-1 items-center cursor-pointer",
              "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
              disabled && "opacity-50 cursor-not-allowed"
            )}
            onClick={(e) => {
              e.preventDefault();
              if (!disabled) setOpen(true);
            }}
          >
            {selectedOptions.length === 0 && (
              <span className="text-muted-foreground">{placeholder}</span>
            )}

            {selectedOptions.length > 0 && (
              <>
                {selectedOptions.slice(0, maxDisplay).map((option) => (
                  <Badge
                    key={option.value}
                    variant="secondary"
                    className="flex items-center gap-1 pl-2"
                  >
                    {option.label}
                    <Button
                      variant="ghost"
                      size="sm"
                      className="h-4 w-4 p-0 hover:bg-muted"
                      onClick={(e) => handleRemove(option.value, e)}
                    >
                      <X className="h-3 w-3" />
                      <span className="sr-only">Remove {option.label}</span>
                    </Button>
                  </Badge>
                ))}

                {selectedOptions.length > maxDisplay && (
                  <Badge variant="secondary">
                    +{selectedOptions.length - maxDisplay} more
                  </Badge>
                )}

                {selectedOptions.length > 1 && (
                  <Button
                    variant="ghost"
                    size="sm"
                    className="ml-auto h-4 rounded-sm p-0 hover:bg-muted"
                    onClick={handleClearAll}
                  >
                    <X className="h-3 w-3" />
                    <span className="sr-only">Clear all</span>
                  </Button>
                )}
              </>
            )}

            <ChevronsUpDown className="ml-auto h-4 w-4 shrink-0 opacity-50" />
          </div>

          {/* Hidden input for form submission */}
          {name &&
            selected.map((value) => (
              <input key={value} type="hidden" name={name} value={value} />
            ))}
        </div>
      </PopoverTrigger>
      <PopoverContent
        className="p-0"
        style={{ width: `${inputWidth}px` }}
        align="start"
      >
        <Command className="w-full">
          <CommandInput placeholder={searchPlaceholder} />
          <CommandList style={{ maxHeight: "200px" }}>
            <CommandEmpty>{emptyMessage}</CommandEmpty>
            <CommandGroup>
              {options.map((option) => (
                <CommandItem
                  key={option.value}
                  value={option.label} // Use label for searching
                  onSelect={() => handleSelect(option.value)}
                >
                  <div className="flex items-center">
                    <div
                      className={cn(
                        "mr-2 flex h-4 w-4 items-center justify-center rounded-sm border border-primary",
                        selected.includes(option.value)
                          ? "bg-primary text-primary-foreground"
                          : "opacity-50"
                      )}
                    >
                      {selected.includes(option.value) && (
                        <Check className="h-3 w-3" />
                      )}
                    </div>
                    {option.label}
                  </div>
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}
