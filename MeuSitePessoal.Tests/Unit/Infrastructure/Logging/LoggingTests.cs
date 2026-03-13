using MeuSitePessoal.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Infrastructure.Logging;

public class LoggingTests
{
    [Fact]
    public void AddCustomLogging_ShouldRegisterSerilogLogger()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddLogging(); // Ensure base logging services are registered

        // Act
        services.AddCustomLogging(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var logger = serviceProvider.GetService<ILogger<LoggingTests>>();
        Assert.NotNull(logger);
    }
}
