import { useAuthContext } from "../../../contexts/AuthContext";

export function useAuthentication() {
  return useAuthContext();
}


