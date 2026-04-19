# React to Keycloak Integration Guide

This guide describes the minimum setup needed for any React application to authenticate users with Keycloak using Authorization Code Flow with PKCE.

## Prerequisites

- A running Keycloak instance
- A realm created for your application
- A React application running in the browser
- A redirect URL for local development, for example `http://localhost:5173/auth/callback`

## Step-by-step setup

1. Create a public client in Keycloak.
   - Set `Client ID` to your app name, for example `react-spa`
   - Keep the client type as OpenID Connect
   - Configure the client as a public browser client
   - Enable standard authorization code flow
   - Disable direct access grants for normal SPA usage

2. Configure redirect URLs and web origins.
   - Add your callback URL, for example `http://localhost:5173/*`
   - Add your frontend origin, for example `http://localhost:5173`
   - Add logout redirect URLs if you want Keycloak logout to return to the app

3. Enable PKCE.
   - Require or configure `S256` as the PKCE code challenge method
   - Do not use implicit flow for new React apps

4. Decide which claims the frontend needs.
   - Keep the explicit scope request minimal, usually `openid`
   - Add `profile`, `email`, roles, and custom claims through Keycloak default client scopes where possible

5. In React, keep these values in environment config.
   - Keycloak base URL
   - Realm name
   - Client ID
   - Redirect URI
   - API base URL if the app calls a backend

6. On login, redirect the browser to Keycloak.
   - Generate a PKCE verifier and challenge
   - Generate a random state value
   - Redirect to the Keycloak authorization endpoint with:
     - `client_id`
     - `redirect_uri`
     - `response_type=code`
     - `scope=openid`
     - `state`
     - `code_challenge`
     - `code_challenge_method=S256`

7. Handle the callback route.
   - Read `code` and `state` from the URL
   - Verify the returned `state` matches the saved value
   - Exchange the authorization code for tokens at the token endpoint

8. Store tokens carefully.
   - Prefer short-lived in-memory or session-scoped storage for demos
   - Avoid long-term local storage unless you accept the XSS tradeoff
   - Clear tokens on logout and when they expire

9. Add the access token to API requests.
   - Send `Authorization: Bearer <access_token>`
   - Handle `401` and `403` cleanly in the UI

10. Implement logout.
   - Clear the local session
   - Redirect to Keycloak logout with a post-logout redirect URI
   - Include `id_token_hint` when available

## Gotchas

- Do not let the React app collect username and password directly. Use Keycloak’s hosted login page.
- If Keycloak says `Invalid scopes`, reduce the explicit scope request to `openid` and move extra claims into default client scopes.
- If the login button seems dead, avoid relying on browser-time OIDC discovery if you do not need it. Building the auth URL directly from the configured realm is often simpler.
- Redirect URI mismatches are one of the most common causes of failed login.
- If logout works inconsistently, check the configured post-logout redirect URIs in Keycloak.
- If the user logs in successfully but the API returns `401`, the frontend may be fine and the backend token validation may be wrong.

## Tips and tricks

- Show decoded token claims in a debug panel during development.
- Keep auth logic in one module or provider instead of scattering it across components.
- Store PKCE verifier and state in `sessionStorage` for a simple SPA flow.
- Use separate buttons or views for:
  - login state
  - user profile
  - protected API calls
  - authorization failure scenarios
- For production, consider whether a backend-for-frontend pattern is better than a pure browser token model.
