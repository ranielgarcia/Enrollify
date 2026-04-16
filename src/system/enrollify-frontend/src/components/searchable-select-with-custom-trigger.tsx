import * as React from "react";
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
import { useElementWidth } from "@/hooks/use-element-width";

export type MultiSearchableSelectWithTriggerOption = {
  value: string;
  label: string;
};

interface MultiSearchableSelectWithTriggerProps {
  options: MultiSearchableSelectWithTriggerOption[];
  searchPlaceholder?: string;
  emptyMessage?: string;
  value?: string[];
  onValueChange?: (value: string[]) => void;
  disabled?: boolean;
  className?: string;
  name?: string;
  /** Custom trigger element - receives open state and selected count */
  trigger: React.ReactNode;
  /** Optional: Control popover alignment */
  align?: "start" | "center" | "end";
  /** Optional: Control popover width behavior */
  popoverWidth?: "trigger" | number | string;
}

export function SearchableSelectWithCustomTrigger({
  options,
  searchPlaceholder = "Search items...",
  emptyMessage = "No items found.",
  value,
  onValueChange,
  disabled = false,
  className,
  name,
  trigger,
  align = "start",
  popoverWidth = "trigger",
}: MultiSearchableSelectWithTriggerProps) {
  const [open, setOpen] = React.useState(false);
  const [selected, setSelected] = React.useState<string[]>(value || []);
  const triggerRef = React.useRef<HTMLDivElement>(null);
  const triggerWidth = useElementWidth(triggerRef);

  React.useEffect(() => {
    if (value !== undefined) {
      setSelected(value);
    }
  }, [value]);

  const handleSelect = (optionValue: string) => {
    const newSelected = selected.includes(optionValue)
      ? selected.filter((item) => item !== optionValue)
      : [...selected, optionValue];

    setSelected(newSelected);
    onValueChange?.(newSelected);
  };

  const getPopoverWidth = () => {
    if (popoverWidth === "trigger") {
      return triggerWidth > 0 ? `${triggerWidth}px` : "auto";
    }
    if (typeof popoverWidth === "number") {
      return `${popoverWidth}px`;
    }
    return popoverWidth;
  };

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild disabled={disabled}>
        <div
          ref={triggerRef}
          className={cn("w-full", className)}
          role="combobox"
          aria-expanded={open}
          aria-haspopup="listbox"
        >
          {trigger}
          {/* Hidden inputs for form submission */}
          {name &&
            selected.map((val) => (
              <input key={val} type="hidden" name={name} value={val} />
            ))}
        </div>
      </PopoverTrigger>
      <PopoverContent
        className="p-0"
        style={{ width: getPopoverWidth(), minWidth: "200px" }}
        align={align}
      >
        <Command className="w-full">
          <CommandInput placeholder={searchPlaceholder} aria-label="Search options" />
          <CommandList style={{ maxHeight: "200px" }} role="listbox">
            <CommandEmpty>{emptyMessage}</CommandEmpty>
            <CommandGroup>
              {options.map((option) => (
                <CommandItem
                  key={option.value}
                  value={option.label}
                  onSelect={() => handleSelect(option.value)}
                  role="option"
                  aria-selected={selected.includes(option.value)}
                >
                  <div className="flex items-center">{option.label}</div>
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}
