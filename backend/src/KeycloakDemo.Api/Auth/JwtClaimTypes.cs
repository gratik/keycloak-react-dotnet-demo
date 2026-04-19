namespace KeycloakDemo.Api.Auth;

/// <summary>
/// Contains JWT claim type constants for Keycloak integration.
/// </summary>
public static class JwtClaimTypes
{
    /// <summary>Preferred username claim type.</summary>
    public const string PreferredUsername = "preferred_username";
    /// <summary>Department claim type.</summary>
    public const string Department = "department";
    /// <summary>Subject (user id) claim type.</summary>
    public const string Subject = "sub";
    /// <summary>Realm role claim type.</summary>
    public const string RealmRole = "roles";
}
