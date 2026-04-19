namespace KeycloakDemo.ReportingApi.Tests;

/// <summary>
/// Integration tests for reporting API authentication and authorization scenarios.
/// </summary>
public sealed class ReportingIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ReportingIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Verifies that accessing the summary report without a token returns 401 Unauthorized.
    /// </summary>
    [Fact]
    public async Task Summary_ReturnsUnauthorized_WhenRequestHasNoToken()
    {
        var response = await _client.GetAsync("/api/reports/summary");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verifies that an authenticated user can access the summary report.
    /// </summary>
    [Fact]
    public async Task Summary_ReturnsOk_WhenAuthenticated()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/reports/summary");
        request.Headers.Add("X-Demo-User", "alice");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Verifies that a non-finance user receives 403 Forbidden on the finance report.
    /// </summary>
    [Fact]
    public async Task Finance_ReturnsForbidden_ForNonFinanceUsers()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/reports/finance");
        request.Headers.Add("X-Demo-User", "bob");
        request.Headers.Add("X-Demo-Department", "sales");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Verifies that a finance user can access the finance-only report.
    /// </summary>
    [Fact]
    public async Task Finance_ReturnsOk_ForFinanceUsers()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/reports/finance");
        request.Headers.Add("X-Demo-User", "alice");
        request.Headers.Add("X-Demo-Department", "finance");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
