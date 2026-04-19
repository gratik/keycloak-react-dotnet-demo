using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace KeycloakDemo.Api.Auth;

/// <summary>
/// Transforms Keycloak realm roles into standard ASP.NET role claims for authorization.
/// </summary>
public sealed class KeycloakClaimsTransformation : IClaimsTransformation
{
    /// <summary>
    /// Adds Keycloak realm roles as role claims to the principal for ASP.NET authorization.
    /// </summary>
    /// <param name="principal">The original ClaimsPrincipal.</param>
    /// <returns>ClaimsPrincipal with additional role claims if present.</returns>
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        var realmAccessJson = principal.FindFirst("realm_access")?.Value;
        if (string.IsNullOrWhiteSpace(realmAccessJson))
        {
            return Task.FromResult(principal);
        }

        try
        {
            using var document = JsonDocument.Parse(realmAccessJson);
            if (!document.RootElement.TryGetProperty("roles", out var rolesElement) ||
                rolesElement.ValueKind != JsonValueKind.Array)
            {
                return Task.FromResult(principal);
            }

            foreach (var role in rolesElement.EnumerateArray())
            {
                var roleName = role.GetString();
                if (string.IsNullOrWhiteSpace(roleName))
                {
                    continue;
                }

                if (!identity.HasClaim(ClaimTypes.Role, roleName))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                }
            }
        }
        catch (JsonException)
        {
            return Task.FromResult(principal);
        }

        return Task.FromResult(principal);
    }
}
