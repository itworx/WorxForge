import { useAuthContext } from "../../contexts/AuthContext";
import { APP_NAME } from "../../constants/app.constants";

export function Footer() {
  const { user } = useAuthContext();

  return (
    <footer className="border-t border-secondary/20 bg-white px-4 py-4 text-center text-sm text-text/70 sm:px-6">
      {user ? `Signed in as ${user.displayName} · ${APP_NAME}` : APP_NAME}
    </footer>
  );
}
