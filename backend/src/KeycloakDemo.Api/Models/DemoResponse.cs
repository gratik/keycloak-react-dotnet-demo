namespace KeycloakDemo.Api.Models;

public sealed record DemoResponse(
    string Message,
    string Username,
    string? Department,
    IReadOnlyCollection<string> Roles);
