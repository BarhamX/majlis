using Majlis.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Majlis.Tests.Infrastructure;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddMajlisInfrastructure_WhenConnectionStringIsMissing_FailsFast()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddMajlisInfrastructure(configuration));

        Assert.Contains("Connection string 'MajlisDatabase' is required", exception.Message);
    }
}
