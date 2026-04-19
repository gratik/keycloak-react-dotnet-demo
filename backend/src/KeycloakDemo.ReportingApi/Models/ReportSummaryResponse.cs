namespace KeycloakDemo.ReportingApi.Models;

public sealed record ReportSummaryResponse(
    string ReportName,
    string Username,
    string? Department,
    IReadOnlyCollection<string> AvailableSections);
