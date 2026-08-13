import { Client, type LoginUserCommand } from "../../../api/worxforgeClient";
import { getAccessToken } from "../../../services/tokenService";
import type { AuthToken, UserSession } from "../models/authentication.models";

export interface AuthenticationService {
  loginWithGoogle(idToken: string): Promise<AuthToken>;
  getCurrentUser(signal?: AbortSignal): Promise<UserSession>;
  logout(signal?: AbortSignal): Promise<void>;
}

type MeResponse = {
  userId?: string;
  displayName?: string;
  username?: string;
  role?: string | number;
  avatarUrl?: string;
};

const LOGIN_PATH = "/api/authentication/login";
const ME_PATH = "/api/authentication/me";

function normalizeBaseUrl(baseUrl: string): string {
  return baseUrl.trim().replace(/\/$/, "");
}

function toPath(url: string): string {
  try {
    return new URL(url, window.location.origin).pathname;
  } catch {
    return url;
  }
}

function isPathMatch(requestUrl: string, expectedPath: string): boolean {
  const path = toPath(requestUrl).replace(/\/$/, "");
  const normalizedExpectedPath = expectedPath.replace(/\/$/, "");
  return path.endsWith(normalizedExpectedPath);
}

function mapRole(role: string | number | undefined): UserSession["role"] {
  if (role === "Admin" || role === 1 || role === 2) {
    return "Admin";
  }

  return "User";
}

function mapMeResponse(payload: MeResponse): UserSession {
  return {
    userId: payload.userId ?? "",
    displayName: payload.displayName?.trim() || payload.username?.trim() || "WorxForge User",
    role: mapRole(payload.role),
    avatarUrl: payload.avatarUrl
  };
}

async function readJsonClone<T>(response: Response): Promise<T | undefined> {
  const contentType = response.headers.get("Content-Type") ?? "";
  if (!contentType.toLowerCase().includes("application/json")) {
    return undefined;
  }

  const body = await response.clone().text();
  if (!body.trim()) {
    return undefined;
  }

  return JSON.parse(body) as T;
}

export function createAuthenticationService(baseUrl: string = import.meta.env.VITE_API_BASE_URL): AuthenticationService {
  const normalizedBaseUrl = normalizeBaseUrl(baseUrl ?? "");
  let cachedLoginPayload: AuthToken | undefined;
  let cachedMePayload: MeResponse | undefined;

  const client = new Client(normalizedBaseUrl, {
    async fetch(url, init) {
      const headers = new Headers(init?.headers);
      const accessToken = getAccessToken();
      if (accessToken) {
        headers.set("Authorization", `Bearer ${accessToken}`);
      }

      const response = await fetch(url, { ...init, headers });
      const requestUrl = typeof url === "string" ? url : url instanceof URL ? url.toString() : url.url;

      if (isPathMatch(requestUrl, LOGIN_PATH)) {
        cachedLoginPayload = response.ok ? await readJsonClone<AuthToken>(response) : undefined;
      }

      if (isPathMatch(requestUrl, ME_PATH)) {
        cachedMePayload = response.ok ? await readJsonClone<MeResponse>(response) : undefined;
      }

      return response;
    }
  });

  return {
    async loginWithGoogle(idToken: string) {
      cachedLoginPayload = undefined;
      const command: LoginUserCommand = { idToken };
      await client.login(command);

      const loginPayload = cachedLoginPayload as AuthToken | undefined;
      const accessToken = loginPayload?.accessToken?.trim();
      if (!accessToken) {
        throw new Error("Authentication response did not include an access token.");
      }

      return { accessToken };
    },

    async getCurrentUser(signal?: AbortSignal) {
      cachedMePayload = undefined;
      await client.meGET(signal);

      const mePayload = cachedMePayload as MeResponse | undefined;
      if (!mePayload) {
        throw new Error("Current user response payload was empty.");
      }

      return mapMeResponse(mePayload);
    },

    logout(signal?: AbortSignal) {
      return client.logout(signal);
    }
  };
}


