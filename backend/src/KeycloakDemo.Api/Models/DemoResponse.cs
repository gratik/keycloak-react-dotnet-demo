namespace KeycloakDemo.Api.Models;

/// <summary>
/// Represents a demo API response with user and claim info.
/// </summary>
/// <param name="Message">A message describing the result.</param>
/// <param name="Username">The preferred username.</param>
/// <param name="Department">The user's department claim, if present.</param>
/// <param name="Roles">The user's roles.</param>
public sealed record DemoResponse(
    string Message,
    string Username,
    string? Department,
    IReadOnlyCollection<string> Roles);
