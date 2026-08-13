import { RouterProvider } from "react-router-dom";
import { AppProviders } from "../providers/AppProviders";
import { appRouter } from "../routes/appRouter";

export function App() {
  // TODO: Implementation
  return (
    <AppProviders>
      <RouterProvider router={appRouter} />
    </AppProviders>
  );
}

