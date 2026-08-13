import { ExploreFilterSelect } from "./ExploreFilterSelect";
import type { ExploreFilters } from "../models/explore.models";

export interface ExploreFilterValues {
  assetTypeId: string;
  domainId: string;
  useCaseId: string;
  toolId: string;
  freshness: string;
  sortBy: string;
}

interface ExploreFiltersBarProps {
  filters?: ExploreFilters;
  values: ExploreFilterValues;
  onChange: (field: keyof ExploreFilterValues, value: string) => void;
  isLoading?: boolean;
}

const FRESHNESS_OPTIONS = [
  { value: "", label: "Any freshness" },
  { value: "0", label: "Week" },
  { value: "1", label: "Month" },
  { value: "2", label: "Quarter" }
];

const SORT_OPTIONS = [
  { value: "relevance", label: "Relevant" },
  { value: "helpful", label: "Helpful" },
  { value: "added", label: "Added" },
  { value: "updated", label: "Updated" },
  { value: "discussed", label: "Discussed" }
];

export function ExploreFiltersBar({ filters, values, onChange, isLoading = false }: ExploreFiltersBarProps) {
  return (
    <div className="grid grid-cols-1 gap-3 rounded-xl border border-secondary/30 bg-white p-4 shadow-sm md:grid-cols-3 lg:grid-cols-6">
      <ExploreFilterSelect
        label="Type"
        value={values.assetTypeId}
        disabled={isLoading}
        onValueChange={(value) => onChange("assetTypeId", value)}
        options={[{ value: "", label: "Any type" }, ...(filters?.assetTypes ?? []).map((item) => ({ value: item.id, label: item.name }))]}
      />
      <ExploreFilterSelect
        label="Domain"
        value={values.domainId}
        disabled={isLoading}
        onValueChange={(value) => onChange("domainId", value)}
        options={[{ value: "", label: "Any domain" }, ...(filters?.domains ?? []).map((item) => ({ value: item.id, label: item.name }))]}
      />
      <ExploreFilterSelect
        label="Use case"
        value={values.useCaseId}
        disabled={isLoading}
        onValueChange={(value) => onChange("useCaseId", value)}
        options={[{ value: "", label: "Any use case" }, ...(filters?.useCases ?? []).map((item) => ({ value: item.id, label: item.name }))]}
      />
      <ExploreFilterSelect
        label="Tool"
        value={values.toolId}
        disabled={isLoading}
        onValueChange={(value) => onChange("toolId", value)}
        options={[{ value: "", label: "Any tool" }, ...(filters?.tools ?? []).map((item) => ({ value: item.id, label: item.name }))]}
      />
      <ExploreFilterSelect
        label="Freshness"
        value={values.freshness}
        onValueChange={(value) => onChange("freshness", value)}
        options={FRESHNESS_OPTIONS}
      />
      <ExploreFilterSelect
        label="Sort"
        value={values.sortBy}
        onValueChange={(value) => onChange("sortBy", value)}
        options={SORT_OPTIONS}
      />
    </div>
  );
}
