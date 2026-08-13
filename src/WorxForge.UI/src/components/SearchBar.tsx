import { useState, type FormEvent } from "react";
import { Search, ArrowRight } from "lucide-react";
import { cn } from "../lib/utils";

interface SearchBarProps {
  placeholder?: string;
  defaultValue?: string;
  onSearch: (query: string) => void;
  className?: string;
  autoFocus?: boolean;
}

/** Reusable pill-shaped search bar used across pages (Home, Explore, ...). */
export function SearchBar({ placeholder = "Search...", defaultValue = "", onSearch, className, autoFocus }: SearchBarProps) {
  const [query, setQuery] = useState(defaultValue);

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    onSearch(query.trim());
  }

  return (
    <form onSubmit={handleSubmit} role="search" className={cn("relative", className)}>
      <Search aria-hidden="true" className="pointer-events-none absolute top-1/2 left-4 h-5 w-5 -translate-y-1/2 text-muted-foreground" />
      <input
        type="search"
        value={query}
        onChange={(event) => setQuery(event.target.value)}
        placeholder={placeholder}
        aria-label={placeholder}
        autoFocus={autoFocus}
        className="h-14 w-full rounded-full border border-input bg-white pr-24 pl-12 text-sm text-foreground shadow-md transition-colors placeholder:truncate placeholder:text-muted-foreground focus-visible:ring-1 focus-visible:ring-ring focus-visible:outline-none sm:pr-32 sm:text-base"
      />
      <button
        type="submit"
        className="absolute top-1/2 right-2 inline-flex h-10 -translate-y-1/2 items-center justify-center gap-1 rounded-full bg-primary px-3 text-sm font-medium text-primary-foreground shadow transition-colors hover:bg-primary/90 focus-visible:ring-1 focus-visible:ring-ring focus-visible:outline-none sm:px-5"
      >
        <span className="hidden sm:inline">Search</span>
        <ArrowRight aria-hidden="true" className="h-4 w-4" />
      </button>
    </form>
  );
}
