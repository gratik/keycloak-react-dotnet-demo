import { authConfig } from "./auth/config";
import type { AuthSession } from "./auth/oidc";

export async function callApi(
  baseUrl: string,
  path: string,
  session: AuthSession
): Promise<{ status: number; body: unknown }> {
  const response = await fetch(`${baseUrl}${path}`, {
    headers: {
      Authorization: `Bearer ${session.accessToken}`
    }
  });

  const text = await response.text();
  let body: unknown = text;

  if (text) {
    try {
      body = JSON.parse(text) as unknown;
    } catch {
      body = text;
    }
  }

  return { status: response.status, body };
}
