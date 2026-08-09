import { getUserIdFromToken } from "../utils/jwt.utils";

const ACCESS_TOKEN_STORAGE_KEY = "worxforge.access-token";

export function getAccessToken(): string | null {
  try {
    const value = window.sessionStorage.getItem(ACCESS_TOKEN_STORAGE_KEY);
    if (!value) {
      return null;
    }

    const trimmed = value.trim();
    return trimmed.length > 0 ? trimmed : null;
  } catch {
    return null;
  }
}

export function setAccessToken(token: string): void {
  const trimmed = token.trim();
  if (!trimmed) {
    clearAccessToken();
    return;
  }

  try {
    window.sessionStorage.setItem(ACCESS_TOKEN_STORAGE_KEY, trimmed);
  } catch {
    // Ignore storage failures in restricted browser contexts.
  }
}

export function clearAccessToken(): void {
  try {
    window.sessionStorage.removeItem(ACCESS_TOKEN_STORAGE_KEY);
  } catch {
    // Ignore storage failures in restricted browser contexts.
  }
}

/** Derives the current user id from the "sub" claim of the access token, if any. */
export function getCurrentUserId(): string | null {
  const token = getAccessToken();
  return token ? getUserIdFromToken(token) : null;
}
