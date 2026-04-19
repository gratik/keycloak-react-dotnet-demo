import { useEffect, useState } from "react";
import { callApi } from "./api";
import { authConfig, validateConfig } from "./auth/config";
import {
  beginLogin,
  clearSession,
  completeLogin,
  decodeJwtPayload,
  logout,
  readSession,
  type AuthSession
} from "./auth/oidc";

type ApiResult = {
  label: string;
  status: number;
  body: unknown;
} | null;

function pretty(value: unknown): string {
  return JSON.stringify(value, null, 2);
}

export default function App() {
  const [session, setSession] = useState<AuthSession | null>(null);
  const [busy, setBusy] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [apiResult, setApiResult] = useState<ApiResult>(null);

  useEffect(() => {
    validateConfig();

    const initialize = async () => {
      try {
        const url = new URL(window.location.href);
        const code = url.searchParams.get("code");
        const state = url.searchParams.get("state");

        if (window.location.pathname === "/auth/callback" && code) {
          // Clear the callback URL before exchanging the code so React dev
          // re-renders or StrictMode remounts do not try to redeem it twice.
          window.history.replaceState({}, document.title, "/");
          const nextSession = await completeLogin(code, state);
          setSession(nextSession);
          return;
        }

        setSession(readSession());
      } catch (nextError) {
        clearSession();
        setError(
          nextError instanceof Error ? nextError.message : "Authentication failed."
        );
      } finally {
        setBusy(false);
      }
    };

    void initialize();
  }, []);

  const accessClaims = session ? decodeJwtPayload(session.accessToken) : null;
  const identityClaims = session ? decodeJwtPayload(session.idToken) : null;

  async function handleLogin() {
    try {
      setError(null);
      await beginLogin();
    } catch (nextError) {
      setError(nextError instanceof Error ? nextError.message : "Login failed.");
    }
  }

  async function handleApiCall(label: string, path: string) {
    if (!session) {
      return;
    }

    setError(null);
    const result = await callApi(authConfig.apiBaseUrl, path, session);
    setApiResult({ label, ...result });
  }

  async function handleReportingApiCall(label: string, path: string) {
    if (!session) {
      return;
    }

    setError(null);
    const result = await callApi(authConfig.reportingApiBaseUrl, path, session);
    setApiResult({ label, ...result });
  }

  if (busy) {
    return <main className="shell"><div className="card">Loading demo...</div></main>;
  }

  return (
    <main className="shell">
      <section className="hero">
        <p className="eyebrow">Keycloak + React + ASP.NET Core</p>
        <h1>Authorization Code + PKCE Demo</h1>
        <p className="lede">
          The SPA redirects users to Keycloak, receives an authorization code,
          exchanges it for tokens, and calls a secured .NET API with the access
          token.
        </p>
      </section>

      <section className="grid">
        <article className="card">
          <h2>Session</h2>
          <p>
            Keycloak: <strong>{authConfig.keycloakUrl}</strong>
          </p>
          <p>
            API 1: <strong>{authConfig.apiBaseUrl}</strong>
          </p>
          <p>
            API 2: <strong>{authConfig.reportingApiBaseUrl}</strong>
          </p>
          {!session ? (
            <>
              <p>You are signed out. Login happens on the Keycloak-hosted page.</p>
              <button onClick={() => void handleLogin()}>Sign in with Keycloak</button>
            </>
          ) : (
            <>
              <p>
                Signed in as <strong>{String(identityClaims?.preferred_username ?? "user")}</strong>
              </p>
              <div className="button-row">
                <button onClick={() => void handleApiCall("Profile", "/api/auth/me")}>
                  Load /api/auth/me
                </button>
                <button
                  onClick={() =>
                    void handleApiCall("Protected", "/api/demo/protected")
                  }
                >
                  Load protected endpoint
                </button>
                <button
                  onClick={() =>
                    void handleApiCall(
                      "Claims protected",
                      "/api/demo/claims-protected"
                    )
                  }
                >
                  Load claims-protected endpoint
                </button>
                <button
                  onClick={() =>
                    void handleReportingApiCall("Reporting summary", "/api/reports/summary")
                  }
                >
                  Load reporting summary
                </button>
                <button
                  onClick={() =>
                    void handleReportingApiCall("Reporting finance", "/api/reports/finance")
                  }
                >
                  Load reporting finance
                </button>
                <button
                  className="secondary"
                  onClick={() => {
                    setApiResult(null);
                    void logout();
                  }}
                >
                  Logout
                </button>
              </div>
            </>
          )}
          {error ? <p className="error">{error}</p> : null}
        </article>

        <article className="card">
          <h2>Demo users</h2>
          <ul>
            <li><strong>alice / Passw0rd!</strong> has the `department=finance` claim.</li>
            <li><strong>bob / Passw0rd!</strong> authenticates successfully but is denied the finance-only endpoints in both APIs.</li>
          </ul>
        </article>

        <article className="card">
          <h2>ID token claims</h2>
          <pre>{identityClaims ? pretty(identityClaims) : "Sign in to inspect claims."}</pre>
        </article>

        <article className="card">
          <h2>Access token claims</h2>
          <pre>{accessClaims ? pretty(accessClaims) : "Sign in to inspect claims."}</pre>
        </article>

        <article className="card wide">
          <h2>API result</h2>
          <pre>
            {apiResult
              ? pretty(apiResult)
              : "Call an endpoint after signing in to inspect the API response."}
          </pre>
        </article>
      </section>
    </main>
  );
}
