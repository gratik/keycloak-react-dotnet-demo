namespace KeycloakDemo.Api.Models;

/// <summary>
/// Represents diagnostic information about the authentication context and token.
/// </summary>
/// <param name="Authority">The configured authority (issuer URL).</param>
/// <param name="PublicIssuer">The expected public issuer.</param>
/// <param name="Audience">The expected audience.</param>
/// <param name="RequireHttpsMetadata">Whether HTTPS metadata is required.</param>
/// <param name="AuthenticationType">The authentication type used.</param>
/// <param name="TokenIssuer">The issuer from the token.</param>
/// <param name="TokenAudience">The audience from the token.</param>
/// <param name="Username">The preferred username from the token.</param>
public sealed record AuthDiagnosticsResponse(
    string Authority,
    string PublicIssuer,
    string Audience,
    bool RequireHttpsMetadata,
    string AuthenticationType,
    string? TokenIssuer,
    string? TokenAudience,
    string Username);
