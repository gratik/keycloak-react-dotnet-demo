using System.Security.Claims;
using KeycloakDemo.ReportingApi.Auth;
using KeycloakDemo.ReportingApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeycloakDemo.ReportingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReportsController : ControllerBase
{
    [HttpGet("summary")]
    [Authorize]
    public ActionResult<ReportSummaryResponse> Summary()
    {
        return Ok(new ReportSummaryResponse(
            ReportName: "Cross-service report summary",
            Username: User.FindFirstValue(JwtClaimTypes.PreferredUsername) ?? string.Empty,
            Department: User.FindFirstValue(JwtClaimTypes.Department),
            AvailableSections: ["sales-overview", "team-status"]));
    }

    [HttpGet("finance")]
    [Authorize(Policy = AuthorizationPolicies.FinanceDepartment)]
    public ActionResult<ReportSummaryResponse> Finance()
    {
        return Ok(new ReportSummaryResponse(
            ReportName: "Finance-only report",
            Username: User.FindFirstValue(JwtClaimTypes.PreferredUsername) ?? string.Empty,
            Department: User.FindFirstValue(JwtClaimTypes.Department),
            AvailableSections: ["budget", "forecast", "variance"]));
    }
}
