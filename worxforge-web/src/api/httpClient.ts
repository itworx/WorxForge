// TODO: VITE_API_BASE_URL must be set in .env before implementation.
async function request<T>(_path: string, _init?: RequestInit): Promise<T> {
  // TODO: Implementation
  throw new Error("TODO: Implementation");
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

/**
 * TEMPORARY: attaches a bearer token from VITE_DEV_AUTH_TOKEN to every request so the
 * generated API clients can be exercised before real authentication is wired up.
 * Remove this once login + tokenService are implemented.
 */
export function createDevAuthenticatedFetch(): FetchLike {
  const devToken = import.meta.env.VITE_DEV_AUTH_TOKEN as string | undefined;

  return {
    fetch(url, init) {
      if (!devToken) return fetch(url, init);

      const headers = new Headers(init?.headers);
      headers.set("Authorization", `Bearer ${devToken}`);
      return fetch(url, { ...init, headers });
    }
  };
}

