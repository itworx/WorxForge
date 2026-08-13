function base64UrlDecode(segment: string): string {
  const normalized = segment.replace(/-/g, "+").replace(/_/g, "/");
  const padded = normalized.padEnd(normalized.length + ((4 - (normalized.length % 4)) % 4), "=");
  return atob(padded);
}

/** Decodes a JWT's payload without verifying its signature (verification happens server-side). */
export function decodeJwtPayload(token: string): Record<string, unknown> | null {
  const parts = token.split(".");
  if (parts.length !== 3) {
    return null;
  }

  try {
    const json = decodeURIComponent(
      Array.from(base64UrlDecode(parts[1]))
        .map((char) => `%${char.charCodeAt(0).toString(16).padStart(2, "0")}`)
        .join("")
    );
    return JSON.parse(json) as Record<string, unknown>;
  } catch {
    return null;
  }
}

const SUB_CLAIM = "sub";
const LEGACY_NAME_ID_CLAIM = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

/** Reads the user id ("sub") claim from a JWT access token. */
export function getUserIdFromToken(token: string): string | null {
  const payload = decodeJwtPayload(token);
  const userId = payload?.[SUB_CLAIM] ?? payload?.[LEGACY_NAME_ID_CLAIM];
  return typeof userId === "string" ? userId : null;
}
