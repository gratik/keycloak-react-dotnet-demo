using System.ComponentModel.DataAnnotations;

namespace KeycloakDemo.ReportingApi.Auth;

/// <summary>
/// Configuration options for Keycloak integration in the Reporting API.
/// </summary>
public sealed class KeycloakOptions
{
    /// <summary>Configuration section name for Keycloak options.</summary>
    public const string SectionName = "Keycloak";

    /// <summary>Authority (issuer URL) for Keycloak.</summary>
    [Required]
    public string Authority { get; init; } = string.Empty;

    /// <summary>Expected audience for tokens.</summary>
    [Required]
    public string Audience { get; init; } = string.Empty;

    /// <summary>Optional public issuer for validation.</summary>
    public string? PublicIssuer { get; init; }

    /// <summary>Whether HTTPS metadata is required.</summary>
    public bool RequireHttpsMetadata { get; init; }
}
