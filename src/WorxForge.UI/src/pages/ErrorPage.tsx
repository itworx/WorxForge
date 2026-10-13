import { StatusPage } from "../components/StatusPage";
import { ROUTES } from "../constants/routes.constants";

export function ErrorPage() {
  return (
    <StatusPage
      fullscreen
      statusCode="500"
      title="Something went wrong"
      description="An unexpected error occurred. Try again, or head back home."
      action={{ label: "Try again", onClick: () => window.location.reload() }}
      secondaryAction={{ label: "Back to home", to: ROUTES.HOME }}
    />
  );
}
