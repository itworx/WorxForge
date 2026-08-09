import { Outlet } from "react-router-dom";

export function AuthLayout() {
  return (
    <main className="wf-page-enter relative min-h-screen overflow-hidden bg-background px-4 py-10 sm:px-6">
      <div className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_14%_18%,color-mix(in_oklab,var(--color-primary)_20%,transparent)_0%,transparent_38%),radial-gradient(circle_at_86%_82%,color-mix(in_oklab,var(--color-secondary)_30%,transparent)_0%,transparent_44%)]" />
      <div className="relative mx-auto flex min-h-[calc(100vh-5rem)] w-full max-w-md items-center">
        <Outlet />
      </div>
    </main>
  );
}

