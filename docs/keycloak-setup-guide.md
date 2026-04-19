# Keycloak Setup Guide

This guide describes the practical steps for setting up Keycloak so browser apps and APIs can use it safely and predictably.

## Prerequisites

- Docker or another way to run Keycloak
- A plan for your realms, clients, users, roles, and claims
- Local URLs for your frontend and APIs

## Step-by-step setup

1. Start Keycloak.
   - For local work, a Docker setup is usually the fastest option
   - Create an admin user for the local environment

2. Create a dedicated realm for your application or demo.
   - Do not reuse the master realm for application traffic
   - Keep realm names stable across environments when possible

3. Configure SSL requirements for the environment.
   - For local HTTP testing only, set the realm SSL requirement to `none`
   - For production, require HTTPS

4. Create the frontend client.
   - Use OpenID Connect
   - Make it a public client for a browser SPA
   - Enable standard flow
   - Enable PKCE with `S256`
   - Configure redirect URIs, web origins, and logout redirect URIs

5. Create API client or audience configuration.
   - For simple setups, a bearer-only API client is fine
   - Make sure access tokens include the audience your APIs expect

6. Create client scopes and mappers.
   - Add default scopes such as profile and email as needed
   - Add protocol mappers for any custom claims your apps need
   - Example custom claims:
     - department
     - tenant
     - application role markers

7. Create roles.
   - Add realm roles or client roles depending on your authorization design
   - Keep role naming consistent and readable

8. Create users for testing.
   - Add passwords
   - Add roles
   - Add user attributes used by custom claim mappers

9. Export and version your realm configuration.
   - Store a realm export or bootstrap configuration in source control for reproducible environments
   - Keep environment-specific secrets out of the export where possible

10. Test the full login and token flow.
   - Browser app redirects to Keycloak
   - User signs in
   - Keycloak returns an authorization code
   - App exchanges the code for tokens
   - API accepts the token and enforces authorization

## Gotchas

- Forgetting redirect URIs or web origins is one of the fastest ways to break login.
- If Keycloak shows `HTTPS required`, your realm SSL setting is too strict for local HTTP.
- If the browser app works but APIs return `401`, the Keycloak issuer or audience setup may not match backend validation.
- If the token is missing the claim you expected, check protocol mappers and whether the scope is actually applied.
- If Keycloak says `Invalid scopes`, the frontend may be requesting scopes that are not configured for the client.
- Docker hostnames and browser URLs are not the same thing. Plan for public versus internal addresses.

## Tips and tricks

- Use one realm export file for local demo reproducibility.
- Put custom claim logic in named client scopes rather than burying everything directly in the client config.
- Keep demo users simple and intentionally different so success and failure cases are obvious.
- Add one finance-type user and one non-finance user if you want to demonstrate authorization.
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
