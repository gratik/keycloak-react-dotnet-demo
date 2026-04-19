namespace KeycloakDemo.Api.Models;

public sealed record UserInfoResponse(
    string Subject,
    string Username,
    string? Department,
    IReadOnlyCollection<string> Roles,
    IReadOnlyDictionary<string, string[]> Claims);
