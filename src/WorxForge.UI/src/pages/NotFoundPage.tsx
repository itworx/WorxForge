import { StatusPage } from "../components/StatusPage";
import { ROUTES } from "../constants/routes.constants";

export function NotFoundPage() {
  return (
    <StatusPage
      statusCode="404"
      title="Page not found"
      description="The page you are looking for doesn't exist or may have been moved."
      action={{ label: "Back to home", to: ROUTES.HOME }}
      secondaryAction={{ label: "Explore assets", to: ROUTES.EXPLORE }}
    />
  );
}
