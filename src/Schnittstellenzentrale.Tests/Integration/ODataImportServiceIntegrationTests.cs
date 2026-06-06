using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Services;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Integration;

/// <summary>WebApplicationFactory-Integrationstest für IODataImportService mit realer In-Memory-Datenbank.</summary>
public class ODataImportServiceIntegrationTests : IClassFixture<ControllerTestFactory>
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

    private readonly ControllerTestFactory _factory;

    /// <summary>Initialisiert ODataImportServiceIntegrationTests.</summary>
    public ODataImportServiceIntegrationTests(ControllerTestFactory factory)
    {
        _factory = factory;
    }

    private static ODataImportService CreateServiceWithMetadata(string metadata, IEndpointRepository endpointRepository)
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

        return new ODataImportService(factoryMock.Object, endpointRepository, NullLogger<ODataImportService>.Instance);
    }

    /// <summary>Import_NewODataApplication_PersistsEndpoints</summary>
    [Fact]
    public async Task Import_NewODataApplication_PersistsEndpoints()
    {
        using var scope = _factory.Services.CreateScope();
        var applicationRepository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
        var endpointRepository = scope.ServiceProvider.GetRequiredService<IEndpointRepository>();

        var application = await applicationRepository.AddApplicationAsync(new Application
        {
            Name = "OData Integration Test App",
            BaseUrl = "http://localhost",
            InterfaceUrl = "http://localhost/$metadata",
            InterfaceType = InterfaceType.OData
        });

        var service = CreateServiceWithMetadata(ODataMetadata, endpointRepository);

        var diff = await service.ImportAsync(application);

        Assert.Equal(2, diff.NewEndpoints.Count);
        Assert.Null(diff.ErrorMessage);

        await service.ApplyDiffAsync(diff);

        var persistedEndpoints = await endpointRepository.GetEndpointsAsync(application.Id);
        Assert.Equal(2, persistedEndpoints.Count);
        Assert.Contains(persistedEndpoints, e => e.RelativePath == "Products");
    }
}
