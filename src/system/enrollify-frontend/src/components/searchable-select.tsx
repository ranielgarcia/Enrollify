import * as React from "react";
import { Check, ChevronsUpDown } from "lucide-react";

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
import { Input } from "@/components/ui/input";
import { useElementWidth } from "@/hooks/use-element-width";

export type SearchableSelectOption = {
  value: string;
  label: string;
};

interface SearchableSelectProps {
  options: SearchableSelectOption[];
  placeholder?: string;
  searchPlaceholder?: string;
  emptyMessage?: string;
  value?: string;
  onValueChange?: (value: string) => void;
  disabled?: boolean;
  className?: string;
  name?: string;
}

export function SearchableSelect({
  options,
  placeholder = "Select an item",
  searchPlaceholder = "Search items...",
  emptyMessage = "No items found.",
  value,
  onValueChange,
  disabled = false,
  className,
  name,
}: SearchableSelectProps) {
  const [open, setOpen] = React.useState(false);
  const [selected, setSelected] = React.useState<string>(value || "");
  const inputRef = React.useRef<HTMLDivElement>(null);
  const inputWidth = useElementWidth(inputRef);

  React.useEffect(() => {
    if (value !== undefined) {
      setSelected(value);
    }
  }, [value]);

  const selectedOption = options.find((option) => option.value === selected);

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <div
          className={cn("relative", className)}
          ref={inputRef}
          role="combobox"
          aria-expanded={open}
          aria-haspopup="listbox"
        >
          <Input
            readOnly
            value={selectedOption ? selectedOption.label : ""}
            placeholder={placeholder}
            className="pr-8 cursor-pointer focus:ring-2 focus:ring-ring focus:ring-offset-2 w-full"
            onClick={(e) => {
              // Prevent form submission
              e.preventDefault();
              setOpen(true);
            }}
            disabled={disabled}
            name={name}
          />
          <ChevronsUpDown className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 opacity-50" />
        </div>
      </PopoverTrigger>
      <PopoverContent
        className="p-0"
        style={{ width: `${inputWidth}px` }}
        align="start"
      >
        <Command className="w-full">
          <CommandInput placeholder={searchPlaceholder} aria-label="Search options" />
          <CommandList style={{ maxHeight: "200px" }} role="listbox">
            <CommandEmpty>{emptyMessage}</CommandEmpty>
            <CommandGroup>
              {options.map((option) => (
                <CommandItem
                  key={option.value}
                  value={option.label} // Use label for searching
                  onSelect={() => {
                    setSelected(option.value);
                    onValueChange?.(option.value);
                    setOpen(false);
                  }}
                  role="option"
                  aria-selected={selected === option.value}
                >
                  <Check
                    className={cn(
                      "mr-2 h-4 w-4",
                      selected === option.value ? "opacity-100" : "opacity-0"
                    )}
                  />
                  {option.label}
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}
