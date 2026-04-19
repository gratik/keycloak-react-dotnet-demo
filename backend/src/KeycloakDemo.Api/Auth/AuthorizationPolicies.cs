namespace KeycloakDemo.Api.Auth;

/// <summary>
/// Contains authorization policy names used in the API.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Policy requiring the user to be in the finance department.
    /// </summary>
    public const string FinanceDepartment = "FinanceDepartment";
}
