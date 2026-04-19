# Setup Guide

This demo uses:

- Keycloak in Docker on `http://localhost:8080`
- ASP.NET Core API in Docker on `http://localhost:8081`
- ASP.NET Core reporting API in Docker on `http://localhost:8082`
- React SPA running locally on `http://localhost:5173`

## Development sequence

1. Copy `.env.example` to `.env` at the repo root if you want to change ports or admin credentials.

-
1. Start everything with a helper script:
- macOS/Linux: `./start-demo.sh`
- Windows: `powershell -ExecutionPolicy Bypass -File .\start-demo.ps1`

-
1. Or start manually:
- `docker compose -f infra/docker-compose.yml up --build`
- `cd frontend`
- `npm install`
- `npm run dev`

1. Open `http://localhost:5173`.

1. Click the login button and complete authentication on the Keycloak-hosted page.

1. Return to the SPA callback route, complete the code exchange, and call the demo API endpoints across both .NET services.

## Keycloak bootstrap

The realm import file is `infra/keycloak/realm/demo-realm.json`.

It configures:

- Realm: `demo-realm`
- Realm SSL requirement: `none` for local HTTP testing only
- Public SPA client: `react-spa`
- Bearer-only API audience client: `dotnet-api`
- A dedicated client scope named `demo-claims`
- Demo users `alice` and `bob`

The `demo-claims` scope adds:

- `department` as a token claim derived from the Keycloak user attribute
- `dotnet-api` as an audience claim so the ASP.NET Core API can validate intended token usage
- The same audience is accepted by both demo APIs so one token can be used against multiple secured backend projects
- The APIs fetch Keycloak metadata using the internal Docker hostname but validate tokens against the public issuer `http://localhost:8080/realms/demo-realm`

## OAuth details

- The SPA is redirected to Keycloak's authorization endpoint.
- The SPA requests the minimal OIDC scope `openid`; profile and email claims are supplied by the client's default scopes.
- Keycloak authenticates the user on its own login page.
- Keycloak redirects back with an authorization code.
- The SPA exchanges the code for tokens using the token endpoint and the original PKCE verifier.
- The API validates the resulting bearer token using the realm authority and JWKS metadata.
- The same access token is then sent to both backend services to demonstrate multi-API use.

## Demo accounts

- `alice / Passw0rd!`
  - `department=finance`
  - Allowed on `/api/demo/claims-protected`
  - Allowed on `/api/reports/finance`
- `bob / Passw0rd!`
  - `department=sales`
  - Rejected with `403 Forbidden` on `/api/demo/claims-protected`
  - Rejected with `403 Forbidden` on `/api/reports/finance`

## Security model

- The React app is a public OAuth client.
- Authentication uses Authorization Code Flow with PKCE.
- The SPA never collects the username and password directly.
- The .NET API validates signed bearer tokens issued by Keycloak.
- Claim-based authorization is enforced by API policies.
- Local HTTP, `sslRequired=none`, and `RequireHttpsMetadata=false` are demo-only concessions.
- In production, use HTTPS, stronger secret management, hardened cookie/session patterns if a BFF is introduced, and production-grade Keycloak operational settings.
