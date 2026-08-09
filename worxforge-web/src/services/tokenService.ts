// TODO: Token storage strategy (httpOnly cookie vs localStorage) to be decided once login/logout exist.
import { getUserIdFromToken } from "../utils/jwt.utils";

/** TEMPORARY: hardcoded to VITE_DEV_AUTH_TOKEN until real authentication is implemented. */
export function getAccessToken(): string | null {
  const devToken = import.meta.env.VITE_DEV_AUTH_TOKEN as string | undefined;
  return devToken && devToken.length > 0 ? devToken : null;
}

export function setAccessToken(_token: string): void {
  // TODO: Implementation (once real login exists)
}

export function clearAccessToken(): void {
  // TODO: Implementation (once real logout exists)
}

/** Derives the current user id from the "sub" claim of the access token, if any. */
export function getCurrentUserId(): string | null {
  const token = getAccessToken();
  return token ? getUserIdFromToken(token) : null;
}
