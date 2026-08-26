using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Majlis.Tests.Infrastructure;

public sealed class AuthenticationConfigurationTests
{
    [Fact]
    public void Production_WhenTestAuthenticationIsConfigured_FailsClosed()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Production");
                builder.UseSetting("Authentication:Mode", "Test");
                builder.UseSetting(
                    "ConnectionStrings:MajlisDatabase",
                    "Host=localhost;Database=not_used;Username=not_used;Password=not_used");
            });

        var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());

        Assert.Contains(
            "Test authentication is allowed only in Development and Testing",
            exception.ToString(),
            StringComparison.Ordinal);
    }
}
