using System.Security.Claims;
using KeycloakDemo.Api.Auth;
using KeycloakDemo.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeycloakDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Demonstrates protected API endpoints with different authorization requirements.
/// </summary>
public sealed class DemoController : ControllerBase
{
    /// <summary>
    /// Returns a response if the user is authenticated with a valid bearer token.
    /// </summary>
    /// <returns>DemoResponse with user and claim info.</returns>
    [HttpGet("protected")]
    [Authorize]
    public ActionResult<DemoResponse> Protected()
    {
        return Ok(BuildResponse("The authenticated endpoint accepted the bearer token."));
    }

    /// <summary>
    /// Returns a response only if the user meets the finance department policy (department=finance).
    /// </summary>
    /// <returns>DemoResponse with user and claim info.</returns>
    [HttpGet("claims-protected")]
    [Authorize(Policy = AuthorizationPolicies.FinanceDepartment)]
    public ActionResult<DemoResponse> ClaimsProtected()
    {
        return Ok(BuildResponse("The finance-only policy accepted the Keycloak department claim."));
    }

    /// <summary>
    /// Helper to build a DemoResponse from the current user claims.
    /// </summary>
    /// <param name="message">Message to include in the response.</param>
    /// <returns>DemoResponse with user and claim info.</returns>
    private DemoResponse BuildResponse(string message)
    {
        return new DemoResponse(
            Message: message,
            Username: User.FindFirstValue(JwtClaimTypes.PreferredUsername) ?? string.Empty,
            Department: User.FindFirstValue(JwtClaimTypes.Department),
            Roles: User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray());
    }
}
