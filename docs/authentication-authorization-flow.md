# Authentication and Authorization Flow

This document explains the full flow used by this demo, including what happens in the browser, in Keycloak, and in the .NET APIs.

## Tested stack

- Keycloak 26.1
- React 18 + TypeScript + Vite 5
- ASP.NET Core APIs on .NET 10 preview

## Overview

There are three distinct responsibilities in this demo:

- Authentication: proving who the user is
- Token issuance: Keycloak issuing signed tokens after successful login
- Authorization: the APIs deciding what the authenticated user is allowed to do

The React app does not validate credentials itself. Keycloak handles the login form and authentication. The APIs do not trust the React app. They validate the signed bearer token directly.

## Stage 1: User starts unauthenticated

When the user first opens the React application:

1. The app checks whether it already has a valid session in browser storage.
1. If no valid session is found, the UI remains signed out.
1. The user clicks `Sign in with Keycloak`.

At this point, no API call is made yet. The app is only preparing to start the OpenID Connect login flow.

## Stage 2: React prepares the authorization request

Before redirecting the browser to Keycloak, the React app creates:

1. A `state` value

- Used to protect against request forgery and callback confusion

1. A PKCE code verifier

- A random secret generated in the browser

1. A PKCE code challenge

- A SHA-256 based derived value sent to Keycloak

The React app stores the `state` and code verifier in session storage, then redirects the browser to Keycloak’s authorization endpoint.

The authorization request includes:

- `client_id`
- `redirect_uri`
- `response_type=code`
- `scope=openid`
- `state`
- `code_challenge`
- `code_challenge_method=S256`

## Stage 3: Keycloak authenticates the user

Once redirected, the browser is no longer interacting with the React app. The user is now on the Keycloak-hosted login page.

Keycloak performs authentication:

1. The user enters username and password
2. Keycloak verifies those credentials against the realm
3. If valid, Keycloak creates a login session
4. Keycloak prepares an authorization code

At this stage:

- The React app never sees the password
- Keycloak is responsible for proving the user identity
- No access token is sent to the React app yet

## Stage 4: Keycloak redirects back with an authorization code

After successful login, Keycloak redirects the browser back to the React callback URL.

The callback contains:

- `code`
- `state`

The authorization code is short-lived and single use.

The React app:

1. Reads the `code` and `state`
1. Verifies the returned `state` matches the stored value
1. Removes the callback URL from browser history
1. Sends the code to the token endpoint

If the state check fails, the login flow is rejected.

## Stage 5: React exchanges the code for tokens

The React app sends a POST request to Keycloak’s token endpoint with:

- `grant_type=authorization_code`
- `client_id`
- `code`
- `redirect_uri`
- `code_verifier`

Keycloak verifies:

1. The authorization code is valid
2. The code has not already been used
3. The redirect URI matches
4. The PKCE verifier matches the original challenge

If everything matches, Keycloak returns tokens, including:

- Access token
- ID token
- Optionally refresh token

These tokens are signed by Keycloak.

## Stage 6: What the tokens mean

### ID token

The ID token is mainly about identity.

It tells the React app things like:

- who the user is
- the username
- profile and email style identity claims

### Access token

The access token is mainly for APIs.

It tells backend services:

- who the caller is
- who issued the token
- who the token is meant for
- what claims and roles are associated with the user

In this demo, the access token includes:

- issuer
- audience
- username
- custom `department` claim
- Keycloak realm roles

## Stage 7: React stores the session and calls APIs

After a successful token exchange:

1. The React app stores the token session in browser session storage

1. The user is treated as signed in

1. When the user clicks an API button, the React app sends:

- `Authorization: Bearer <access_token>`

The same access token is sent to both .NET API projects.

This is an important point:

- The frontend authenticates once with Keycloak
- One access token can then be used across multiple backend services if they accept the same issuer and audience

## Stage 8: The API authenticates the bearer token

When a request reaches either .NET API:

1. ASP.NET Core JWT bearer authentication reads the bearer token
1. It loads Keycloak metadata and signing keys from the configured authority
1. It validates the token signature using Keycloak public keys
1. It validates the issuer
1. It validates the audience
1. It validates token lifetime

If any of those checks fail, the API returns `401 Unauthorized`.

Examples of authentication failure:

- tampered token payload
- wrong issuer
- wrong audience
- expired token
- malformed token
- missing token

This is the point where the API proves:

- the token really came from Keycloak
- the token was not modified after Keycloak signed it

## Stage 9: The API normalizes Keycloak claims

After authentication succeeds, the API can transform claims into a shape the application expects.

In this demo:

1. Keycloak realm roles may appear under `realm_access.roles`
1. The API maps them into standard ASP.NET role claims

This step does not authenticate the token. Authentication already happened. This step only prepares the claims for application use.

## Stage 10: The API authorizes the user

After the token is accepted, authorization rules are applied.

This is different from authentication.

Authentication answers:

- Is this a real valid token from Keycloak?

Authorization answers:

- Is this authenticated user allowed to do this specific action?

Examples in this demo:

- `/api/demo/protected`
  - Requires any valid authenticated user
- `/api/demo/claims-protected`
  - Requires `department=finance`
- `/api/reports/summary`
  - Requires any valid authenticated user
- `/api/reports/finance`
  - Requires `department=finance`

If authentication succeeds but the policy does not match, the API returns `403 Forbidden`.

## Stage 11: Diagnostic behavior

The primary API includes `/api/auth/diagnostics`.

That endpoint helps show:

- configured authority
- configured public issuer
- configured audience
- observed token issuer
- observed token audience
- current authenticated username

This is useful when debugging:

- issuer mismatch
- audience mismatch
- container hostname versus browser hostname problems

## Stage 12: Logout

When the user clicks logout:

1. The React app clears its local session
1. The browser is redirected to Keycloak’s logout endpoint
1. Keycloak clears the identity provider session
1. The browser is redirected back to the frontend

After logout:

- the local session is gone
- protected API calls should fail again until the user logs in

## Authentication vs Authorization summary

### Authentication

Handled primarily by Keycloak and then validated by the APIs.

Questions answered:

- Did the user successfully log in?
- Did Keycloak issue this token?
- Is the token signed correctly?
- Is the token still valid?

### Authorization

Handled by API policies after authentication succeeds.

Questions answered:

- Is this user allowed to access this endpoint?
- Does the token contain the required claim or role?

## Common failure points

### Login button does nothing

- Browser-side discovery call failed
- Redirect logic was blocked
- JavaScript error occurred before redirect

### Keycloak says HTTPS required

- Realm SSL settings are too strict for local HTTP development

### Token exchange fails with `invalid_grant`

- Authorization code was reused
- Callback logic ran twice
- Redirect URI mismatch

### API returns `401`

- Token signature validation failed
- Issuer mismatch
- Audience mismatch
- Token expired

### API returns `403`

- Authentication succeeded
- The user lacks the required claim or role

## Practical mental model

The shortest correct way to think about this demo is:

1. React sends the user to Keycloak
1. Keycloak authenticates the user
1. React exchanges the returned code for signed tokens
1. React sends the access token to the APIs
1. The APIs validate the token using Keycloak public keys
1. The APIs apply authorization rules based on claims

That is the complete authentication and authorization chain used in this project.

## Related reading

- `docs/react-keycloak-integration.md`
- `docs/dotnet-api-keycloak-integration.md`
- `docs/keycloak-setup-guide.md`
- `docs/glossary.md`
