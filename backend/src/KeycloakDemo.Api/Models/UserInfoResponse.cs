namespace KeycloakDemo.Api.Models;

/// <summary>
/// Represents information about the authenticated user, including claims and roles.
/// </summary>
/// <param name="Subject">The subject (user id) from the token.</param>
/// <param name="Username">The preferred username.</param>
/// <param name="Department">The user's department claim, if present.</param>
/// <param name="Roles">The user's roles.</param>
/// <param name="Claims">All claims grouped by type.</param>
public sealed record UserInfoResponse(
    string Subject,
    string Username,
    string? Department,
    IReadOnlyCollection<string> Roles,
    IReadOnlyDictionary<string, string[]> Claims);
