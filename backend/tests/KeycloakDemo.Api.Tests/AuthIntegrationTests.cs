namespace KeycloakDemo.Api.Tests;

/// <summary>
/// Integration tests for API authentication and authorization scenarios.
/// </summary>
public sealed class AuthIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Verifies that accessing a protected endpoint without a token returns 401 Unauthorized.
    /// </summary>
    [Fact]
    public async Task Protected_ReturnsUnauthorized_WhenRequestHasNoToken()
    {
        var response = await _client.GetAsync("/api/demo/protected");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verifies that an authenticated user can access the protected endpoint.
    /// </summary>
    [Fact]
    public async Task Protected_ReturnsOk_WhenAuthenticated()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/demo/protected");
        request.Headers.Add("X-Demo-User", "alice");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Verifies that an authenticated user can access the diagnostics endpoint.
    /// </summary>
    [Fact]
    public async Task Diagnostics_ReturnsOk_WhenAuthenticated()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/diagnostics");
        request.Headers.Add("X-Demo-User", "alice");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Verifies that a user without the required department claim receives 403 Forbidden.
    /// </summary>
    [Fact]
    public async Task ClaimsProtected_ReturnsForbidden_WhenDepartmentClaimIsMissing()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/demo/claims-protected");
        request.Headers.Add("X-Demo-User", "bob");
        request.Headers.Add("X-Demo-Department", "sales");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Verifies that a user with the correct department claim can access the finance-protected endpoint.
    /// </summary>
    [Fact]
    public async Task ClaimsProtected_ReturnsOk_WhenDepartmentClaimMatchesPolicy()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/demo/claims-protected");
        request.Headers.Add("X-Demo-User", "alice");
        request.Headers.Add("X-Demo-Department", "finance");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
