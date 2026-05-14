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

    [Fact]
    public async Task Import_NewODataMetadata_ReturnsCorrectDiff()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var service = CreateService(ODataMetadata, repoMock);
        var app = new Core.Models.Application { Id = 1, MetadataUrl = "http://localhost/$metadata", BaseUrl = "http://localhost" };

        var diff = await service.ImportAsync(app);

        Assert.True(diff.NewEndpoints.Count >= 1);
        Assert.Empty(diff.ChangedEndpoints);
        Assert.Empty(diff.RemovedEndpoints);
    }

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
        var app = new Core.Models.Application { Id = 1, MetadataUrl = "http://localhost/$metadata", BaseUrl = "http://localhost" };

        var diff = await service.ImportAsync(app);

        Assert.Contains(diff.ChangedEndpoints, e => e.RelativePath == "Products" && e.Method == Core.Enums.HttpMethod.GET);
    }

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
        var app = new Core.Models.Application { Id = 1, MetadataUrl = "http://localhost/$metadata", BaseUrl = "http://localhost" };

        var diff = await service.ImportAsync(app);

        Assert.Contains(diff.RemovedEndpoints, e => e.Method == Core.Enums.HttpMethod.DELETE && e.RelativePath == "Products");
    }
}
