import type { RefObject } from "react";
import { useId } from "react";
import { Shapes, X } from "lucide-react";
import { Link, useLocation } from "react-router-dom";
import { useNotificationContext } from "../contexts/NotificationContext";
import { cn } from "../lib/utils";
import { navigationItems, type NavigationItem } from "../navigation/navigation.config";

type AppSidebarProps = {
  collapsed?: boolean;
  isMobile?: boolean;
  animateSequence?: boolean;
  mobileCloseButtonRef?: RefObject<HTMLButtonElement | null>;
  onCloseMobile?: () => void;
  onNavigate?: () => void;
};

const primaryNavigationItems = navigationItems.filter((item) => item.group === "primary");
const accountNavigationItems = navigationItems.filter((item) => item.group === "account");

function isNavigationItemActive(pathname: string, item: NavigationItem): boolean {
  if (item.excludePaths?.includes(pathname)) {
    return false;
  }

  if (item.match === "prefix") {
    return pathname === item.path || pathname.startsWith(`${item.path}/`);
  }

  return pathname === item.path;
}

function formatNotificationCount(unreadCount: number): string {
  return unreadCount > 99 ? "99+" : unreadCount.toString();
}

function SidebarLink({
  collapsed = false,
  enterDelayMs,
  shouldAnimate,
  item,
  onNavigate,
  unreadCount = 0
}: {
  collapsed?: boolean;
  enterDelayMs?: number;
  shouldAnimate?: boolean;
  item: NavigationItem;
  onNavigate?: () => void;
  unreadCount?: number;
}) {
  const location = useLocation();
  const isActive = isNavigationItemActive(location.pathname, item);
  const showBadge = item.showNotificationBadge && unreadCount > 0;
  const Icon = item.icon;

  return (
    <Link
      to={item.path}
      aria-current={isActive ? "page" : undefined}
      aria-label={collapsed ? item.label : undefined}
      className={cn(
        "group relative flex items-center overflow-hidden rounded-lg text-sm transition-[background-color,color,transform] motion-safe:duration-180 motion-safe:ease-out motion-reduce:transition-none focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-sidebar-ring focus-visible:ring-offset-2 focus-visible:ring-offset-sidebar active:scale-[0.98] motion-reduce:active:scale-100",
        collapsed ? "justify-center gap-0 px-2.5 py-2.5" : "gap-3 px-3 py-2.5",
        !collapsed && "motion-safe:hover:translate-x-0.5",
        shouldAnimate && "wf-sidebar-item-enter motion-safe:opacity-0",
        isActive && "wf-rainbow-active",
        item.emphasized
          ? "bg-sidebar-primary text-sidebar-primary-foreground shadow-sm hover:bg-sidebar-primary/90 active:bg-sidebar-primary/80"
          : isActive
            ? "bg-sidebar-accent text-sidebar-accent-foreground shadow-sm"
            : "text-sidebar-foreground/80 hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
      )}
      style={shouldAnimate ? { animationDelay: `${enterDelayMs ?? 0}ms` } : undefined}
      onClick={onNavigate}
      title={collapsed ? item.label : undefined}
    >
      <Icon
        aria-hidden="true"
        className={cn(
          "size-[18px] shrink-0 transition-colors motion-safe:duration-200 motion-safe:ease-in-out motion-reduce:transition-none",
          item.emphasized || isActive ? "text-current" : "text-sidebar-foreground/70 group-hover:text-current"
        )}
      />
      <div
        className={cn(
          "flex min-w-0 items-center justify-between gap-2 overflow-hidden transition-[opacity,max-width,transform] motion-safe:duration-220 motion-safe:ease-out motion-reduce:transition-none",
          collapsed
            ? "max-w-0 flex-none justify-center opacity-0 -translate-y-1 scale-[0.985]"
            : "max-w-[12rem] flex-1 opacity-100 translate-y-0 scale-100"
        )}
      >
        {!collapsed ? (
          <span className={cn("min-w-0 flex-1 truncate", isActive || item.emphasized ? "font-semibold" : "font-medium")}>
            {item.label}
          </span>
        ) : null}

        {showBadge && !collapsed ? (
          <span className="inline-flex min-w-6 items-center justify-center rounded-full bg-sidebar text-[11px] font-semibold text-sidebar-primary px-1.5 py-0.5">
            {formatNotificationCount(unreadCount)}
          </span>
        ) : null}
      </div>

      {collapsed && showBadge ? (
        <span className="absolute right-2 top-2 size-2 rounded-full bg-sidebar-primary" aria-hidden="true" />
      ) : null}
    </Link>
  );
}

