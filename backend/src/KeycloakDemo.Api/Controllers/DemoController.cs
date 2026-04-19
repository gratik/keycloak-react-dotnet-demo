using System.Security.Claims;
using KeycloakDemo.Api.Auth;
using KeycloakDemo.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeycloakDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DemoController : ControllerBase
{
    [HttpGet("protected")]
    [Authorize]
    public ActionResult<DemoResponse> Protected()
    {
        return Ok(BuildResponse("The authenticated endpoint accepted the bearer token."));
    }

    [HttpGet("claims-protected")]
    [Authorize(Policy = AuthorizationPolicies.FinanceDepartment)]
    public ActionResult<DemoResponse> ClaimsProtected()
    {
        return Ok(BuildResponse("The finance-only policy accepted the Keycloak department claim."));
    }

    private DemoResponse BuildResponse(string message)
    {
        return new DemoResponse(
            Message: message,
            Username: User.FindFirstValue(JwtClaimTypes.PreferredUsername) ?? string.Empty,
            Department: User.FindFirstValue(JwtClaimTypes.Department),
            Roles: User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray());
    }
}
