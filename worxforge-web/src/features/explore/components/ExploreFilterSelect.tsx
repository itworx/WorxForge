import { useEffect, useRef, useState } from "react";
import { Check, ChevronDown } from "lucide-react";
import { cn } from "../../../lib/utils";

interface ExploreFilterSelectOption {
  value: string;
  label: string;
}

interface ExploreFilterSelectProps {
  label: string;
  value: string;
  options: ExploreFilterSelectOption[];
  onValueChange: (value: string) => void;
  disabled?: boolean;
}

/** Custom-styled listbox used across the Explore filters bar (native <select> can't be themed consistently). */
export function ExploreFilterSelect({ label, value, options, onValueChange, disabled = false }: ExploreFilterSelectProps) {
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const selectedOption = options.find((option) => option.value === value);

  useEffect(() => {
    if (!isOpen) return;

    function handlePointerDown(event: MouseEvent) {
      if (!containerRef.current?.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") setIsOpen(false);
    }

    document.addEventListener("mousedown", handlePointerDown);
    document.addEventListener("keydown", handleKeyDown);
    return () => {
      document.removeEventListener("mousedown", handlePointerDown);
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [isOpen]);

  function handleSelect(optionValue: string) {
    onValueChange(optionValue);
    setIsOpen(false);
  }

  return (
    <div ref={containerRef}>
      <label className="text-xs text-muted-foreground">{label}</label>
      <div className="relative mt-1">
        <button
          type="button"
          role="combobox"
          aria-expanded={isOpen}
          aria-label={label}
          disabled={disabled}
          onClick={() => setIsOpen((previous) => !previous)}
          className={cn(
            "flex h-9 w-full items-center justify-between gap-2 rounded-md border border-input bg-white px-3 text-sm shadow-sm ring-offset-background transition-colors",
            "hover:bg-accent/40 focus:ring-1 focus:ring-ring focus:outline-none disabled:cursor-not-allowed disabled:opacity-50"
          )}
        >
          <span className="truncate">{selectedOption?.label ?? label}</span>
          <ChevronDown
            aria-hidden="true"
            className={cn("h-4 w-4 shrink-0 opacity-50 transition-transform", isOpen && "rotate-180")}
          />
        </button>

        {isOpen && (
          <ul
            role="listbox"
            aria-label={label}
            className="absolute z-20 mt-1 max-h-60 w-full overflow-auto rounded-md border border-input bg-white p-1 shadow-md"
          >
            {options.map((option) => {
              const isSelected = option.value === value;
              return (
                <li key={option.value}>
                  <button
                    type="button"
                    role="option"
                    aria-selected={isSelected}
                    onClick={() => handleSelect(option.value)}
                    className={cn(
                      "flex w-full items-center justify-between gap-2 rounded-sm px-2 py-1.5 text-left text-sm transition-colors",
                      "hover:bg-accent hover:text-accent-foreground",
                      isSelected ? "bg-primary/10 font-medium text-primary" : "text-foreground"
                    )}
                  >
                    <span className="truncate">{option.label}</span>
                    {isSelected && <Check aria-hidden="true" className="h-4 w-4 shrink-0" />}
                  </button>
                </li>
              );
            })}
          </ul>
        )}
      </div>
    </div>
  );
}

