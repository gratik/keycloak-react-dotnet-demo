namespace KeycloakDemo.ReportingApi.Auth;

/// <summary>
/// Contains authorization policy names used in the Reporting API.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Policy requiring the user to be in the finance department.
    /// </summary>
    public const string FinanceDepartment = "FinanceDepartment";
}
