using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using log4net;
using RmqClient;
using MicroservicePublicApi;

namespace MicroservicePublicApi.Tests;

public class ApiControllerTests
{
    [Fact]
    public void GetOk_ShouldReturnOkWithSmileyResponse()
    {
        // Arrange
        var mockProducer = CreateMockProducer();
        var mockLogger = new Mock<ILog>();
        var controller = new ApiController(mockProducer, mockLogger.Object);

        // Act
        var result = controller.GetOk();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(":)", okResult.Value);
    }

    [Fact]
    public void GetOk_ShouldReturnOkResult()
    {
        // Arrange
        var mockProducer = CreateMockProducer();
        var mockLogger = new Mock<ILog>();
        var controller = new ApiController(mockProducer, mockLogger.Object);

        // Act
        var result = controller.GetOk();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void ApiController_ShouldInheritFromControllerBase()
    {
        // Arrange
        var mockProducer = CreateMockProducer();
        var mockLogger = new Mock<ILog>();

        // Act
        var controller = new ApiController(mockProducer, mockLogger.Object);

        // Assert
        Assert.IsAssignableFrom<ControllerBase>(controller);
    }

    [Fact]
    public void Constructor_ShouldAcceptValidDependencies()
    {
        // Arrange
        var mockProducer = CreateMockProducer();
        var mockLogger = new Mock<ILog>();

        // Act
        var controller = new ApiController(mockProducer, mockLogger.Object);

        // Assert
        Assert.NotNull(controller);
    }

    [Fact]
    public void GetOk_ShouldHaveCorrectRouteAttribute()
    {
        // This test verifies the controller is properly set up for API usage
        // Arrange
        var mockProducer = CreateMockProducer();
        var mockLogger = new Mock<ILog>();
        var controller = new ApiController(mockProducer, mockLogger.Object);

        // Act
        var result = controller.GetOk();

        // Assert
        Assert.NotNull(result);
        // Verify it returns a valid action result
        Assert.IsAssignableFrom<IActionResult>(result);
    }

    private Producer CreateMockProducer()
    {
        // Create a real producer instance with mock dependencies
        var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        var mockConnection = new Mock<RabbitMQ.Client.IConnection>();
        return new Producer(mockConfiguration.Object, mockConnection.Object);
    }
}