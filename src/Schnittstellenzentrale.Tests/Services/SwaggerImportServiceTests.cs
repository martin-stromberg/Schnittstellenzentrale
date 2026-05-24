using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>SwaggerImportServiceTests</summary>
public class SwaggerImportServiceTests
{
    private const string SwaggerWithGetPost = """
        {
          "openapi": "3.0.0",
          "info": { "title": "Test", "version": "1.0" },
          "paths": {
            "/items": {
              "get": { "operationId": "getItems", "responses": { "200": { "description": "OK" } } },
              "post": { "operationId": "createItem", "responses": { "200": { "description": "OK" } } }
            }
          }
        }
        """;

    private static SwaggerImportService CreateService(
        string swaggerJson,
        Mock<IEndpointRepository> repoMock)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(swaggerJson, Encoding.UTF8, "application/json")
            });

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(handlerMock.Object));

        return new SwaggerImportService(factoryMock.Object, repoMock.Object, NullLogger<SwaggerImportService>.Instance);
    }

    /// <summary>Import_NewSwaggerDefinition_ReturnsCorrectDiff</summary>
    [Fact]
    public async Task Import_NewSwaggerDefinition_ReturnsCorrectDiff()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var service = CreateService(SwaggerWithGetPost, repoMock);
        var app = new Core.Models.Application { Id = 1, InterfaceUrl = "http://localhost/swagger.json", InterfaceType = Core.Enums.InterfaceType.Rest, BaseUrl = "http://localhost" };

        var diff = await service.ImportAsync(app);

        Assert.Equal(2, diff.NewEndpoints.Count);
        Assert.Empty(diff.ChangedEndpoints);
        Assert.Empty(diff.RemovedEndpoints);
    }

    /// <summary>Import_ChangedSwaggerOperation_ReturnsChangedInDiff</summary>
    [Fact]
    public async Task Import_ChangedSwaggerOperation_ReturnsChangedInDiff()
    {
        var existing = new List<Core.Models.Endpoint>
        {
            new() { Id = 1, Name = "oldName", Method = Core.Enums.HttpMethod.GET, RelativePath = "/items", ApplicationId = 1 }
        };
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync(existing);
        var service = CreateService(SwaggerWithGetPost, repoMock);
        var app = new Core.Models.Application { Id = 1, InterfaceUrl = "http://localhost/swagger.json", InterfaceType = Core.Enums.InterfaceType.Rest, BaseUrl = "http://localhost" };

        var diff = await service.ImportAsync(app);

        Assert.Contains(diff.ChangedEndpoints, e => e.RelativePath == "/items" && e.Method == Core.Enums.HttpMethod.GET);
    }

    /// <summary>Import_RemovedSwaggerOperation_ReturnsRemovedInDiff</summary>
    [Fact]
    public async Task Import_RemovedSwaggerOperation_ReturnsRemovedInDiff()
    {
        var existing = new List<Core.Models.Endpoint>
        {
            new() { Id = 1, Name = "deleteItems", Method = Core.Enums.HttpMethod.DELETE, RelativePath = "/items", ApplicationId = 1 }
        };
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync(existing);
        var service = CreateService(SwaggerWithGetPost, repoMock);
        var app = new Core.Models.Application { Id = 1, InterfaceUrl = "http://localhost/swagger.json", InterfaceType = Core.Enums.InterfaceType.Rest, BaseUrl = "http://localhost" };

        var diff = await service.ImportAsync(app);

        Assert.Contains(diff.RemovedEndpoints, e => e.Method == Core.Enums.HttpMethod.DELETE && e.RelativePath == "/items");
    }
}
