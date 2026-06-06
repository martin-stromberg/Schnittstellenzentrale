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

/// <summary>ODataImportServiceTests</summary>
public class ODataImportServiceTests
{
    private const string ODataMetadata = """
        <?xml version="1.0" encoding="utf-8"?>
        <edmx:Edmx Version="4.0" xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx">
          <edmx:DataServices>
            <Schema Namespace="TestService" xmlns="http://docs.oasis-open.org/odata/ns/edm">
              <EntityType Name="Product">
                <Key><PropertyRef Name="Id" /></Key>
                <Property Name="Id" Type="Edm.Int32" Nullable="false" />
                <Property Name="Name" Type="Edm.String" />
              </EntityType>
              <EntityContainer Name="Container">
                <EntitySet Name="Products" EntityType="TestService.Product" />
              </EntityContainer>
            </Schema>
          </edmx:DataServices>
        </edmx:Edmx>
        """;

    private static ODataImportService CreateService(string metadata, Mock<IEndpointRepository> repoMock)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(metadata, Encoding.UTF8, "application/xml")
            });

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(handlerMock.Object));

        return new ODataImportService(factoryMock.Object, repoMock.Object, NullLogger<ODataImportService>.Instance);
    }

    private static ODataImportService CreateServiceWithErrorHandler(Mock<IEndpointRepository> repoMock)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(handlerMock.Object));

        return new ODataImportService(factoryMock.Object, repoMock.Object, NullLogger<ODataImportService>.Instance);
    }

    /// <summary>Import_NewODataMetadata_ReturnsCorrectDiff</summary>
    [Fact]
    public async Task Import_NewODataMetadata_ReturnsCorrectDiff()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var service = CreateService(ODataMetadata, repoMock);
        var app = new Core.Models.Application { Id = 1, InterfaceUrl = "http://localhost/$metadata", InterfaceType = Core.Enums.InterfaceType.OData, BaseUrl = "http://localhost" };

        var diff = await service.ImportAsync(app);

        Assert.Equal(2, diff.NewEndpoints.Count);
        Assert.Empty(diff.ChangedEndpoints);
        Assert.Empty(diff.RemovedEndpoints);
    }

    /// <summary>Import_ChangedODataMetadata_ReturnsChangedInDiff</summary>
    [Fact]
    public async Task Import_ChangedODataMetadata_ReturnsChangedInDiff()
    {
        var existing = new List<Core.Models.Endpoint>
        {
            new() { Id = 1, Name = "OldName", Method = Core.Enums.HttpMethod.GET, RelativePath = "Products", ApplicationId = 1 }
        };
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync(existing);
        var service = CreateService(ODataMetadata, repoMock);
        var app = new Core.Models.Application { Id = 1, InterfaceUrl = "http://localhost/$metadata", InterfaceType = Core.Enums.InterfaceType.OData, BaseUrl = "http://localhost" };

        var diff = await service.ImportAsync(app);

        Assert.Contains(diff.ChangedEndpoints, e => e.RelativePath == "Products" && e.Method == Core.Enums.HttpMethod.GET);
    }

    /// <summary>Import_RemovedODataEndpoint_ReturnsRemovedInDiff</summary>
    [Fact]
    public async Task Import_RemovedODataEndpoint_ReturnsRemovedInDiff()
    {
        var existing = new List<Core.Models.Endpoint>
        {
            new() { Id = 1, Name = "DELETE Products", Method = Core.Enums.HttpMethod.DELETE, RelativePath = "Products", ApplicationId = 1 }
        };
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync(existing);
        var service = CreateService(ODataMetadata, repoMock);
        var app = new Core.Models.Application { Id = 1, InterfaceUrl = "http://localhost/$metadata", InterfaceType = Core.Enums.InterfaceType.OData, BaseUrl = "http://localhost" };

        var diff = await service.ImportAsync(app);

        Assert.Contains(diff.RemovedEndpoints, e => e.Method == Core.Enums.HttpMethod.DELETE && e.RelativePath == "Products");
    }

    /// <summary>Import_HttpError_ReturnsErrorMessage</summary>
    [Fact]
    public async Task Import_HttpError_ReturnsErrorMessage()
    {
        var repoMock = new Mock<IEndpointRepository>();
        var service = CreateServiceWithErrorHandler(repoMock);
        var app = new Core.Models.Application { Id = 1, InterfaceUrl = "http://localhost/$metadata", InterfaceType = Core.Enums.InterfaceType.OData, BaseUrl = "http://localhost" };

        var diff = await service.ImportAsync(app);

        Assert.NotNull(diff.ErrorMessage);
    }

    /// <summary>Import_InvalidXml_ReturnsErrorMessage</summary>
    [Fact]
    public async Task Import_InvalidXml_ReturnsErrorMessage()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var service = CreateService("this is not valid xml <<<", repoMock);
        var app = new Core.Models.Application { Id = 1, InterfaceUrl = "http://localhost/$metadata", InterfaceType = Core.Enums.InterfaceType.OData, BaseUrl = "http://localhost" };

        var diff = await service.ImportAsync(app);

        Assert.NotNull(diff.ErrorMessage);
    }

    /// <summary>Import_EmptyInterfaceUrl_ReturnsEmptyDiff</summary>
    [Fact]
    public async Task Import_EmptyInterfaceUrl_ReturnsEmptyDiff()
    {
        var repoMock = new Mock<IEndpointRepository>();
        var service = CreateServiceWithErrorHandler(repoMock);
        var app = new Core.Models.Application { Id = 1, InterfaceUrl = "", InterfaceType = Core.Enums.InterfaceType.OData, BaseUrl = "http://localhost" };

        var diff = await service.ImportAsync(app);

        Assert.Null(diff.ErrorMessage);
        Assert.Empty(diff.NewEndpoints);
        Assert.Empty(diff.ChangedEndpoints);
        Assert.Empty(diff.RemovedEndpoints);
    }

    /// <summary>ApplyDiff_NewChangedRemoved_CallsRepositoryMethods</summary>
    [Fact]
    public async Task ApplyDiff_NewChangedRemoved_CallsRepositoryMethods()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.AddEndpointAsync(It.IsAny<Core.Models.Endpoint>()))
            .ReturnsAsync((Core.Models.Endpoint e) => e);
        repoMock.Setup(r => r.UpdateEndpointAsync(It.IsAny<Core.Models.Endpoint>()))
            .ReturnsAsync((Core.Models.Endpoint e) => e);
        repoMock.Setup(r => r.DeleteEndpointAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var service = CreateService(ODataMetadata, repoMock);

        var newEndpoint = new Core.Models.Endpoint { Id = 0, Name = "GET NewSet", Method = Core.Enums.HttpMethod.GET, RelativePath = "NewSet", ApplicationId = 1 };
        var changedEndpoint = new Core.Models.Endpoint { Id = 2, Name = "GET ChangedSet", Method = Core.Enums.HttpMethod.GET, RelativePath = "ChangedSet", ApplicationId = 1 };
        var removedEndpoint = new Core.Models.Endpoint { Id = 3, Name = "GET RemovedSet", Method = Core.Enums.HttpMethod.GET, RelativePath = "RemovedSet", ApplicationId = 1 };

        var diff = new ImportDiff
        {
            NewEndpoints = [newEndpoint],
            ChangedEndpoints = [changedEndpoint],
            RemovedEndpoints = [removedEndpoint]
        };

        await service.ApplyDiffAsync(diff);

        repoMock.Verify(r => r.AddEndpointAsync(newEndpoint), Times.Once);
        repoMock.Verify(r => r.UpdateEndpointAsync(changedEndpoint), Times.Once);
        repoMock.Verify(r => r.DeleteEndpointAsync(removedEndpoint.Id), Times.Once);
    }
}
