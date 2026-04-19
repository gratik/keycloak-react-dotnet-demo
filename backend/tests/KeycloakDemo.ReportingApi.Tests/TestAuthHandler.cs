using System.Security.Claims;
using System.Text.Encodings.Web;
using KeycloakDemo.ReportingApi.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace KeycloakDemo.ReportingApi.Tests;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Demo-User", out var usernameValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, usernameValues.ToString())
            };

        if (Request.Headers.TryGetValue("X-Demo-Department", out var departmentValues))
        {
            claims.Add(new Claim(JwtClaimTypes.Department, departmentValues.ToString()));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
        /// <summary>
        /// Test authentication handler that always authenticates as a test user for integration tests in the Reporting API.
        /// </summary>
        public const string AuthenticationScheme = "Test";

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAuthHandler"/> class.
        /// </summary>
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        /// <summary>
        /// Handles authentication by always returning a test user principal.
        /// </summary>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Always authenticate the user as a test user for integration testing.
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim("department", "finance"),
                new Claim("roles", "finance-admin")
            };
            var identity = new ClaimsIdentity(claims, AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, AuthenticationScheme);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
}
