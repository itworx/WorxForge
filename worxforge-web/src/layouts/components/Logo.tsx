interface LogoProps {
  className?: string;
}

export function Logo({ className = "size-8" }: LogoProps) {
  return (
    <svg viewBox="0 0 32 32" className={className} role="img" aria-label="WorxForge logo">
      <rect width="32" height="32" rx="8" fill="var(--color-primary)" />
      <polyline
        points="7,10 11,22 16,13 21,22 25,10"
        fill="none"
        stroke="white"
        strokeWidth="2.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}
