# Architecture Notes

## Frontend

- React SPA with a dedicated authentication service
- Authorization Code Flow with PKCE
- Centralized token-aware API client
- Minimal explicit scope request of `openid`, with profile/email provided by Keycloak default client scopes
- Direct construction of the Keycloak auth, token, and logout endpoints from the configured realm to avoid a brittle browser-time discovery dependency
- Explicit handling for:
  - authorization redirect generation
  - PKCE verifier and state storage in `sessionStorage`
  - callback processing and code exchange
  - logout through Keycloak's end-session endpoint

## Backend

- ASP.NET Core API
- ASP.NET Core reporting API
- JWT bearer token validation using Keycloak discovery metadata
- Policy-based authorization for authenticated and claim-protected endpoints
- Explicit Keycloak claim normalization so nested realm roles can be exposed as standard role claims
- Both APIs accept the same Keycloak audience so a single SPA access token can be used across services

## Identity

- Keycloak realm dedicated to the demo
- Demo users with different access levels
- Protocol mappers emitting authorization claims used by the API

## Endpoint model

- `/api/auth/me`: returns normalized user identity and claims for inspection
- `/api/auth/diagnostics`: returns JWT validation config and current token issuer/audience for troubleshooting
- `/api/demo/protected`: requires a valid bearer token
- `/api/demo/claims-protected`: requires the custom `department=finance` claim
- `/api/reports/summary`: second API showing the same token works across a separate project
- `/api/reports/finance`: second API endpoint with the same finance claim policy
