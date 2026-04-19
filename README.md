# Keycloak React + .NET Demo

This project demonstrates a browser-based OpenID Connect integration using Keycloak, a React SPA, and multiple ASP.NET Core APIs.

## Components

- `infra`: Docker Compose and Keycloak realm bootstrap assets
- `frontend`: Vite + React + TypeScript SPA using Authorization Code Flow with PKCE
- `backend`: multiple ASP.NET Core APIs validating Keycloak-issued JWT bearer tokens
- `docs`: focused setup and architecture notes

## Local URLs

- Keycloak: `http://localhost:8080`
- React SPA: `http://localhost:5173`
- API 1: `http://localhost:8081`
- API 2: `http://localhost:8082`

## Demo flow

1. The SPA redirects the user to Keycloak's authorization endpoint.
2. The user signs in on the Keycloak-hosted login page.
3. Keycloak redirects back to the SPA with an authorization code.
4. The SPA exchanges the code for tokens.
5. The SPA uses the access token to call multiple secured .NET APIs.

## Demo identities

- `alice / Passw0rd!`: authenticated user with `department=finance`; allowed to call the finance-protected endpoints in both APIs.
- `bob / Passw0rd!`: authenticated user with `department=sales`; denied by the finance-only policy in both APIs.

## Run locally

1. Copy `.env.example` to `.env` if you want to override the default container ports or admin credentials.
2. Start everything with the helper script:
   - `./start-demo.sh`
   - or on Windows: `powershell -ExecutionPolicy Bypass -File .\start-demo.ps1`
3. Or start the services manually:
   - `docker compose -f infra/docker-compose.yml up --build`
4. In a second terminal, start the frontend:
   - `cd frontend`
   - `npm install`
   - `npm run dev`
5. Open `http://localhost:5173`.
6. Click `Sign in with Keycloak` and authenticate as `alice` or `bob`.
7. Use the frontend to call both secured APIs with the same access token.

If login succeeds but the APIs return `401`, restart the stack after config changes with `./start-demo.sh` so the containers reload the Keycloak issuer settings.

## What the API enforces

- Primary API:
  - `GET /api/auth/me`: any authenticated user
  - `GET /api/auth/diagnostics`: authenticated diagnostics for authority, issuer, audience, and current token metadata
  - `GET /api/demo/protected`: any authenticated user
  - `GET /api/demo/claims-protected`: requires `department=finance`
- Reporting API:
  - `GET /api/reports/summary`: any authenticated user
  - `GET /api/reports/finance`: requires `department=finance`

Detailed setup steps are documented in `docs/setup.md`, and the architecture rationale is in `docs/architecture.md`.

## Reusable Guides

- `docs/react-keycloak-integration.md`: how to integrate any React SPA with Keycloak
- `docs/dotnet-api-keycloak-integration.md`: how to secure any ASP.NET Core API with Keycloak-issued tokens
- `docs/keycloak-setup-guide.md`: how to configure Keycloak realms, clients, scopes, claims, and users
- `docs/authentication-authorization-flow.md`: full end-to-end explanation of the login, token, validation, and authorization flow used by this demo
