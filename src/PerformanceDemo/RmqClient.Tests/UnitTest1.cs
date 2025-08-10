using Xunit;
using Moq;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;

namespace RmqClient.Tests;

public class ProducerTests
{
    [Fact]
    public void Constructor_ShouldCreateProducerWithValidDependencies()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        var mockConnection = new Mock<IConnection>();

        // Act
        var producer = new Producer(mockConfiguration.Object, mockConnection.Object);

        // Assert
        Assert.NotNull(producer);
    }

    [Fact]
    public void Publish_WithNullStringMessage_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IModel>();
        
        mockConnection.Setup(c => c.CreateModel()).Returns(mockChannel.Object);
        mockChannel.Setup(c => c.IsOpen).Returns(true);
        mockConfiguration.Setup(c => c["RoutingKey"]).Returns("test-routing-key");
        mockConfiguration.Setup(c => c["Exchange"]).Returns("test-exchange");

        var producer = new Producer(mockConfiguration.Object, mockConnection.Object);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => producer.Publish((string)null));
    }

    [Fact]
    public void Publish_WithEmptyStringMessage_ShouldNotThrow()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IModel>();
        
        mockConnection.Setup(c => c.CreateModel()).Returns(mockChannel.Object);
        mockChannel.Setup(c => c.IsOpen).Returns(true);
        mockConfiguration.Setup(c => c["RoutingKey"]).Returns("test-routing-key");
        mockConfiguration.Setup(c => c["Exchange"]).Returns("test-exchange");

        var producer = new Producer(mockConfiguration.Object, mockConnection.Object);

        // Act & Assert - Should not throw exception
        producer.Publish("");
    }

    [Fact]
    public void Publish_WithValidObject_ShouldNotThrow()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IModel>();
        
        mockConnection.Setup(c => c.CreateModel()).Returns(mockChannel.Object);
        mockChannel.Setup(c => c.IsOpen).Returns(true);
        mockConfiguration.Setup(c => c["RoutingKey"]).Returns("test-routing-key");
        mockConfiguration.Setup(c => c["Exchange"]).Returns("test-exchange");

        var producer = new Producer(mockConfiguration.Object, mockConnection.Object);
        var testObject = new { Name = "Test", Value = 123 };

        // Act & Assert - Should not throw exception when properly configured
        producer.Publish(testObject);
    }
}

public class BaseRmqExecutorTests
{
    private class TestBaseRmqExecutor : BaseRmqExecutor
    {
        public TestBaseRmqExecutor(IConnection connection) : base(connection) { }
        
        public IModel TestChannel => Channel;
    }

    [Fact]
    public void Channel_ShouldCreateModelFromConnection()
    {
        // Arrange
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IModel>();
        
        mockConnection.Setup(c => c.CreateModel()).Returns(mockChannel.Object);
        mockChannel.Setup(c => c.IsOpen).Returns(true);

        var executor = new TestBaseRmqExecutor(mockConnection.Object);

        // Act
        var channel = executor.TestChannel;

        // Assert
        Assert.NotNull(channel);
        mockConnection.Verify(c => c.CreateModel(), Times.Once);
    }

    [Fact]
    public void Channel_WhenChannelIsClosed_ShouldCreateNewChannel()
    {
        // Arrange
        var mockConnection = new Mock<IConnection>();
        var mockChannel1 = new Mock<IModel>();
        var mockChannel2 = new Mock<IModel>();
        
        mockChannel1.Setup(c => c.IsOpen).Returns(false);
        mockChannel2.Setup(c => c.IsOpen).Returns(true);
        
        mockConnection.SetupSequence(c => c.CreateModel())
            .Returns(mockChannel1.Object)
            .Returns(mockChannel2.Object);

        var executor = new TestBaseRmqExecutor(mockConnection.Object);

        // Act
        var channel1 = executor.TestChannel;
        var channel2 = executor.TestChannel;

        // Assert
        Assert.NotNull(channel1);
        Assert.NotNull(channel2);
        mockConnection.Verify(c => c.CreateModel(), Times.Exactly(2));
    }

    [Fact]
    public void Dispose_ShouldDisposeChannel()
    {
        // Arrange
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IModel>();
        
        mockConnection.Setup(c => c.CreateModel()).Returns(mockChannel.Object);
        mockChannel.Setup(c => c.IsOpen).Returns(true);

        var executor = new TestBaseRmqExecutor(mockConnection.Object);
        
        // Access channel to create it
        var _ = executor.TestChannel;

        // Act
        executor.Dispose();

        // Assert
        mockChannel.Verify(c => c.Dispose(), Times.Once);
    }

    [Fact]
    public void Dispose_WhenChannelIsNull_ShouldNotThrow()
    {
        // Arrange
        var mockConnection = new Mock<IConnection>();
        var executor = new TestBaseRmqExecutor(mockConnection.Object);

        // Act & Assert - Should not throw when channel is null
        executor.Dispose();
    }
}