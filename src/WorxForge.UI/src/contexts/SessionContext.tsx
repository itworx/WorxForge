import { createContext, useContext } from "react";
import type { PropsWithChildren } from "react";

type SessionContextValue = {
  navigationKey: string;
};

const SessionContext = createContext<SessionContextValue>({ navigationKey: "" });

export function SessionProvider({ children }: PropsWithChildren) {
  // TODO: Implementation
  return <SessionContext.Provider value={{ navigationKey: "" }}>{children}</SessionContext.Provider>;
}

export function useSessionContext(): SessionContextValue {
  // TODO: Implementation
  return useContext(SessionContext);
}

