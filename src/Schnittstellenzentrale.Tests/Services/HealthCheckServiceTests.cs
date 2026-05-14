using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Services;

public class HealthCheckServiceTests
{
    private static HealthCheckService CreateService(
        Mock<HttpMessageHandler> handlerMock,
        int cooldownSeconds = 60)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "HealthCheck:CooldownSeconds", cooldownSeconds.ToString() }
            })
            .Build();

        var httpClient = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var loggerMock = new Mock<ILogger<HealthCheckService>>();

        return new HealthCheckService(factoryMock.Object, config, loggerMock.Object);
    }

    private static Mock<HttpMessageHandler> CreateHandler(HttpStatusCode code)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(code));
        return handlerMock;
    }

    [Fact]
    public async Task CheckAsync_WithinCooldown_DoesNotSendRequest()
    {
        var handlerMock = CreateHandler(HttpStatusCode.OK);
        var service = CreateService(handlerMock, cooldownSeconds: 300);
        var app = new Core.Models.Application { Id = 1, BaseUrl = "http://localhost" };

        await service.CheckAsync(app);
        await service.CheckAsync(app);

        handlerMock.Protected().Verify("SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task CheckAsync_AfterCooldownExpired_SendsRequest()
    {
        var handlerMock = CreateHandler(HttpStatusCode.OK);
        var service = CreateService(handlerMock, cooldownSeconds: 0);
        var app = new Core.Models.Application { Id = 2, BaseUrl = "http://localhost" };

        await service.CheckAsync(app);
        await service.CheckAsync(app);

        handlerMock.Protected().Verify("SendAsync",
            Times.AtLeast(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task CheckAsync_UnreachableUrl_ReturnsFalse()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var service = CreateService(handlerMock, cooldownSeconds: 0);
        var app = new Core.Models.Application { Id = 3, BaseUrl = "http://unreachable.invalid" };

        var result = await service.CheckAsync(app);

        Assert.False(result);
    }
}
