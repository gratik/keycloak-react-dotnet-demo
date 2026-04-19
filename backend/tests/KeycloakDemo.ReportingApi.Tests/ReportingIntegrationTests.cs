namespace KeycloakDemo.ReportingApi.Tests;

public sealed class ReportingIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ReportingIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Summary_ReturnsUnauthorized_WhenRequestHasNoToken()
    {
        var response = await _client.GetAsync("/api/reports/summary");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Summary_ReturnsOk_WhenAuthenticated()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/reports/summary");
        request.Headers.Add("X-Demo-User", "alice");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Finance_ReturnsForbidden_ForNonFinanceUsers()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/reports/finance");
        request.Headers.Add("X-Demo-User", "bob");
        request.Headers.Add("X-Demo-Department", "sales");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

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
