export const authConfig = {
  keycloakUrl: import.meta.env.VITE_KEYCLOAK_URL,
  realm: import.meta.env.VITE_KEYCLOAK_REALM,
  clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID,
  redirectUri: import.meta.env.VITE_KEYCLOAK_REDIRECT_URI,
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL,
  reportingApiBaseUrl: import.meta.env.VITE_REPORTING_API_BASE_URL
};

export function validateConfig(): void {
  const missing = Object.entries(authConfig)
    .filter(([, value]) => !value)
    .map(([key]) => key);

  if (missing.length > 0) {
    throw new Error(`Missing frontend configuration: ${missing.join(", ")}`);
  }
}
