using System.Security.Claims;
using KeycloakDemo.Api.Auth;
using KeycloakDemo.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;

namespace KeycloakDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Handles authentication-related endpoints for user info and diagnostics.
/// </summary>
public sealed class AuthController : ControllerBase
{
    private readonly KeycloakOptions _keycloakOptions;

    /// <summary>
    /// Initializes the AuthController with Keycloak options.
    /// </summary>
    /// <param name="keycloakOptions">Injected Keycloak options.</param>
    public AuthController(IOptions<KeycloakOptions> keycloakOptions)
    {
        _keycloakOptions = keycloakOptions.Value;
    }

    /// <summary>
    /// Returns information about the currently authenticated user, including claims and roles.
    /// </summary>
    /// <returns>UserInfoResponse with claims and roles.</returns>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<UserInfoResponse> Me()
    {
        var claims = User.Claims
            .GroupBy(claim => claim.Type)
            .ToDictionary(
                group => group.Key,
                group => group.Select(claim => claim.Value).ToArray());

        return Ok(new UserInfoResponse(
            Subject: User.FindFirstValue(JwtClaimTypes.Subject) ?? string.Empty,
            Username: User.FindFirstValue(JwtClaimTypes.PreferredUsername) ?? string.Empty,
            Department: User.FindFirstValue(JwtClaimTypes.Department),
            Roles: User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray(),
            Claims: claims));
    }

    /// <summary>
    /// Returns diagnostic information about the current authentication context and token.
    /// Useful for debugging Keycloak integration and token validation.
    /// </summary>
    /// <returns>AuthDiagnosticsResponse with authority, issuer, audience, and user info.</returns>
    [HttpGet("diagnostics")]
    [Authorize]
    public ActionResult<AuthDiagnosticsResponse> Diagnostics()
    {
        return Ok(new AuthDiagnosticsResponse(
            Authority: _keycloakOptions.Authority,
            PublicIssuer: _keycloakOptions.PublicIssuer ?? _keycloakOptions.Authority,
            Audience: _keycloakOptions.Audience,
            RequireHttpsMetadata: _keycloakOptions.RequireHttpsMetadata,
            AuthenticationType: User.Identity?.AuthenticationType ?? string.Empty,
            TokenIssuer: User.FindFirst("iss")?.Value,
            TokenAudience: User.FindFirst("aud")?.Value,
            Username: User.FindFirstValue(JwtClaimTypes.PreferredUsername) ?? string.Empty));
    }
}
