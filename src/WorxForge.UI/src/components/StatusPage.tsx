import { Link } from "react-router-dom";
import { APP_NAME } from "../constants/app.constants";
import { Logo } from "../layouts/components/Logo";

interface StatusPageAction {
  label: string;
  to?: string;
  onClick?: () => void;
}

interface StatusPageProps {
  statusCode: string;
  title: string;
  description: string;
  action: StatusPageAction;
  secondaryAction?: StatusPageAction;
  fullscreen?: boolean;
}

const primaryActionClassName =
  "inline-flex items-center justify-center rounded-lg bg-primary px-5 py-2.5 text-sm font-medium text-primary-foreground shadow transition-colors hover:bg-primary/90 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary";

const secondaryActionClassName =
  "inline-flex items-center justify-center rounded-lg border border-secondary/40 px-5 py-2.5 text-sm font-medium text-foreground transition-colors hover:bg-primary/10 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary";

function StatusAction({ action, className }: { action: StatusPageAction; className: string }) {
  if (action.to) {
    return (
      <Link to={action.to} className={className}>
        {action.label}
      </Link>
    );
  }

  return (
    <button type="button" onClick={action.onClick} className={className}>
      {action.label}
    </button>
  );
}

export function StatusPage({
  statusCode,
  title,
  description,
  action,
  secondaryAction,
  fullscreen = false
}: StatusPageProps) {
  const card = (
    <section className="relative flex w-full max-w-md flex-col items-center gap-4 rounded-2xl border border-border/80 bg-card/95 p-8 text-center shadow-xl backdrop-blur-sm">
      <Logo className="size-10" />
      <p aria-hidden="true" className="bg-gradient-to-br from-primary to-secondary bg-clip-text text-6xl font-bold leading-none text-transparent">
        {statusCode}
      </p>
      <h1 className="text-xl font-semibold text-foreground">{title}</h1>
      <p className="text-sm text-muted-foreground">{description}</p>
      <div className="mt-1 flex flex-col items-center gap-2 sm:flex-row">
        <StatusAction action={action} className={primaryActionClassName} />
        {secondaryAction && <StatusAction action={secondaryAction} className={secondaryActionClassName} />}
      </div>
      <p className="mt-2 text-xs text-muted-foreground">{APP_NAME}</p>
    </section>
  );

  if (fullscreen) {
    return (
      <main className="wf-page-enter relative flex min-h-screen items-center justify-center overflow-hidden bg-background px-4 py-10 sm:px-6">
        <div className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_14%_18%,color-mix(in_oklab,var(--color-primary)_20%,transparent)_0%,transparent_38%),radial-gradient(circle_at_86%_82%,color-mix(in_oklab,var(--color-secondary)_30%,transparent)_0%,transparent_44%)]" />
        {card}
      </main>
    );
  }

  return (
    <div className="wf-page-enter flex w-full flex-col items-center px-4 py-10 sm:px-6">
      {card}
    </div>
  );
}
