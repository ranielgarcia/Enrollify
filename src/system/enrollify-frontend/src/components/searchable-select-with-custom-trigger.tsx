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
  const [triggerWidth, setTriggerWidth] = React.useState<number>(0);

  React.useEffect(() => {
    if (value !== undefined) {
      setSelected(value);
    }
  }, [value]);

  React.useEffect(() => {
    if (triggerRef.current && popoverWidth === "trigger") {
      setTriggerWidth(triggerRef.current.offsetWidth);

      const resizeObserver = new ResizeObserver((entries) => {
        for (const entry of entries) {
          setTriggerWidth(entry.contentRect.width);
        }
      });

      resizeObserver.observe(triggerRef.current);

      return () => {
        if (triggerRef.current) {
          resizeObserver.unobserve(triggerRef.current);
        }
      };
    }
  }, [popoverWidth]);

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
        <div ref={triggerRef} className={cn("w-full", className)}>
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
          <CommandInput placeholder={searchPlaceholder} />
          <CommandList style={{ maxHeight: "200px" }}>
            <CommandEmpty>{emptyMessage}</CommandEmpty>
            <CommandGroup>
              {options.map((option) => (
                <CommandItem
                  key={option.value}
                  value={option.label}
                  onSelect={() => handleSelect(option.value)}
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
