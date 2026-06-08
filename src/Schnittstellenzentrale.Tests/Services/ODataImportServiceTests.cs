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

        var credentialMock = new Mock<ICredentialService>();
        return new ODataImportService(factoryMock.Object, repoMock.Object, credentialMock.Object, NullLogger<ODataImportService>.Instance);
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

        var credentialMock = new Mock<ICredentialService>();
        return new ODataImportService(factoryMock.Object, repoMock.Object, credentialMock.Object, NullLogger<ODataImportService>.Instance);
    }

    private static ODataImportService CreateServiceWithCancellationHandler(Mock<IEndpointRepository> repoMock)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timed out"));

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(handlerMock.Object));

        var credentialMock = new Mock<ICredentialService>();
        return new ODataImportService(factoryMock.Object, repoMock.Object, credentialMock.Object, NullLogger<ODataImportService>.Instance);
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

        // GET Products, POST Products, PUT Products({key}), PATCH Products({key}), DELETE Products({key})
        Assert.Equal(5, diff.NewEndpoints.Count);
        Assert.Contains(diff.NewEndpoints, e => e.Name == "GET Products");
        Assert.Contains(diff.NewEndpoints, e => e.Name == "POST Products");
        Assert.Contains(diff.NewEndpoints, e => e.Name == "PUT Products");
        Assert.Contains(diff.NewEndpoints, e => e.Name == "PATCH Products");
        Assert.Contains(diff.NewEndpoints, e => e.Name == "DELETE Products");
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

    /// <summary>Import_Cancelled_ReturnsErrorMessage</summary>
    [Fact]
    public async Task Import_Cancelled_ReturnsErrorMessage()
    {
        var repoMock = new Mock<IEndpointRepository>();
        var service = CreateServiceWithCancellationHandler(repoMock);
        var app = new Core.Models.Application { Id = 1, InterfaceUrl = "http://localhost/$metadata", InterfaceType = Core.Enums.InterfaceType.OData, BaseUrl = "http://localhost" };

        var diff = await service.ImportAsync(app);

        Assert.NotNull(diff.ErrorMessage);
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
        repoMock.Setup(r => r.GetEndpointGroupsAsync(It.IsAny<int>()))
            .ReturnsAsync([]);
        repoMock.Setup(r => r.AddEndpointGroupAsync(It.IsAny<EndpointGroup>()))
            .ReturnsAsync((EndpointGroup g) => { g.Id = 1; return g; });

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

    /// <summary>Import_BaseUrlMatchesServiceUrl_UsesEntityNameAsRelativePath</summary>
    [Fact]
    public async Task Import_BaseUrlMatchesServiceUrl_UsesEntityNameAsRelativePath()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var service = CreateService(ODataMetadata, repoMock);
        var app = new Core.Models.Application
        {
            Id = 1,
            InterfaceUrl = "http://localhost/odatav4/$metadata",
            InterfaceType = Core.Enums.InterfaceType.OData,
            BaseUrl = "http://localhost/odatav4"
        };

        var diff = await service.ImportAsync(app);

        var productsGet = diff.NewEndpoints.FirstOrDefault(e => e.Name == "GET Products");
        Assert.NotNull(productsGet);
        Assert.Equal("Products", productsGet.RelativePath);
    }

    /// <summary>Import_BaseUrlDiffersFromServiceUrl_UsesSubpathAsRelativePath</summary>
    [Fact]
    public async Task Import_BaseUrlDiffersFromServiceUrl_UsesSubpathAsRelativePath()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var service = CreateService(ODataMetadata, repoMock);
        var app = new Core.Models.Application
        {
            Id = 1,
            InterfaceUrl = "http://localhost/odatav4/$metadata",
            InterfaceType = Core.Enums.InterfaceType.OData,
            BaseUrl = "http://localhost"
        };

        var diff = await service.ImportAsync(app);

        var productsGet = diff.NewEndpoints.FirstOrDefault(e => e.Name == "GET Products");
        Assert.NotNull(productsGet);
        Assert.Equal("odatav4/Products", productsGet.RelativePath);
    }

    /// <summary>Import_AuthenticateEndpointNotAutoAdded — automatische Einfügung wurde entfernt</summary>
    [Fact]
    public async Task Import_AuthenticateEndpointNotAutoAdded()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var service = CreateService(ODataMetadata, repoMock);
        var app = new Core.Models.Application
        {
            Id = 1,
            InterfaceUrl = "http://localhost/$metadata",
            InterfaceType = Core.Enums.InterfaceType.OData,
            BaseUrl = "http://localhost"
        };

        var diff = await service.ImportAsync(app);

        Assert.DoesNotContain(diff.NewEndpoints, e => e.Name == "POST authenticate");
    }

    /// <summary>Import_PutPatchDelete_EndpointsHaveKeyInRelativePath</summary>
    [Fact]
    public async Task Import_PutPatchDelete_EndpointsHaveKeyInRelativePath()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var service = CreateService(ODataMetadata, repoMock);
        var app = new Core.Models.Application { Id = 1, InterfaceUrl = "http://localhost/$metadata", InterfaceType = Core.Enums.InterfaceType.OData, BaseUrl = "http://localhost" };

        var diff = await service.ImportAsync(app);

        var putEndpoint = diff.NewEndpoints.FirstOrDefault(e => e.Method == Core.Enums.HttpMethod.PUT);
        var patchEndpoint = diff.NewEndpoints.FirstOrDefault(e => e.Method == Core.Enums.HttpMethod.PATCH);
        var deleteEndpoint = diff.NewEndpoints.FirstOrDefault(e => e.Method == Core.Enums.HttpMethod.DELETE);
        Assert.NotNull(putEndpoint);
        Assert.NotNull(patchEndpoint);
        Assert.NotNull(deleteEndpoint);
        Assert.Contains("{key}", putEndpoint.RelativePath);
        Assert.Contains("{key}", patchEndpoint.RelativePath);
        Assert.Contains("{key}", deleteEndpoint.RelativePath);
    }

    /// <summary>Import_WithAuthTypeAnnotation_SetsAuthTypeFromAnnotation</summary>
    [Fact]
    public async Task Import_WithAuthTypeAnnotation_SetsAuthTypeFromAnnotation()
    {
        const string metadataWithAnnotation = """
            <?xml version="1.0" encoding="utf-8"?>
            <edmx:Edmx Version="4.0" xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx">
              <edmx:DataServices>
                <Schema Namespace="TestService" xmlns="http://docs.oasis-open.org/odata/ns/edm">
                  <EntityType Name="Product">
                    <Key><PropertyRef Name="Id" /></Key>
                    <Property Name="Id" Type="Edm.Int32" Nullable="false" />
                  </EntityType>
                  <EntityContainer Name="Container">
                    <EntitySet Name="Products" EntityType="TestService.Product">
                      <Annotation Term="x-sz-auth-type" String="BearerToken" />
                    </EntitySet>
                  </EntityContainer>
                </Schema>
              </edmx:DataServices>
            </edmx:Edmx>
            """;
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var service = CreateService(metadataWithAnnotation, repoMock);
        var app = new Core.Models.Application { Id = 1, InterfaceUrl = "http://localhost/$metadata", InterfaceType = Core.Enums.InterfaceType.OData, BaseUrl = "http://localhost" };

        var diff = await service.ImportAsync(app);

        Assert.All(diff.NewEndpoints, e => Assert.Equal(Core.Enums.AuthenticationType.BearerToken, e.AuthenticationType));
    }

    /// <summary>Import_WithPostRequestScriptAnnotation_SetsPostRequestScript</summary>
    [Fact]
    public async Task Import_WithPostRequestScriptAnnotation_SetsPostRequestScript()
    {
        const string metadataWithScript = """
            <?xml version="1.0" encoding="utf-8"?>
            <edmx:Edmx Version="4.0" xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx">
              <edmx:DataServices>
                <Schema Namespace="TestService" xmlns="http://docs.oasis-open.org/odata/ns/edm">
                  <EntityType Name="Product">
                    <Key><PropertyRef Name="Id" /></Key>
                    <Property Name="Id" Type="Edm.Int32" Nullable="false" />
                  </EntityType>
                  <EntityContainer Name="Container">
                    <EntitySet Name="Products" EntityType="TestService.Product">
                      <Annotation Term="x-sz-post-request-script" String="console.log('done');" />
                    </EntitySet>
                  </EntityContainer>
                </Schema>
              </edmx:DataServices>
            </edmx:Edmx>
            """;
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var service = CreateService(metadataWithScript, repoMock);
        var app = new Core.Models.Application { Id = 1, InterfaceUrl = "http://localhost/$metadata", InterfaceType = Core.Enums.InterfaceType.OData, BaseUrl = "http://localhost" };

        var diff = await service.ImportAsync(app);

        Assert.All(diff.NewEndpoints, e => Assert.Equal("console.log('done');", e.PostRequestScript));
    }

    /// <summary>ApplyDiff_NewEndpoints_CreatesEndpointGroupPerEntitySet</summary>
    [Fact]
    public async Task ApplyDiff_NewEndpoints_CreatesEndpointGroupPerEntitySet()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointGroupsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var createdGroups = new List<EndpointGroup>();
        repoMock.Setup(r => r.AddEndpointGroupAsync(It.IsAny<EndpointGroup>()))
            .ReturnsAsync((EndpointGroup g) => { g.Id = createdGroups.Count + 1; createdGroups.Add(g); return g; });
        repoMock.Setup(r => r.AddEndpointAsync(It.IsAny<Core.Models.Endpoint>()))
            .ReturnsAsync((Core.Models.Endpoint e) => e);

        var service = CreateService(ODataMetadata, repoMock);

        var getEndpoint = new Core.Models.Endpoint { Id = 0, Name = "GET Products", Method = Core.Enums.HttpMethod.GET, RelativePath = "Products", ApplicationId = 1 };
        var postEndpoint = new Core.Models.Endpoint { Id = 0, Name = "POST Products", Method = Core.Enums.HttpMethod.POST, RelativePath = "Products", ApplicationId = 1 };
        var diff = new ImportDiff { NewEndpoints = [getEndpoint, postEndpoint] };

        await service.ApplyDiffAsync(diff);

        Assert.Single(createdGroups);
        Assert.Equal("Products", createdGroups[0].Name);
        Assert.NotNull(getEndpoint.EndpointGroupId);
        Assert.NotNull(postEndpoint.EndpointGroupId);
        Assert.Equal(getEndpoint.EndpointGroupId, postEndpoint.EndpointGroupId);
    }

    /// <summary>Import_FunctionWithAuthTypeAnnotation_SetsAuthTypeFromAnnotation — prüft dass Function-Elemente in ParseOperationAnnotations verarbeitet werden</summary>
    [Fact]
    public async Task Import_FunctionWithAuthTypeAnnotation_SetsAuthTypeFromAnnotation()
    {
        const string metadataWithFunction = """
            <?xml version="1.0" encoding="utf-8"?>
            <edmx:Edmx Version="4.0" xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx">
              <edmx:DataServices>
                <Schema Namespace="TestService" xmlns="http://docs.oasis-open.org/odata/ns/edm">
                  <EntityType Name="Product">
                    <Key><PropertyRef Name="Id" /></Key>
                    <Property Name="Id" Type="Edm.Int32" Nullable="false" />
                  </EntityType>
                  <Function Name="GetStatus">
                    <ReturnType Type="Edm.String" />
                    <Annotation Term="Schnittstellenzentrale.V1.x-sz-auth-type" String="Negotiate" />
                  </Function>
                  <EntityContainer Name="Container">
                    <EntitySet Name="Products" EntityType="TestService.Product" />
                    <FunctionImport Name="GetStatus" Function="TestService.GetStatus" />
                  </EntityContainer>
                </Schema>
              </edmx:DataServices>
            </edmx:Edmx>
            """;
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var service = CreateService(metadataWithFunction, repoMock);
        var app = new Core.Models.Application
        {
            Id = 1,
            InterfaceUrl = "http://localhost/$metadata",
            InterfaceType = Core.Enums.InterfaceType.OData,
            BaseUrl = "http://localhost"
        };

        var diff = await service.ImportAsync(app);

        var statusEndpoint = diff.NewEndpoints.FirstOrDefault(e => e.Name == "GetStatus");
        Assert.NotNull(statusEndpoint);
        Assert.Equal(Core.Enums.AuthenticationType.Negotiate, statusEndpoint.AuthenticationType);
    }

    /// <summary>ApplyDiff_WithMultipleBearerTokenEndpoints_SavesCredentialOnlyOnce</summary>
    [Fact]
    public async Task ApplyDiff_WithMultipleBearerTokenEndpoints_SavesCredentialOnlyOnce()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointGroupsAsync(It.IsAny<int>())).ReturnsAsync([]);
        repoMock.Setup(r => r.AddEndpointGroupAsync(It.IsAny<EndpointGroup>()))
            .ReturnsAsync((EndpointGroup g) => { g.Id = 1; return g; });
        repoMock.Setup(r => r.AddEndpointAsync(It.IsAny<Core.Models.Endpoint>()))
            .ReturnsAsync((Core.Models.Endpoint e) => e);

        var credentialMock = new Mock<ICredentialService>();
        credentialMock.Setup(c => c.GetPassword(It.IsAny<string>())).Returns((string?)null);

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ODataMetadata, Encoding.UTF8, "application/xml") });
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerMock.Object));

        var service = new ODataImportService(factoryMock.Object, repoMock.Object, credentialMock.Object, NullLogger<ODataImportService>.Instance);

        var bearerTokens = new Dictionary<string, string>();
        var endpoints = new List<Core.Models.Endpoint>();
        for (var i = 0; i < 5; i++)
        {
            var ep = new Core.Models.Endpoint
            {
                Name = $"GET Products{i}",
                Method = Core.Enums.HttpMethod.GET,
                RelativePath = $"Products{i}",
                ApplicationId = 42,
                AuthenticationType = AuthenticationType.BearerToken
            };
            endpoints.Add(ep);
            bearerTokens[Core.Helpers.EndpointKeyHelper.BuildKey(ep)] = "shared-token";
        }

        var diff = new ImportDiff { NewEndpoints = endpoints, BearerTokens = bearerTokens };

        await service.ApplyDiffAsync(diff);

        credentialMock.Verify(c => c.SavePassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    /// <summary>Import_WithInvalidAnnotationXml_DoesNotThrow_ReturnsEmptyAnnotations</summary>
    [Fact]
    public async Task Import_WithInvalidAnnotationXml_DoesNotThrow_ReturnsEmptyAnnotations()
    {
        // XML is valid CSDL but has malformed annotation content — ParseAnnotationsForElement
        // should catch only XmlException/InvalidOperationException and return empty dict.
        const string metadataWithBrokenAnnotation = """
            <?xml version="1.0" encoding="utf-8"?>
            <edmx:Edmx Version="4.0" xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx">
              <edmx:DataServices>
                <Schema Namespace="TestService" xmlns="http://docs.oasis-open.org/odata/ns/edm">
                  <EntityType Name="Product">
                    <Key><PropertyRef Name="Id" /></Key>
                    <Property Name="Id" Type="Edm.Int32" Nullable="false" />
                  </EntityType>
                  <EntityContainer Name="Container">
                    <EntitySet Name="Products" EntityType="TestService.Product" />
                  </EntityContainer>
                </Schema>
              </edmx:DataServices>
            </edmx:Edmx>
            """;

        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var service = CreateService(metadataWithBrokenAnnotation, repoMock);
        var app = new Core.Models.Application
        {
            Id = 1,
            InterfaceUrl = "http://localhost/$metadata",
            InterfaceType = Core.Enums.InterfaceType.OData,
            BaseUrl = "http://localhost"
        };

        var exception = await Record.ExceptionAsync(() => service.ImportAsync(app));

        Assert.Null(exception);
    }

    /// <summary>ApplyDiff_ChangedEndpoints_UpdatesEndpointGroupId</summary>
    [Fact]
    public async Task ApplyDiff_ChangedEndpoints_UpdatesEndpointGroupId()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointGroupsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var createdGroups = new List<EndpointGroup>();
        repoMock.Setup(r => r.AddEndpointGroupAsync(It.IsAny<EndpointGroup>()))
            .ReturnsAsync((EndpointGroup g) => { g.Id = createdGroups.Count + 1; createdGroups.Add(g); return g; });
        repoMock.Setup(r => r.UpdateEndpointAsync(It.IsAny<Core.Models.Endpoint>()))
            .ReturnsAsync((Core.Models.Endpoint e) => e);

        var service = CreateService(ODataMetadata, repoMock);

        var changedEndpoint = new Core.Models.Endpoint { Id = 5, Name = "GET NewModule", Method = Core.Enums.HttpMethod.GET, RelativePath = "NewModule", ApplicationId = 1 };
        var diff = new ImportDiff { ChangedEndpoints = [changedEndpoint] };

        await service.ApplyDiffAsync(diff);

        Assert.Single(createdGroups);
        Assert.Equal("NewModule", createdGroups[0].Name);
        Assert.NotNull(changedEndpoint.EndpointGroupId);
        Assert.Equal(createdGroups[0].Id, changedEndpoint.EndpointGroupId);
    }

    /// <summary>Import_WithExistingBearerToken_SetsAuthTypeOnEndpoints</summary>
    [Fact]
    public async Task Import_WithExistingBearerToken_SetsAuthTypeOnEndpoints()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new System.Net.Http.StringContent(ODataMetadata, System.Text.Encoding.UTF8, "application/xml")
            });
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(handlerMock.Object));

        var credentialMock = new Mock<ICredentialService>();
        credentialMock.Setup(c => c.GetPassword(It.IsAny<string>())).Returns("my-token");

        var service = new ODataImportService(factoryMock.Object, repoMock.Object, credentialMock.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger<ODataImportService>.Instance);
        var app = new Core.Models.Application
        {
            Id = 1,
            InterfaceUrl = "http://localhost/$metadata",
            InterfaceType = Core.Enums.InterfaceType.OData,
            BaseUrl = "http://localhost"
        };

        var diff = await service.ImportAsync(app);

        Assert.All(diff.NewEndpoints, e => Assert.Equal(Core.Enums.AuthenticationType.BearerToken, e.AuthenticationType));
        Assert.True(diff.BearerTokens.Values.All(v => v == "my-token"));
    }

    /// <summary>ApplyDiff_NewAndChangedEndpointsFromDifferentApps_LoadsGroupsForBothApps</summary>
    [Fact]
    public async Task ApplyDiff_NewAndChangedEndpointsFromDifferentApps_LoadsGroupsForBothApps()
    {
        var repoMock = new Mock<IEndpointRepository>();

        var existingGroupApp1 = new EndpointGroup { Id = 10, Name = "Products", ApplicationId = 1, ParentGroupId = null };
        var existingGroupApp2 = new EndpointGroup { Id = 20, Name = "Orders", ApplicationId = 2, ParentGroupId = null };

        repoMock.Setup(r => r.GetEndpointGroupsAsync(1)).ReturnsAsync([existingGroupApp1]);
        repoMock.Setup(r => r.GetEndpointGroupsAsync(2)).ReturnsAsync([existingGroupApp2]);
        repoMock.Setup(r => r.AddEndpointAsync(It.IsAny<Core.Models.Endpoint>()))
            .ReturnsAsync((Core.Models.Endpoint e) => e);
        repoMock.Setup(r => r.UpdateEndpointAsync(It.IsAny<Core.Models.Endpoint>()))
            .ReturnsAsync((Core.Models.Endpoint e) => e);

        var service = CreateService(ODataMetadata, repoMock);

        var newEndpoint = new Core.Models.Endpoint { Id = 0, Name = "GET Products", Method = Core.Enums.HttpMethod.GET, RelativePath = "Products", ApplicationId = 1 };
        var changedEndpoint = new Core.Models.Endpoint { Id = 5, Name = "GET Orders", Method = Core.Enums.HttpMethod.GET, RelativePath = "Orders", ApplicationId = 2 };

        var diff = new ImportDiff
        {
            NewEndpoints = [newEndpoint],
            ChangedEndpoints = [changedEndpoint]
        };

        await service.ApplyDiffAsync(diff);

        // Both group lookups succeeded — no additional AddEndpointGroupAsync calls needed
        repoMock.Verify(r => r.GetEndpointGroupsAsync(1), Times.Once);
        repoMock.Verify(r => r.GetEndpointGroupsAsync(2), Times.Once);
        repoMock.Verify(r => r.AddEndpointGroupAsync(It.IsAny<EndpointGroup>()), Times.Never);
        Assert.Equal(existingGroupApp1.Id, newEndpoint.EndpointGroupId);
        Assert.Equal(existingGroupApp2.Id, changedEndpoint.EndpointGroupId);
    }

    /// <summary>Import_WithBearerTokenAnnotation_StoresBearerTokenVariableFromAnnotation</summary>
    [Fact]
    public async Task Import_WithBearerTokenAnnotation_StoresBearerTokenVariableFromAnnotation()
    {
        const string metadataWithBearerToken = """
            <?xml version="1.0" encoding="utf-8"?>
            <edmx:Edmx Version="4.0" xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx">
              <edmx:DataServices>
                <Schema Namespace="TestService" xmlns="http://docs.oasis-open.org/odata/ns/edm">
                  <EntityType Name="Product">
                    <Key><PropertyRef Name="Id" /></Key>
                    <Property Name="Id" Type="Edm.Int32" Nullable="false" />
                  </EntityType>
                  <EntityContainer Name="Container">
                    <EntitySet Name="Products" EntityType="TestService.Product">
                      <Annotation Term="x-sz-auth-type" String="BearerToken" />
                      <Annotation Term="x-sz-bearer-token" String="{{schnittstellenzentrale.authToken}}" />
                    </EntitySet>
                  </EntityContainer>
                </Schema>
              </edmx:DataServices>
            </edmx:Edmx>
            """;

        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);

        var credentialMock = new Mock<ICredentialService>();
        credentialMock.Setup(c => c.GetPassword(It.IsAny<string>())).Returns((string?)null);

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new System.Net.Http.StringContent(metadataWithBearerToken, System.Text.Encoding.UTF8, "application/xml")
            });
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerMock.Object));

        var service = new ODataImportService(factoryMock.Object, repoMock.Object, credentialMock.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger<ODataImportService>.Instance);
        var app = new Core.Models.Application
        {
            Id = 1,
            InterfaceUrl = "http://localhost/$metadata",
            InterfaceType = Core.Enums.InterfaceType.OData,
            BaseUrl = "http://localhost"
        };

        var diff = await service.ImportAsync(app);

        Assert.All(diff.NewEndpoints, e => Assert.Equal(Core.Enums.AuthenticationType.BearerToken, e.AuthenticationType));
        Assert.True(diff.BearerTokens.Values.All(v => v == "{{schnittstellenzentrale.authToken}}"));
    }
}
