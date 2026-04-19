

# Keycloak Setup Guide

## Table of Contents

1. [Tested stack](#tested-stack)
1. [Recommended runtime](#recommended-runtime)
1. [Copy-paste starter setup](#copy-paste-starter-setup)
1. [Prerequisites](#prerequisites)
1. [Step-by-step setup](#step-by-step-setup)
1. [Sequence diagram](#sequence-diagram)
1. [Example realm export fragment](#example-realm-export-fragment)
1. [Example decoded access token payload](#example-decoded-access-token-payload)
1. [How to adapt this to another-keycloak-setup](#how-to-adapt-this-to-another-keycloak-setup)
1. [Admin console checklist](#admin-console-checklist)
1. [Gotchas](#gotchas)
1. [Troubleshooting matrix](#troubleshooting-matrix)
1. [Tips and tricks](#tips-and-tricks)
1. [Minimum secure production changes](#minimum-secure-production-changes)
1. [Quick validation checklist](#quick-validation-checklist)
1. [Backup and restore](#backup-and-restore)
1. [Automated setup with-docker-compose](#automated-setup-with-docker-compose)
1. [API diagnostics endpoint](#api-diagnostics-endpoint)
1. [OIDC discovery endpoint](#oidc-discovery-endpoint)
1. [Related reading](#related-reading)
1. [References](#references)

This guide describes the practical steps for setting up Keycloak so browser apps and APIs can use it safely and predictably.

## Tested stack

- Keycloak 26.1
- Docker-based local runtime
- React SPA client using Authorization Code Flow with PKCE
- ASP.NET Core APIs validating JWT bearer tokens

## Recommended runtime

For local development, these are the most practical defaults:

- Keycloak in Docker
- One dedicated realm per demo or application
- One public SPA client for the browser app
- One bearer-only or audience-targeted API client for backend services
- Client scopes and protocol mappers for custom claims

Example local Docker run:

```bash
docker run --name keycloak \
  -p 8080:8080 \
  -e KEYCLOAK_ADMIN=admin \
  -e KEYCLOAK_ADMIN_PASSWORD=admin \
  quay.io/keycloak/keycloak:26.1 \
  start-dev
```json

## Copy-paste starter setup

Use this as the minimum reusable Keycloak setup for a browser SPA plus API:

```text
Realm name: demo-realm
Frontend client ID: react-spa
API audience/client ID: dotnet-api
Local frontend URL: http://localhost:5173
Local Keycloak URL: http://localhost:8080
```

Recommended local test users:

```text
alice / Passw0rd!  -> department=finance
bob   / Passw0rd!  -> department=sales
```

## Prerequisites

- Docker or another way to run Keycloak
- A plan for your realms, clients, users, roles, and claims
- Local URLs for your frontend and APIs

## Step-by-step setup


1. Start Keycloak.
  - For local work, a Docker setup is usually the fastest option
  - Create an admin user for the local environment
  - ![Keycloak Docker Startup](images/keycloak-docker-start.png)


1. Create a dedicated realm for your application or demo.
  - Do not reuse the master realm for application traffic
  - Keep realm names stable across environments when possible
  - ![Create Realm](images/keycloak-create-realm.png)

1. Configure SSL requirements for the environment.
   - For local HTTP testing only, set the realm SSL requirement to `none`
   - For production, require HTTPS

   In the admin UI:
   - Realm settings
   - Login or General settings depending on Keycloak version
   - Set SSL Required to:
     - `none` for local development only
     - `external` or stricter for real environments


1. Create the frontend client.
  - Use OpenID Connect
  - Make it a public client for a browser SPA
  - Enable standard flow
  - Enable PKCE with `S256`
  - Configure redirect URIs, web origins, and logout redirect URIs
  - ![Create SPA Client](images/keycloak-create-spa-client.png)

   Suggested admin UI values:
   - Client ID: `react-spa`
   - Client authentication: `Off`
   - Authorization: `Off`
   - Standard flow: `On`
   - Direct access grants: `Off`
   - Implicit flow: `Off`
   - Root URL: `http://localhost:5173`
   - Home URL: `http://localhost:5173`
   - Valid redirect URIs:
     - `http://localhost:5173/*`
   - Valid post logout redirect URIs:
     - `http://localhost:5173/*`
   - Web origins:
     - `http://localhost:5173`
   - PKCE code challenge method:
     - `S256`


1. Create API client or audience configuration.
  - For simple setups, a bearer-only API client is fine
  - Make sure access tokens include the audience your APIs expect
  - ![Create API Client](images/keycloak-create-api-client.png)

   Suggested API client values:
   - Client ID: `dotnet-api`
   - Client authentication: `Off`
   - Bearer-only if you want a pure API definition
   - No browser redirect URIs required


1. Create client scopes and mappers.
   - Add default scopes such as profile and email as needed
   - Add protocol mappers for any custom claims your apps need
   - Example custom claims:
     - department
     - tenant
     - application role markers
   - ![Add Client Scope and Mapper](images/keycloak-client-scope-mapper.png)

   Suggested client scope for this style of app:
   - Name: `demo-claims`
   - Attach it to the SPA client as a default client scope

1. Start Keycloak.

   - For local work, a Docker setup is usually the fastest option
   - Create an admin user for the local environment
   - ![Keycloak Docker Startup](images/keycloak-docker-start.png)

2. Create a dedicated realm for your application or demo.

   - Do not reuse the master realm for application traffic
   - Keep realm names stable across environments when possible
   - ![Create Realm](images/keycloak-create-realm.png)

3. Configure SSL requirements for the environment.

   - For local HTTP testing only, set the realm SSL requirement to `none`
   - For production, require HTTPS

   In the admin UI:
     - Realm settings
     - Login or General settings depending on Keycloak version
     - Set SSL Required to:
       - `none` for local development only
       - `external` or stricter for real environments

4. Create the frontend client.

   - Use OpenID Connect
   - Make it a public client for a browser SPA
   - Enable standard flow
   - Enable PKCE with `S256`
   - Configure redirect URIs, web origins, and logout redirect URIs
   - ![Create SPA Client](images/keycloak-create-spa-client.png)

   Suggested admin UI values:
     - Client ID: `react-spa`
     - Client authentication: `Off`
     - Authorization: `Off`
     - Standard flow: `On`
     - Direct access grants: `Off`
     - Implicit flow: `Off`
     - Root URL: `http://localhost:5173`
     - Home URL: `http://localhost:5173`
     - Valid redirect URIs:
       - `http://localhost:5173/*`
     - Valid post logout redirect URIs:
       - `http://localhost:5173/*`
     - Web origins:
       - `http://localhost:5173`
     - PKCE code challenge method:
       - `S256`

5. Create API client or audience configuration.

   - For simple setups, a bearer-only API client is fine
   - Make sure access tokens include the audience your APIs expect
   - ![Create API Client](images/keycloak-create-api-client.png)

   Suggested API client values:
     - Client ID: `dotnet-api`
     - Client authentication: `Off`
     - Bearer-only if you want a pure API definition
     - No browser redirect URIs required

6. Create client scopes and mappers.

   - Add default scopes such as profile and email as needed
   - Add protocol mappers for any custom claims your apps need
   - Example custom claims:
     - department
     - tenant
     - application role markers
   - ![Add Client Scope and Mapper](images/keycloak-client-scope-mapper.png)

   Suggested client scope for this style of app:
     - Name: `demo-claims`
     - Attach it to the SPA client as a default client scope

   Example mapper for a custom `department` claim:
     - Mapper type: User Attribute
     - User attribute: `department`
     - Token claim name: `department`
     - Claim JSON type: `String`
     - Add to access token: `On`
     - Add to ID token: `On`
     - Add to userinfo: `On`

   Example mapper for API audience:
     - Mapper type: Audience
     - Included client audience: `dotnet-api`
     - Add to access token: `On`
     - Add to ID token: usually `Off`

1. Create roles.

   - Add realm roles or client roles depending on your authorization design
   - Keep role naming consistent and readable

1. Create users for testing.

   - Add passwords
   - Add roles
   - Add user attributes used by custom claim mappers
   - ![Create Test User](images/keycloak-create-user.png)

   Example demo users:
     - `alice`
       - password: `Passw0rd!`
       - roles: `app-user`, `finance-reader`
       - attributes:
         - `department=finance`
     - `bob`
       - password: `Passw0rd!`
       - roles: `app-user`
       - attributes:
         - `department=sales`

1. Export and version your realm configuration.

   - Store a realm export or bootstrap configuration in source control for reproducible environments
   - Keep environment-specific secrets out of the export where possible

1. Test the full login and token flow.

    - Browser app redirects to Keycloak
    - User signs in
    - Keycloak returns an authorization code
    - App exchanges the code for tokens
    - API accepts the token and enforces authorization
    - ![Login Flow](images/keycloak-login-flow.png)
        {
          "name": "api-audience",
          "protocolMapper": "oidc-audience-mapper",
          "config": {
            "included.client.audience": "dotnet-api",
            "access.token.claim": "true",
            "id.token.claim": "false"
          }
        }
      ]
    }
  ]

}
```

## Example decoded access token payload

A correctly configured Keycloak token for this style of app often looks like:

```json
{
  "iss": "http://localhost:8080/realms/demo-realm",
  "aud": ["account", "dotnet-api"],
  "preferred_username": "alice",
  "department": "finance",
  "realm_access": {
    "roles": ["app-user", "finance-reader"]
  },
  "exp": 1893456789
}

```

This is the quickest way to verify whether your audience mapper and custom claim mapper are working.

## How to adapt this to another Keycloak setup

When reusing this pattern:

- Change:
  - realm name
  - frontend client ID
  - API audience/client ID
  - redirect URIs
  - web origins
  - logout URIs
  - custom claims and roles
- Keep:
  - Authorization Code Flow with PKCE for browser apps
  - a dedicated client scope for custom claims
  - a reproducible realm export
- Decide per project:
  - whether to use realm roles or client roles
  - whether one token should work across multiple APIs
  - whether the same realm serves multiple apps or each app has its own realm

## Admin console checklist

For every new application, confirm these settings in the Keycloak UI:

- Realm exists
- Realm SSL requirement is correct
- SPA client exists
- SPA redirect URIs are correct
- SPA web origins are correct
- PKCE is enabled for the SPA client
- API audience/client exists
- Client scope exists
- Protocol mappers are attached
- Test users exist
- Test users have correct roles
- Test users have correct custom attributes
- Tokens contain the expected audience and claims

## Gotchas

- Forgetting redirect URIs or web origins is one of the fastest ways to break login.
- If Keycloak shows `HTTPS required`, your realm SSL setting is too strict for local HTTP.
- If the browser app works but APIs return `401`, the Keycloak issuer or audience setup may not match backend validation.
- If the token is missing the claim you expected, check protocol mappers and whether the scope is actually applied.
- If Keycloak says `Invalid scopes`, the frontend may be requesting scopes that are not configured for the client.
- Docker hostnames and browser URLs are not the same thing. Plan for public versus internal addresses.
- If the SPA works in the browser but APIs in Docker reject tokens, you may need:
  - public issuer: `http://localhost:8080/...`
  - internal authority: `http://keycloak:8080/...`

## Security Notes

- Never use default admin credentials in production.
- Disable unused endpoints and features.
- Always require HTTPS in production.
- Regularly review and rotate secrets and credentials.
- Limit custom claims to only what is necessary.

## Backup and Restore

To backup your realm:

```bash
docker exec keycloak /opt/keycloak/bin/kc.sh export --dir /tmp --realm demo-realm
docker cp keycloak:/tmp/demo-realm-realm.json ./infra/realm/
```

To restore:

```bash
docker cp ./infra/realm/demo-realm-realm.json keycloak:/tmp/
docker exec keycloak /opt/keycloak/bin/kc.sh import --dir /tmp
```

## Automated Setup with Docker Compose

Example `docker-compose.yml`:

```yaml
version: '3.8'
services:
  keycloak:
    image: quay.io/keycloak/keycloak:26.1
    ports:
      - "8080:8080"
    environment:
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: admin
    command: start-dev
```

## API Diagnostics Endpoint

Add a simple endpoint in your API to echo back claims for debugging:

```csharp
[ApiController]
[Route("/diagnostics")]
public class DiagnosticsController : ControllerBase
{
  [HttpGet]
  public IActionResult Get() => new JsonResult(User.Claims.Select(c => new { c.Type, c.Value }));
}
```

## OIDC Discovery Endpoint

Keycloak exposes a discovery endpoint at:

```text
http://localhost:8080/realms/demo-realm/.well-known/openid-configuration
```

Use this for integration troubleshooting and to verify endpoints, supported scopes, and keys.

## References

- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [Keycloak Docker Guide](https://www.keycloak.org/server/containers)
- [OIDC Discovery Spec](https://openid.net/specs/openid-connect-discovery-1_0.html)

---

*Note: Keycloak UI and features may change between versions. Always pin your Docker image and check for breaking changes when upgrading.*

## Troubleshooting matrix

| Symptom | Likely cause | Where to look | Fix |
| --- | --- | --- | --- |
| Keycloak says `HTTPS required` | Realm SSL requirement too strict | realm settings | Set `sslRequired=none` for local HTTP only |
| Login redirects fail | Redirect URI mismatch | client settings | Add the exact callback URL |
| Browser app blocked by CORS/web origin | Missing web origin | client settings | Add frontend origin to web origins |
| Token missing `department` | Mapper not configured or scope not attached | client scope mappers, token payload | Fix mapper and attach scope |
| API returns `401` | Audience or issuer mismatch | token payload, API diagnostics | Add audience mapper or fix issuer config |
| `Invalid scopes` in Keycloak | Frontend requested unsupported scopes | client scopes and login request | Request only `openid` explicitly |

## Tips and tricks

- Use one realm export file for local demo reproducibility.
- Put custom claim logic in named client scopes rather than burying everything directly in the client config.
- Keep demo users simple and intentionally different so success and failure cases are obvious.
- Add one finance-type user and one non-finance user if you want to demonstrate authorization.
- Keep your SPA explicit scope request small, usually just `openid`, and use client scopes to supply the rest.
- Test tokens in both places:
  - browser-side decoded token panel
  - backend diagnostics endpoint
- Keep a short checklist for every new app:
  - realm
  - client
  - redirect URIs
  - web origins
  - logout URIs
  - audience
  - client scopes
  - protocol mappers
  - test users

## Minimum secure production changes

Before using the same pattern in production:

- Require HTTPS
- Do not leave realm SSL at `none`
- Use production-grade admin credentials and secret handling
- Review session timeout, access-token lifetime, and refresh-token lifetime
- Separate local, test, and production realms or at least clients
- Restrict redirect URIs and web origins tightly
- Avoid overly broad custom claims

## Quick validation checklist

- Realm exists and is enabled
- SPA client exists
- SPA redirect URIs are correct
- SPA web origins are correct
- PKCE is enabled
- API audience/client exists
- Audience mapper is active
- Custom claim mapper is active
- Test users can log in
- Access token contains expected `iss`, `aud`, and custom claims

## Related reading

- `docs/react-keycloak-integration.md`
- `docs/dotnet-api-keycloak-integration.md`
- `docs/authentication-authorization-flow.md`
- `docs/glossary.md`
