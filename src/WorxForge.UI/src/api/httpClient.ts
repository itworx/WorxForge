import { getAccessToken } from "../services/tokenService";

export class HttpRequestError extends Error {
  readonly status: number;

  constructor(message: string, status: number) {
    super(message);
    this.name = "HttpRequestError";
    this.status = status;
  }
}

function normalizeBaseUrl(baseUrl: string): string {
  return baseUrl.trim().replace(/\/$/, "");
}

function buildUrl(path: string): string {
  const normalizedPath = path.startsWith("/") ? path : `/${path}`;
  const baseUrl = normalizeBaseUrl(import.meta.env.VITE_API_BASE_URL ?? "");
  return baseUrl ? `${baseUrl}${normalizedPath}` : normalizedPath;
}

function isJsonResponse(response: Response): boolean {
  const contentType = response.headers.get("Content-Type");
  return typeof contentType === "string" && contentType.toLowerCase().includes("application/json");
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await createDevAuthenticatedFetch().fetch(buildUrl(path), {
    ...init,
    headers: {
      Accept: "application/json",
      ...(init?.body ? { "Content-Type": "application/json" } : {}),
      ...(init?.headers ?? {})
    }
  });

  if (!response.ok) {
    const errorBody = await response.text();
    throw new HttpRequestError(errorBody || `Request failed with status ${response.status}.`, response.status);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  if (!isJsonResponse(response)) {
    return undefined as T;
  }

  return await response.json() as T;
}

export const httpClient = {
  get: <T>(path: string) => request<T>(path, { method: "GET" }),
  post: <T>(path: string, body: unknown) => request<T>(path, { method: "POST", body: JSON.stringify(body) }),
  put: <T>(path: string, body: unknown) => request<T>(path, { method: "PUT", body: JSON.stringify(body) }),
  delete: <T>(path: string) => request<T>(path, { method: "DELETE" }),
};

export interface FetchLike {
  fetch(url: RequestInfo, init?: RequestInit): Promise<Response>;
}

/** Attaches the access token issued by the backend authentication flow. */
export function createDevAuthenticatedFetch(): FetchLike {
  return {
    fetch(url, init) {
      const token = getAccessToken();

      if (!token) return fetch(url, init);

      const headers = new Headers(init?.headers);
      headers.set("Authorization", `Bearer ${token}`);
      return fetch(url, { ...init, headers });
    }
  };
}

