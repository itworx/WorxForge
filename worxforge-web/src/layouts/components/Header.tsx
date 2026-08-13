import { Link } from "react-router-dom";
import { FaRegBell, FaUserCircle } from "react-icons/fa";
import { useAuthContext } from "../../contexts/AuthContext";
import { useNotificationContext } from "../../contexts/NotificationContext";
import { ROUTES } from "../../constants/routes.constants";
import { APP_NAME } from "../../constants/app.constants";
import { cn } from "../../lib/utils";
import { Logo } from "./Logo";

type HeaderBrandMode = "full" | "nameOnly" | "hidden";

type HeaderProps = {
  brandMode?: HeaderBrandMode;
  brandOffsetClassName?: string;
  transferToSidebar?: boolean;
  transferFromSidebar?: boolean;
  synchronizeLogoReveal?: boolean;
  popLogo?: boolean;
};

function getInitials(displayName: string): string {
  return displayName
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]!.toUpperCase())
    .join("");
}

export function Header({
  brandMode = "full",
  brandOffsetClassName,
  transferToSidebar = false,
  transferFromSidebar = false,
  synchronizeLogoReveal = false,
  popLogo = false
}: HeaderProps) {
  const { user } = useAuthContext();
  const { unreadCount } = useNotificationContext();
  const showLogo = brandMode === "full";
  const showBrand = brandMode !== "hidden";

  return (
    <header className="sticky top-0 z-40 flex h-16 items-center justify-between gap-4 border-b border-border bg-background px-4 sm:px-6">
      <Link
        to={ROUTES.HOME}
        className={cn(
          "origin-left flex items-center gap-0 overflow-hidden text-text transition-[margin-left,max-width,opacity,transform] motion-safe:duration-700 motion-safe:ease-in-out motion-reduce:transition-none",
          showBrand ? "max-w-[16rem] opacity-100 translate-x-0" : "pointer-events-none max-w-0 opacity-0 -translate-x-10",
          transferToSidebar && "wf-brand-transfer-out",
          transferFromSidebar && "wf-brand-transfer-in-left",
          brandOffsetClassName
        )}
        aria-hidden={!showBrand}
        tabIndex={showBrand ? undefined : -1}
      >
        <span
          className={cn(
            "inline-flex items-center overflow-hidden transition-[max-width,opacity,margin-right,transform] motion-safe:duration-650 motion-safe:ease-in-out motion-reduce:transition-none",
            showLogo ? "max-w-8 opacity-100 mr-2" : "pointer-events-none max-w-0 opacity-0 mr-0",
            popLogo && "wf-brand-logo-pop-in"
          )}
          style={{
            transitionDelay: showLogo && !synchronizeLogoReveal ? "220ms" : "0ms"
          }}
          aria-hidden={!showLogo}
        >
          <Logo className="size-8" />
        </span>
        <span
          className={cn(
            "text-lg font-semibold text-text transition-transform motion-safe:duration-700 motion-safe:ease-in-out motion-reduce:transition-none",
            showLogo ? "translate-x-0" : "-translate-x-0.5"
          )}
        >
          {APP_NAME}
        </span>
      </Link>

      <div className="flex items-center gap-3">
        <Link
          to={ROUTES.NOTIFICATIONS}
          aria-label="Notifications"
          className="relative rounded-full p-2 text-text/70 transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background"
        >
          <FaRegBell aria-hidden="true" className="size-5" />
          {unreadCount > 0 && (
            <span className="absolute -top-0.5 -right-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-primary px-1 text-[10px] font-semibold text-primary-foreground">
              {unreadCount > 99 ? "99+" : unreadCount}
            </span>
          )}
        </Link>

        <Link to={ROUTES.PROFILE_ME} className="flex items-center gap-2">
          {user?.avatarUrl ? (
            <img src={user.avatarUrl} alt={user.displayName} className="size-9 rounded-full object-cover" />
          ) : user ? (
            <span className="flex size-9 items-center justify-center rounded-full bg-muted text-sm font-semibold text-text">
              {getInitials(user.displayName)}
            </span>
          ) : (
            <FaUserCircle aria-hidden="true" className="size-9 text-muted-foreground" />
          )}
          {user?.role === "Admin" && (
            <span className="inline-flex items-center rounded-full bg-accent px-2 py-0.5 text-[11px] leading-4 font-medium text-accent-foreground">
              Admin
            </span>
          )}
        </Link>
      </div>
    </header>
  );
}
