# .NET API to Keycloak Integration Guide

This guide describes how to secure any ASP.NET Core API so it validates bearer tokens issued by Keycloak.

## Prerequisites

- A running Keycloak instance
- A realm for the application
- An API audience or client definition in Keycloak
- An ASP.NET Core API project

## Step-by-step setup

1. Decide what the API should accept.
   - Which Keycloak realm issues the token
   - Which audience the token must contain
   - Which claims or roles are needed for authorization

2. Add JWT bearer authentication to the API.
   - Use ASP.NET Core JWT bearer auth
   - Point `Authority` at the Keycloak realm
   - Configure the expected `Audience`

3. If Docker is involved, separate discovery from issuer if needed.
   - Containers may need to reach Keycloak by an internal hostname such as `http://keycloak:8080`
   - Browser-issued tokens may still have a public issuer such as `http://localhost:8080`
   - If those differ, fetch metadata using the internal authority but validate the token against the public issuer

4. Configure validation explicitly.
   - Validate issuer
   - Validate audience
   - Validate lifetime
   - Validate signature using Keycloak metadata and JWKS

5. Disable default inbound claim remapping if you want predictable claim names.
   - Keep claims like `preferred_username`, `department`, and `iss` in their original names

6. Add authorization policies.
   - Require authentication for general protected endpoints
   - Add claim-based policies for business rules, for example `department=finance`
   - Add role-based policies if you expose Keycloak roles as role claims

7. Normalize Keycloak-specific claims if necessary.
   - Realm roles are often nested under `realm_access.roles`
   - If your app expects standard ASP.NET role claims, add a claims transformation step

8. Protect controllers and endpoints.
   - Use `[Authorize]` for any authenticated user
   - Use `[Authorize(Policy = "...")]` for claim-specific or role-specific routes

9. Add CORS if the API is called from a browser app.
   - Restrict allowed origins to your frontend
   - Allow the `Authorization` header

10. Add diagnostics for troubleshooting.
   - A small authenticated diagnostics endpoint is useful during development
   - Return config values such as authority, public issuer, audience, and observed token metadata

## Gotchas

- A token can be valid in the browser and still fail in the API because of issuer mismatch.
- Audience mismatch is another very common reason for `401 Unauthorized`.
- If you are using Docker, `localhost` inside a container does not mean the host machine.
- If the API accepts a tampered token, signature validation is not configured correctly.
- If authorization fails with `403`, authentication likely worked and the claim or role policy is what rejected the user.
- If the API cannot load Keycloak metadata, check network reachability from the API runtime, not just from your browser.

## Tips and tricks

- Keep Keycloak auth settings in one config section such as:
  - `Authority`
  - `PublicIssuer`
  - `Audience`
  - `RequireHttpsMetadata`
- Log authentication failures in development. The exact issuer or audience mismatch usually appears in the exception.
- Add one endpoint that returns current user claims so frontend and backend teams can compare what the token contains.
- Keep integration tests for:
  - no token returns `401`
  - valid token returns `200`
  - missing required claim returns `403`
  - required claim returns `200`
- Treat `RequireHttpsMetadata=false` as a local-only development setting.
