using System.Security.Claims;
using KeycloakDemo.ReportingApi.Auth;
using KeycloakDemo.ReportingApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeycloakDemo.ReportingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Provides reporting endpoints with different authorization requirements.
/// </summary>
public sealed class ReportsController : ControllerBase
{
    /// <summary>
    /// Returns a summary report for any authenticated user.
    /// </summary>
    /// <returns>ReportSummaryResponse with user and section info.</returns>
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

    /// <summary>
    /// Returns a finance-only report for users meeting the finance department policy.
    /// </summary>
    /// <returns>ReportSummaryResponse with user and section info.</returns>
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
