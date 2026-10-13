import { StatusPage } from "../components/StatusPage";
import { ROUTES } from "../constants/routes.constants";

export function UnauthorizedPage() {
  return (
    <StatusPage
      statusCode="403"
      title="Access restricted"
      description="This area is limited to administrators, and your account doesn't have access. You can keep exploring the hub instead."
      action={{ label: "Back to home", to: ROUTES.HOME }}
      secondaryAction={{ label: "Explore assets", to: ROUTES.EXPLORE }}
    />
  );
}
