using System.ComponentModel.DataAnnotations;

namespace KeycloakDemo.Api.Auth;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    [Required]
    public string Authority { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    public string? PublicIssuer { get; init; }

    public bool RequireHttpsMetadata { get; init; } = false;
}