export function AppSidebar({
  collapsed = false,
  isMobile = false,
  animateSequence = false,
  mobileCloseButtonRef,
  onCloseMobile,
  onNavigate
}: AppSidebarProps) {
  const { unreadCount } = useNotificationContext();
  const primaryLabelId = useId();
  const accountLabelId = useId();
  const isCollapsed = !isMobile && collapsed;
  const shouldAnimateSequence = animateSequence && !isCollapsed;
  const primaryBaseDelay = 320;
  const accountHeadingDelay = primaryBaseDelay + (primaryNavigationItems.length * 130) + 160;
  const accountBaseDelay = accountHeadingDelay + 130;

  return (
    <aside
      className={cn(
        "flex h-full shrink-0 flex-col border-r border-sidebar-border bg-sidebar text-sidebar-foreground",
        isMobile
          ? "w-64 shadow-lg"
          : "motion-safe:transition-[width] motion-safe:duration-300 motion-safe:ease-in-out motion-reduce:transition-none",
        isCollapsed ? "w-20" : "w-64"
      )}
    >
      <div className="flex items-center justify-between gap-3 border-b border-sidebar-border px-4 py-4">
        <div
          className={cn("flex min-w-0 items-center gap-3", isCollapsed && "justify-center", shouldAnimateSequence && "wf-sidebar-item-enter motion-safe:opacity-0")}
          style={shouldAnimateSequence ? { animationDelay: "40ms" } : undefined}
        >
          <div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-sidebar-primary text-sidebar-primary-foreground shadow-sm transition-transform motion-safe:duration-250 motion-safe:ease-out motion-reduce:transition-none">
            <Shapes aria-hidden="true" className="size-5" />
          </div>
          <div
            className={cn(
              "min-w-0 overflow-hidden transition-[opacity,max-width,transform] motion-safe:duration-220 motion-safe:ease-out motion-reduce:transition-none",
              isCollapsed
                ? "max-w-0 opacity-0 -translate-y-1 scale-[0.985]"
                : "max-w-[9rem] opacity-100 translate-y-0 scale-100"
            )}
          >
            {!isCollapsed ? (
              <>
              <p className="truncate text-sm font-semibold tracking-tight">WorxForge</p>
              <p className="truncate text-xs text-sidebar-foreground/65">Creator workspace</p>
              </>
            ) : null}
          </div>
        </div>
        {isMobile ? (
          <button
            ref={mobileCloseButtonRef}
            type="button"
            className="inline-flex size-10 items-center justify-center rounded-lg text-sidebar-foreground/70 transition-colors hover:bg-sidebar-accent hover:text-sidebar-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-sidebar-ring focus-visible:ring-offset-2 focus-visible:ring-offset-sidebar"
            onClick={onCloseMobile}
            aria-label="Close navigation menu"
          >
            <X aria-hidden="true" className="size-5" />
          </button>
        ) : null}
      </div>

      <div className="flex flex-1 flex-col overflow-hidden">
        <div className="flex-1 overflow-y-auto px-3 py-4">
          <section aria-labelledby={primaryLabelId} className="space-y-3">
            <div
              className={cn(
                "overflow-hidden px-1 transition-[opacity,max-height,transform] motion-safe:duration-220 motion-safe:ease-out motion-reduce:transition-none",
                isCollapsed
                  ? "max-h-0 opacity-0 -translate-y-1"
                  : "max-h-8 opacity-100 translate-y-0",
                shouldAnimateSequence && "wf-sidebar-item-enter motion-safe:opacity-0"
              )}
              style={shouldAnimateSequence ? { animationDelay: "90ms" } : undefined}
            >
              <p id={primaryLabelId} className="text-xs font-semibold uppercase tracking-[0.16em] text-sidebar-foreground/55">
                Navigation
              </p>
            </div>
            <nav aria-labelledby={primaryLabelId} className="space-y-1.5">
              {primaryNavigationItems.map((item, index) => (
                <SidebarLink
                  key={item.path}
                  collapsed={isCollapsed}
                  enterDelayMs={primaryBaseDelay + (index * 130)}
                  item={item}
                  onNavigate={onNavigate}
                  shouldAnimate={shouldAnimateSequence}
                  unreadCount={unreadCount}
                />
              ))}
            </nav>
          </section>
        </div>

        <div className="mt-auto border-t border-sidebar-border px-3 py-4">
          <section aria-labelledby={accountLabelId} className="space-y-3">
            <div
              className={cn(
                "overflow-hidden px-1 transition-[opacity,max-height,transform] motion-safe:duration-220 motion-safe:ease-out motion-reduce:transition-none",
                isCollapsed
                  ? "max-h-0 opacity-0 -translate-y-1"
                  : "max-h-8 opacity-100 translate-y-0",
                shouldAnimateSequence && "wf-sidebar-item-enter motion-safe:opacity-0"
              )}
              style={shouldAnimateSequence ? { animationDelay: `${accountHeadingDelay}ms` } : undefined}
            >
              <p id={accountLabelId} className="text-xs font-semibold uppercase tracking-[0.16em] text-sidebar-foreground/55">
                Account
              </p>
            </div>
            <nav aria-labelledby={accountLabelId} className="space-y-1.5">
              {accountNavigationItems.map((item, index) => (
                <SidebarLink
                  key={item.path}
                  collapsed={isCollapsed}
                  enterDelayMs={accountBaseDelay + (index * 130)}
                  item={item}
                  onNavigate={onNavigate}
                  shouldAnimate={shouldAnimateSequence}
                />
              ))}
            </nav>
          </section>
        </div>
      </div>
    </aside>
  );
}