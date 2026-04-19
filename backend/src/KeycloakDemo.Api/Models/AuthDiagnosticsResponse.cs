namespace KeycloakDemo.Api.Models;

public sealed record AuthDiagnosticsResponse(
    string Authority,
    string PublicIssuer,
    string Audience,
    bool RequireHttpsMetadata,
    string AuthenticationType,
    string? TokenIssuer,
    string? TokenAudience,
    string Username);
