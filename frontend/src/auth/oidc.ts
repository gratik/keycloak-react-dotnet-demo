import { authConfig } from "./config";
import { createPkcePair, createState } from "./pkce";

const TRANSACTION_KEY = "keycloak-demo:transaction";
const SESSION_KEY = "keycloak-demo:session";

export type AuthSession = {
  accessToken: string;
  idToken: string;
  refreshToken?: string;
  expiresAt: number;
};

type AuthTransaction = {
  state: string;
  codeVerifier: string;
};

function realmBaseUrl(): string {
  return `${authConfig.keycloakUrl}/realms/${authConfig.realm}/protocol/openid-connect`;
}

function persistTransaction(transaction: AuthTransaction): void {
  sessionStorage.setItem(TRANSACTION_KEY, JSON.stringify(transaction));
}

function readTransaction(): AuthTransaction | null {
  const raw = sessionStorage.getItem(TRANSACTION_KEY);
  return raw ? (JSON.parse(raw) as AuthTransaction) : null;
}

function clearTransaction(): void {
  sessionStorage.removeItem(TRANSACTION_KEY);
}

export function readSession(): AuthSession | null {
  const raw = sessionStorage.getItem(SESSION_KEY);

  if (!raw) {
    return null;
  }

  const session = JSON.parse(raw) as AuthSession;
  if (session.expiresAt <= Date.now()) {
    clearSession();
    return null;
  }

  return session;
}

export function saveSession(session: AuthSession): void {
  sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));
}

export function clearSession(): void {
  sessionStorage.removeItem(SESSION_KEY);
}

export async function beginLogin(): Promise<void> {
  const { codeVerifier, codeChallenge } = await createPkcePair();
  const state = createState();

  persistTransaction({ state, codeVerifier });

  const url = new URL(`${realmBaseUrl()}/auth`);
  url.searchParams.set("client_id", authConfig.clientId);
  url.searchParams.set("redirect_uri", authConfig.redirectUri);
  url.searchParams.set("response_type", "code");
  // Request only the mandatory OIDC scope. Profile and email claims arrive
  // through the client's configured default scopes in Keycloak.
  url.searchParams.set("scope", "openid");
  url.searchParams.set("state", state);
  url.searchParams.set("code_challenge", codeChallenge);
  url.searchParams.set("code_challenge_method", "S256");

  window.location.assign(url.toString());
}

export async function completeLogin(
  code: string,
  returnedState: string | null
): Promise<AuthSession> {
  const transaction = readTransaction();

  if (!transaction) {
    throw new Error("Missing authorization transaction state.");
  }

  if (!returnedState || returnedState !== transaction.state) {
    throw new Error("Authorization state validation failed.");
  }

  const body = new URLSearchParams({
    grant_type: "authorization_code",
    client_id: authConfig.clientId,
    code,
    redirect_uri: authConfig.redirectUri,
    code_verifier: transaction.codeVerifier
  });

  const response = await fetch(`${realmBaseUrl()}/token`, {
    method: "POST",
    headers: {
      "Content-Type": "application/x-www-form-urlencoded"
    },
    body
  });

  clearTransaction();

  if (!response.ok) {
    const detail = await response.text();
    throw new Error(`Token exchange failed: ${detail}`);
  }

  const payload = (await response.json()) as {
    access_token: string;
    id_token: string;
    refresh_token?: string;
    expires_in: number;
  };

  const session = {
    accessToken: payload.access_token,
    idToken: payload.id_token,
    refreshToken: payload.refresh_token,
    expiresAt: Date.now() + payload.expires_in * 1000
  };

  saveSession(session);
  return session;
}

export async function logout(): Promise<void> {
  const session = readSession();

  clearSession();
  clearTransaction();

  const url = new URL(`${realmBaseUrl()}/logout`);
  url.searchParams.set("post_logout_redirect_uri", window.location.origin);

  if (session?.idToken) {
    url.searchParams.set("id_token_hint", session.idToken);
  }

  window.location.assign(url.toString());
}

export function decodeJwtPayload(token: string): Record<string, unknown> | null {
  const parts = token.split(".");
  if (parts.length < 2) {
    return null;
  }

  const normalized = parts[1].replace(/-/g, "+").replace(/_/g, "/");
  const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, "=");

  try {
    return JSON.parse(atob(padded)) as Record<string, unknown>;
  } catch {
    return null;
  }
}
