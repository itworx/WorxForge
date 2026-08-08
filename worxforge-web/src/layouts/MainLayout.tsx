import { useEffect, useRef, useState } from "react";
import { Menu, PanelLeftClose, PanelLeftOpen } from "lucide-react";
import { Outlet, useLocation } from "react-router-dom";
import { AppSidebar } from "../components/AppSidebar";
import { cn } from "../lib/utils";
import { Header } from "./components/Header";
import { Footer } from "./components/Footer";

type DesktopSidebarState = "closed" | "expanded" | "collapsed";

const getNextDesktopSidebarState = (state: DesktopSidebarState): DesktopSidebarState => {
  if (state === "closed") {
    return "expanded";
  }

  if (state === "expanded") {
    return "collapsed";
  }

  return "closed";
};

export function MainLayout() {
  const [isMobileSidebarOpen, setIsMobileSidebarOpen] = useState(false);
  const [desktopSidebarState, setDesktopSidebarState] = useState<DesktopSidebarState>("closed");
  const [animateSidebarSequence, setAnimateSidebarSequence] = useState(false);
  const location = useLocation();
  const contentRef = useRef<HTMLDivElement>(null);
  const mobileDrawerRef = useRef<HTMLDivElement>(null);
  const mobileCloseButtonRef = useRef<HTMLButtonElement>(null);
  const isDesktopSidebarClosed = desktopSidebarState === "closed";
  const isDesktopSidebarCollapsed = desktopSidebarState === "collapsed";
  const desktopHeaderBrandMode = isDesktopSidebarCollapsed ? "nameOnly" : "full";
  const desktopHeaderBrandOffsetClassName = isDesktopSidebarClosed
    ? "ml-14"
    : isDesktopSidebarCollapsed
      ? "ml-2.5"
      : "ml-4";

  const handleDesktopSidebarToggle = () => {
    setDesktopSidebarState((currentState) => {
      const nextState = getNextDesktopSidebarState(currentState);

      if (nextState === "expanded") {
        setAnimateSidebarSequence(true);
      }

      return nextState;
    });
  };

  useEffect(() => {
    if (!animateSidebarSequence) {
      return undefined;
    }

    const timeoutId = window.setTimeout(() => {
      setAnimateSidebarSequence(false);
    }, 2300);

    return () => {
      window.clearTimeout(timeoutId);
    };
  }, [animateSidebarSequence]);

  useEffect(() => {
    setIsMobileSidebarOpen(false);
  }, [location.pathname]);

  useEffect(() => {
    if (!isMobileSidebarOpen) {
      return undefined;
    }

    mobileCloseButtonRef.current?.focus();

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        setIsMobileSidebarOpen(false);
      }
    };

    window.addEventListener("keydown", handleKeyDown);

    return () => {
      window.removeEventListener("keydown", handleKeyDown);
    };
  }, [isMobileSidebarOpen]);

  useEffect(() => {
    const contentElement = contentRef.current;
    const mobileDrawerElement = mobileDrawerRef.current;
    const previousOverflow = document.body.style.overflow;

    if (!contentElement || !mobileDrawerElement) {
      return undefined;
    }

    if (isMobileSidebarOpen) {
      contentElement.setAttribute("inert", "");
      contentElement.setAttribute("aria-hidden", "true");
      mobileDrawerElement.removeAttribute("inert");
      mobileDrawerElement.removeAttribute("aria-hidden");
      document.body.style.overflow = "hidden";
    } else {
      contentElement.removeAttribute("inert");
      contentElement.removeAttribute("aria-hidden");
      mobileDrawerElement.setAttribute("inert", "");
      mobileDrawerElement.setAttribute("aria-hidden", "true");
      document.body.style.overflow = previousOverflow;
    }

    return () => {
      contentElement.removeAttribute("inert");
      contentElement.removeAttribute("aria-hidden");
      mobileDrawerElement.removeAttribute("inert");
      mobileDrawerElement.removeAttribute("aria-hidden");
      document.body.style.overflow = previousOverflow;
    };
  }, [isMobileSidebarOpen]);

  return (
    <div className="min-h-screen bg-background text-text">
      <button
        type="button"
        className={cn(
          "fixed top-4 z-50 hidden size-11 items-center justify-center rounded-xl border border-border bg-background/95 text-text shadow-sm backdrop-blur-sm transition-[left,background-color,color,transform] motion-safe:duration-300 motion-safe:ease-in-out motion-reduce:transition-none hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background active:scale-[0.98] motion-reduce:active:scale-100 lg:inline-flex",
          isDesktopSidebarClosed
            ? "left-4"
            : isDesktopSidebarCollapsed
              ? "left-[4.25rem]"
              : "left-[15.25rem]"
        )}
        aria-label={
          isDesktopSidebarClosed
            ? "Open sidebar"
            : desktopSidebarState === "expanded"
              ? "Collapse sidebar"
              : "Close sidebar"
        }
        onClick={handleDesktopSidebarToggle}
      >
        <span className="relative block size-5">
          <Menu
            aria-hidden="true"
            className={cn(
              "absolute inset-0 size-5 transition-[opacity,transform] motion-safe:duration-200 motion-safe:ease-out motion-reduce:transition-none",
              isDesktopSidebarClosed
                ? "opacity-100 scale-100 rotate-0"
                : "opacity-0 scale-90 -rotate-12"
            )}
          />
          <PanelLeftClose
            aria-hidden="true"
            className={cn(
              "absolute inset-0 size-5 transition-[opacity,transform] motion-safe:duration-200 motion-safe:ease-out motion-reduce:transition-none",
              desktopSidebarState === "expanded"
                ? "opacity-100 scale-100 rotate-0"
                : "opacity-0 scale-90 rotate-12"
            )}
          />
          <PanelLeftOpen
            aria-hidden="true"
            className={cn(
              "absolute inset-0 size-5 transition-[opacity,transform] motion-safe:duration-200 motion-safe:ease-out motion-reduce:transition-none",
              desktopSidebarState === "collapsed"
                ? "opacity-100 scale-100 rotate-0"
                : "opacity-0 scale-90 -rotate-12"
            )}
          />
        </span>
      </button>

      <div className="flex min-h-screen">
        <div
          className={cn(
            "relative hidden overflow-hidden lg:block motion-safe:transition-[width] motion-safe:duration-300 motion-safe:ease-in-out motion-reduce:transition-none",
            isDesktopSidebarClosed
              ? "w-0"
              : isDesktopSidebarCollapsed
                ? "w-20"
                : "w-64"
          )}
          aria-hidden={isDesktopSidebarClosed}
          inert={isDesktopSidebarClosed}
        >
          <div
            className={cn(
              "h-full motion-safe:transition-transform motion-safe:duration-300 motion-safe:ease-in-out motion-reduce:transition-none",
              isDesktopSidebarClosed ? "-translate-x-full" : "translate-x-0"
            )}
          >
            <AppSidebar
              collapsed={isDesktopSidebarCollapsed}
              animateSequence={animateSidebarSequence}
            />
          </div>
        </div>

        <div
          className={cn(
            "fixed inset-0 z-40 lg:hidden",
            isMobileSidebarOpen ? "pointer-events-auto" : "pointer-events-none"
          )}
        >
          <button
            type="button"
            className={cn(
              "absolute inset-0 bg-background/70 transition-opacity motion-safe:duration-200 motion-safe:ease-in-out motion-reduce:transition-none",
              isMobileSidebarOpen ? "opacity-100" : "opacity-0"
            )}
            onClick={() => setIsMobileSidebarOpen(false)}
            aria-label="Close navigation menu"
          />
          <div
            id="mobile-navigation-drawer"
            ref={mobileDrawerRef}
            role="dialog"
            aria-modal="true"
            aria-label="Primary navigation"
            className={cn(
              "relative h-full max-w-64 transform transition-transform motion-safe:duration-200 motion-safe:ease-in-out motion-reduce:transition-none",
              isMobileSidebarOpen ? "translate-x-0" : "-translate-x-full"
            )}
          >
            <AppSidebar
              isMobile
              mobileCloseButtonRef={mobileCloseButtonRef}
              onCloseMobile={() => setIsMobileSidebarOpen(false)}
              onNavigate={() => setIsMobileSidebarOpen(false)}
            />
          </div>
        </div>

        <div ref={contentRef} className="flex min-w-0 flex-1 flex-col">
          <div className="hidden lg:block">
            <Header
              brandMode={desktopHeaderBrandMode}
              brandOffsetClassName={desktopHeaderBrandOffsetClassName}
            />
          </div>

          <header className="sticky top-0 z-30 flex h-16 items-center gap-3 border-b border-border bg-background px-4 lg:hidden">
            <button
              type="button"
              className="inline-flex size-10 items-center justify-center rounded-lg border border-border bg-background text-foreground transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background"
              aria-label="Open navigation menu"
              aria-expanded={isMobileSidebarOpen}
              aria-controls="mobile-navigation-drawer"
              onClick={() => setIsMobileSidebarOpen(true)}
            >
              <Menu aria-hidden="true" className="size-5" />
            </button>
            <div className="min-w-0">
              <p className="truncate text-sm font-semibold tracking-tight">WorxForge</p>
              <p className="truncate text-xs text-muted-foreground">Creator workspace</p>
            </div>
          </header>

          <main className="flex-1 overflow-x-hidden">
            <div className="min-h-full p-4 sm:p-6 lg:p-8">
              <Outlet />
            </div>
          </main>

          <Footer />
        </div>
      </div>
    </div>
  );
}
