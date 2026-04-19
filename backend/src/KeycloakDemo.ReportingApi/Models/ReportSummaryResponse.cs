namespace KeycloakDemo.ReportingApi.Models;

/// <summary>
/// Represents a summary report response with user and section info.
/// </summary>
/// <param name="ReportName">The name of the report.</param>
/// <param name="Username">The preferred username.</param>
/// <param name="Department">The user's department claim, if present.</param>
/// <param name="AvailableSections">Sections available in the report.</param>
public sealed record ReportSummaryResponse(
    string ReportName,
    string Username,
    string? Department,
    IReadOnlyCollection<string> AvailableSections);
