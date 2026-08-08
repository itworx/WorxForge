import { Link } from "react-router-dom";
import { FaRegBell, FaUserCircle } from "react-icons/fa";
import { useAuthContext } from "../../contexts/AuthContext";
import { useNotificationContext } from "../../contexts/NotificationContext";
import { ROUTES } from "../../constants/routes.constants";
import { APP_NAME } from "../../constants/app.constants";
import { Logo } from "./Logo";

function getInitials(displayName: string): string {
  return displayName
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]!.toUpperCase())
    .join("");
}

export function Header() {
  const { user } = useAuthContext();
  const { unreadCount } = useNotificationContext();

  return (
    <header className="sticky top-0 z-40 flex h-16 items-center justify-between gap-4 border-b border-secondary/20 bg-white px-4 sm:px-6">
      <Link to={ROUTES.HOME} className="flex items-center gap-2">
        <Logo className="size-8" />
        <span className="text-lg font-semibold text-text">{APP_NAME}</span>
      </Link>

      <div className="flex items-center gap-3">
        <Link
          to={ROUTES.NOTIFICATIONS}
          aria-label="Notifications"
          className="relative rounded-full p-2 text-text/70 transition-colors hover:bg-primary/10 hover:text-primary focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
        >
          <FaRegBell aria-hidden="true" className="size-5" />
          {unreadCount > 0 && (
            <span className="absolute -top-0.5 -right-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-primary px-1 text-[10px] font-semibold text-white">
              {unreadCount > 99 ? "99+" : unreadCount}
            </span>
          )}
        </Link>

        <Link to={ROUTES.PROFILE_ME} className="flex items-center gap-2">
          {user?.avatarUrl ? (
            <img src={user.avatarUrl} alt={user.displayName} className="size-9 rounded-full object-cover" />
          ) : user ? (
            <span className="flex size-9 items-center justify-center rounded-full bg-secondary/20 text-sm font-semibold text-primary">
              {getInitials(user.displayName)}
            </span>
          ) : (
            <FaUserCircle aria-hidden="true" className="size-9 text-secondary" />
          )}
          {user?.role === "Admin" && (
            <span className="inline-flex items-center rounded-full bg-primary/10 px-2 py-0.5 text-[11px] leading-4 font-medium text-primary">
              Admin
            </span>
          )}
        </Link>
      </div>
    </header>
  );
}
