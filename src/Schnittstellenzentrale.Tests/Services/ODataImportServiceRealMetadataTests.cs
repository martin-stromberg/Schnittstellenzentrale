using System.Net;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OData.Edm.Csdl;
using Moq;
using Moq.Protected;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Services;
using Schnittstellenzentrale.OData;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>Unit-Tests für <see cref="ODataImportService"/> mit dem realen CSDL-Dokument von <see cref="ODataEdmModelBuilder"/>.</summary>
public class ODataImportServiceRealMetadataTests
{
    private static string BuildRealMetadata()
    {
        var model = ODataEdmModelBuilder.Build();
        var sb = new StringBuilder();
        using var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true });
        CsdlWriter.TryWriteCsdl(model, xmlWriter, CsdlTarget.OData, out _);
        xmlWriter.Flush();
        return sb.ToString();
    }

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

    /// <summary>Import_RealMetadata_ReturnsCorrectEntitySetEndpoints</summary>
    [Fact]
    public async Task Import_RealMetadata_ReturnsCorrectEntitySetEndpoints()
    {
        var metadata = BuildRealMetadata();
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);

        var service = CreateService(metadata, repoMock);
        var app = new Application
        {
            Id = 1,
            InterfaceUrl = "http://localhost/odatav4/$metadata",
            InterfaceType = InterfaceType.OData,
            BaseUrl = "http://localhost/odatav4"
        };

        var diff = await service.ImportAsync(app);

        Assert.Null(diff.ErrorMessage);

        var newEndpointNames = diff.NewEndpoints.Select(e => e.Name).ToList();
        Assert.Contains(newEndpointNames, n => n.Contains("Applications") && n.Contains("GET"));
        Assert.Contains(newEndpointNames, n => n.Contains("Applications") && n.Contains("POST"));
        Assert.Contains(newEndpointNames, n => n.Contains("ApplicationGroups") && n.Contains("GET"));
        Assert.Contains(newEndpointNames, n => n.Contains("ApplicationGroups") && n.Contains("POST"));
        Assert.Contains(newEndpointNames, n => n.Contains("Endpoints") && n.Contains("GET"));
        Assert.Contains(newEndpointNames, n => n.Contains("Endpoints") && n.Contains("POST"));
        Assert.Contains(newEndpointNames, n => n.Contains("EndpointGroups") && n.Contains("GET"));
        Assert.Contains(newEndpointNames, n => n.Contains("EndpointGroups") && n.Contains("POST"));
    }
}
