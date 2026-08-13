type AssetBadgeVariant = "primary" | "secondary";

interface AssetBadgeProps {
  label: string;
  variant?: AssetBadgeVariant;
}

const VARIANT_CLASSES: Record<AssetBadgeVariant, string> = {
  primary: "bg-primary/10 text-primary",
  secondary: "border border-secondary/30 bg-white text-text",
};

export function AssetBadge({ label, variant = "primary" }: AssetBadgeProps) {
  return (
    <span
      className={`inline-flex items-center rounded-full px-2 py-0.5 text-[11px] leading-4 font-medium ${VARIANT_CLASSES[variant]}`}
    >
      {label}
    </span>
  );
}
