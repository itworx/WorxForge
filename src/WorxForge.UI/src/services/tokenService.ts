import { getUserIdFromToken } from "../utils/jwt.utils";

const ACCESS_TOKEN_STORAGE_KEY = "worxforge.access-token";

function getPersistentStorage(): Storage | null {
  if (typeof window === "undefined") {
    return null;
  }

  return window.localStorage;
}

function getSessionStorage(): Storage | null {
  if (typeof window === "undefined") {
    return null;
  }

  return window.sessionStorage;
}

export function getAccessToken(): string | null {
  try {
    const persistentStorage = getPersistentStorage();
    const sessionStorage = getSessionStorage();
    const persistentValue = persistentStorage?.getItem(ACCESS_TOKEN_STORAGE_KEY) ?? null;
    const sessionValue = sessionStorage?.getItem(ACCESS_TOKEN_STORAGE_KEY) ?? null;
    const value = persistentValue ?? sessionValue;

    if (!value) {
      return null;
    }

    const trimmed = value.trim();

    if (!persistentValue && sessionValue && trimmed) {
      persistentStorage?.setItem(ACCESS_TOKEN_STORAGE_KEY, trimmed);
      sessionStorage?.removeItem(ACCESS_TOKEN_STORAGE_KEY);
    }

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
    const persistentStorage = getPersistentStorage();
    const sessionStorage = getSessionStorage();
    persistentStorage?.setItem(ACCESS_TOKEN_STORAGE_KEY, trimmed);
    sessionStorage?.removeItem(ACCESS_TOKEN_STORAGE_KEY);
  } catch {
    // Ignore storage failures in restricted browser contexts.
  }
}

export function clearAccessToken(): void {
  try {
    const persistentStorage = getPersistentStorage();
    const sessionStorage = getSessionStorage();
    persistentStorage?.removeItem(ACCESS_TOKEN_STORAGE_KEY);
    sessionStorage?.removeItem(ACCESS_TOKEN_STORAGE_KEY);
  } catch {
    // Ignore storage failures in restricted browser contexts.
  }
}

/** Derives the current user id from the "sub" claim of the access token, if any. */
export function getCurrentUserId(): string | null {
  const token = getAccessToken();
  return token ? getUserIdFromToken(token) : null;
}
