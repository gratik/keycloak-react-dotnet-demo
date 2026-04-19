# Identity and Auth Glossary

This glossary explains the main terms used across the React, .NET API, and Keycloak guides.

## Access token

A token sent to APIs in the `Authorization: Bearer ...` header.

It is meant for backend authorization, not for proving login state to the frontend alone.

## Audience

A value in the token that identifies which API or service the token is intended for.

If the API expects audience `dotnet-api` and the token does not contain it, the API should reject the token.

## Authorization

The decision about what an authenticated user is allowed to do.

Examples:

- access any protected endpoint
- access only finance endpoints
- access only admin features

## Authorization Code Flow

An OAuth 2.0 / OpenID Connect flow where:

1. the browser is redirected to Keycloak
2. the user logs in there
3. Keycloak returns an authorization code
4. the app exchanges that code for tokens

This is the recommended browser flow when combined with PKCE.

## Bearer token

A token accepted by an API simply because the caller presents it.

Because of that, the API must validate the token very carefully.

## Claim

A piece of information inside a token.

Examples:

- `iss`
- `aud`
- `preferred_username`
- `department`

## Client

An application definition in Keycloak.

Examples:

- a React SPA client
- an API audience or bearer-only client

## Client scope

A reusable Keycloak grouping for token claims and protocol mappers.

Client scopes are often the cleanest way to add profile, email, audience, or custom claims.

## CORS

Cross-Origin Resource Sharing.

Browser security rules that control which frontend origins can call an API.

## ID token

A token focused on identity information for the client application.

It tells the frontend who the user is, but it is not the main token APIs should authorize against.

## Issuer

The authority that created the token.

In this demo it is Keycloak, for example:

`http://localhost:8080/realms/demo-realm`

## JWKS

JSON Web Key Set.

A published set of public keys used by APIs to verify token signatures.

## Keycloak realm

A logical boundary in Keycloak containing users, clients, roles, scopes, and settings.

You usually create one dedicated realm per application or demo.

## OIDC

OpenID Connect.

An identity layer on top of OAuth 2.0 used for login and identity tokens.

## PKCE

Proof Key for Code Exchange.

A security mechanism that protects authorization code flow in browser and public-client scenarios.

It uses:

- a code verifier
- a code challenge

## Protocol mapper

A Keycloak configuration object that controls how data becomes token claims.

Examples:

- adding `department` from a user attribute
- adding an API audience to the access token

## Public client

A client that cannot safely hold a secret, such as a browser SPA.

React apps are typically public clients.

## Realm role

A role defined at the realm level in Keycloak.

It can be added to users and then emitted in tokens.

## Refresh token

A token that can be used to obtain a new access token without making the user log in again.

Whether you use it in a SPA depends on your security model.

## Redirect URI

The URL Keycloak is allowed to redirect back to after login.

If this is wrong, login will fail.

## Role

A label that expresses a permission grouping.

Examples:

- `app-user`
- `finance-reader`
- `admin`

## Scope

A requested or granted set of access and identity permissions.

In this demo the frontend explicitly requests only `openid`, while additional claims come from Keycloak client scopes.

## Session storage

Browser storage scoped to the current browser tab or window session.

Useful for demo token storage because it avoids some persistence risks of long-lived local storage.

## Signature validation

The process an API uses to verify that a token was signed by the trusted identity provider and not modified afterward.

This is one of the most important security checks in the flow.

## State

A random value sent during login and returned during callback.

It protects the browser flow from request forgery and callback confusion.

## Token exchange

The step where the React app sends the authorization code and PKCE verifier to Keycloak’s token endpoint and receives tokens.

## Token lifetime

How long a token remains valid before expiration.

Expired tokens should be rejected by APIs.
