import type { PropsWithChildren } from "react";
import { useState } from "react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Toaster } from "sonner";
import { AuthProvider } from "../contexts/AuthContext";
import { SessionProvider } from "../contexts/SessionContext";
import { NotificationProvider } from "../contexts/NotificationContext";

export function AppProviders({ children }: PropsWithChildren) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            retry: 1,
            refetchOnWindowFocus: false
          }
        }
      })
  );

  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <SessionProvider>
          <NotificationProvider>
            {children}
            <Toaster
              richColors
              closeButton
              toastOptions={{
                classNames: {
                  closeButton: "!left-auto !right-2"
                }
              }}
            />
          </NotificationProvider>
        </SessionProvider>
      </AuthProvider>
    </QueryClientProvider>
  );
}

