using CompanyName.Sdk.Utils.Core;
using CompanyName.Sdk.Utils.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CompanyName.Sdk.Utils.UnitTests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddUtilsCoreServices_ServiceCollectionIsNull_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => ServiceCollectionExtensions.AddUtilsCoreServices(null!, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Where(ex => ex.ParamName == "services");
    }

    [Fact]
    public void AddUtilsCoreServices_ConfigurationIsNull_ThrowsArgumentNullException()
    {
        // Arrange & Act
        IServiceCollection services = new ServiceCollection();
        var act = () => services.AddUtilsCoreServices(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Where(ex => ex.ParamName == "configuration");
    }

    [Theory]
    [InlineData("LruCache:SizeLimit", "")]
    [InlineData("LruCache", "")]
    [InlineData("", "")]
    public void AddUtilsCoreServices_ConfigurationIsMissing_ThrowsOptionsValidationException(string key, string value)
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        var inMemorySettings = new Dictionary<string, string> {
            { key, value}
        };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();

        // Act
        var act = () => services.AddUtilsCoreServices(configuration);

        // Assert
        act.Should().Throw<OptionsValidationException>()
            .Where(ex => ex.Message.Equals("Please provide a valid value for 'LruCache:SizeLimit' in the configuration."));
    }

    [Fact]
    public void AddUtilsCoreServices_ConfigurationProvidedButIsInvalid_ThrowsOptionsValidationException()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string> {
            {"LruCache:SizeLimit", "0"},
        };
        IConfiguration configuration =
            new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();

        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddUtilsCoreServices(configuration);
            })
            .Build();

        // Act
        var act = () => ActivatorUtilities.CreateInstance<LruCache<string, string>>(host.Services);

        // Assert
        act.Should().Throw<OptionsValidationException>()
            .Where(ex => ex.Message.Contains("SizeLimit"));
    }
}
