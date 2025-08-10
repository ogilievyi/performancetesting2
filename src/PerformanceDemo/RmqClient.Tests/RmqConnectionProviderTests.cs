using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using RmqClient;

namespace RmqClient.Tests;

public class RmqConnectionProviderTests
{
    [Fact]
    public void Constructor_ShouldCreateProviderWithConfiguration()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();

        // Act
        var provider = new RmqConnectionProvider(mockConfiguration.Object);

        // Assert
        Assert.NotNull(provider);
    }

    [Fact]
    public void Constructor_ShouldNotThrowWithValidConfiguration()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();

        // Act & Assert - Should not throw during construction
        var provider = new RmqConnectionProvider(mockConfiguration.Object);
        Assert.NotNull(provider);
    }

    [Fact]
    public void RmqConnectionProvider_ShouldBeInstantiable()
    {
        // This test validates the basic instantiation without complex mocking
        
        // Arrange
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var provider = new RmqConnectionProvider(configuration);

        // Assert
        Assert.NotNull(provider);
    }

    [Fact]
    public void Connection_WithEmptyConfiguration_ShouldThrowWhenAccessed()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var provider = new RmqConnectionProvider(configuration);

        // Act & Assert
        // When accessing Connection property with no connection string configured,
        // it should throw when trying to create the connection
        var exception = Record.Exception(() => _ = provider.Connection);
        
        // Should throw some kind of exception (either ArgumentNull or similar)
        Assert.NotNull(exception);
    }

    [Fact]
    public void Connection_WithNullConnectionString_ShouldThrowWhenAccessed()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            {"ConnectionStrings:rmq", null}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
        var provider = new RmqConnectionProvider(configuration);

        // Act & Assert
        var exception = Record.Exception(() => _ = provider.Connection);
        Assert.NotNull(exception);
    }
}