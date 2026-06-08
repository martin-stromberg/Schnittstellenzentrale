using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Schnittstellenzentrale.Tests.Integration;

/// <summary>Integrationstests für den <see cref="Schnittstellenzentrale.Controllers.ApplicationImportController"/>.</summary>
public class ApplicationImportControllerIntegrationTests : IClassFixture<ApplicationImportControllerTestFactory>
{
    private readonly ApplicationImportControllerTestFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Initialisiert den Test mit der spezialisierten Factory.</summary>
    public ApplicationImportControllerIntegrationTests(ApplicationImportControllerTestFactory factory)
    {
        _factory = factory;
    }

    private static HttpRequestMessage BuildRequest(HttpMethod method, string url, string token)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Storage-Mode", "Team");
        return request;
    }

    /// <summary>Import_WithRestApplication_Returns200AndDiff</summary>
    [Fact]
    public async Task Import_WithRestApplication_Returns200AndDiff()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await appRepo.AddApplicationAsync(new Application
        {
            Name = "SwaggerImportTestApp",
            BaseUrl = "https://swagger-import.example.com",
            InterfaceUrl = "https://swagger-import.example.com/swagger.json",
            InterfaceType = InterfaceType.Rest
        });

        var request = BuildRequest(HttpMethod.Post, $"/api/applications/{app.Id}/import", token);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var diff = await response.Content.ReadFromJsonAsync<ImportDiff>(JsonOptions);
        Assert.NotNull(diff);
        Assert.Single(diff.NewEndpoints);
        Assert.Equal("GET Items", diff.NewEndpoints[0].Name);
    }

    /// <summary>Import_WithODataApplication_Returns200AndDiff</summary>
    [Fact]
    public async Task Import_WithODataApplication_Returns200AndDiff()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await appRepo.AddApplicationAsync(new Application
        {
            Name = "ODataImportTestApp",
            BaseUrl = "https://odata-import.example.com",
            InterfaceUrl = "https://odata-import.example.com/$metadata",
            InterfaceType = InterfaceType.OData
        });

        var request = BuildRequest(HttpMethod.Post, $"/api/applications/{app.Id}/import", token);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var diff = await response.Content.ReadFromJsonAsync<ImportDiff>(JsonOptions);
        Assert.NotNull(diff);
        Assert.Equal("Verbindung fehlgeschlagen", diff.ErrorMessage);
    }

    /// <summary>Import_WithUnknownInterfaceType_Returns422</summary>
    [Fact]
    public async Task Import_WithUnknownInterfaceType_Returns422()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await appRepo.AddApplicationAsync(new Application
        {
            Name = "UnknownTypeApp",
            BaseUrl = "https://unknown.example.com",
            InterfaceType = InterfaceType.Unknown
        });

        var request = BuildRequest(HttpMethod.Post, $"/api/applications/{app.Id}/import", token);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    /// <summary>Import_WithoutToken_Returns401</summary>
    [Fact]
    public async Task Import_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/applications/1/import", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>Import_WithUnknownApplicationId_Returns404</summary>
    [Fact]
    public async Task Import_WithUnknownApplicationId_Returns404()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var request = BuildRequest(HttpMethod.Post, "/api/applications/99999/import", token);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>Import_WithUnknownInterfaceType_Returns422WithErrorMessageBody</summary>
    [Fact]
    public async Task Import_WithUnknownInterfaceType_Returns422WithErrorMessageBody()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await appRepo.AddApplicationAsync(new Application
        {
            Name = "UnknownTypeAppWithBody",
            BaseUrl = "https://unknown-body.example.com",
            InterfaceType = InterfaceType.Unknown
        });

        var request = BuildRequest(HttpMethod.Post, $"/api/applications/{app.Id}/import", token);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        Assert.NotNull(body);
        Assert.True(body.RootElement.TryGetProperty("errorMessage", out var errorMsg));
        Assert.False(string.IsNullOrEmpty(errorMsg.GetString()));
    }

    /// <summary>ApplyODataDiff_WithODataApplication_Returns204AndCallsService</summary>
    [Fact]
    public async Task ApplyODataDiff_WithODataApplication_Returns204AndCallsService()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await appRepo.AddApplicationAsync(new Application
        {
            Name = "ODataApplyDiffApp",
            BaseUrl = "https://odata-apply.example.com",
            InterfaceUrl = "https://odata-apply.example.com/$metadata",
            InterfaceType = InterfaceType.OData
        });

        _factory.ODataImportMock
            .Setup(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>()))
            .Returns(Task.CompletedTask);

        var diff = new ImportDiff();
        var request = BuildRequest(HttpMethod.Post, $"/api/applications/{app.Id}/odata-import/apply", token);
        request.Content = System.Net.Http.Json.JsonContent.Create(diff);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        _factory.ODataImportMock.Verify(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>()), Times.Once);
    }

    /// <summary>ApplyODataDiff_WithNonODataApplication_Returns422</summary>
    [Fact]
    public async Task ApplyODataDiff_WithNonODataApplication_Returns422()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await appRepo.AddApplicationAsync(new Application
        {
            Name = "RestApplyDiffApp",
            BaseUrl = "https://rest-apply.example.com",
            InterfaceType = InterfaceType.Rest
        });

        var diff = new ImportDiff();
        var request = BuildRequest(HttpMethod.Post, $"/api/applications/{app.Id}/odata-import/apply", token);
        request.Content = System.Net.Http.Json.JsonContent.Create(diff);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    /// <summary>ApplyODataDiff_WithoutToken_Returns401</summary>
    [Fact]
    public async Task ApplyODataDiff_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var diff = new ImportDiff();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/applications/1/odata-import/apply");
        request.Content = System.Net.Http.Json.JsonContent.Create(diff);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>ApplyODataDiff_WithUnknownApplicationId_Returns404</summary>
    [Fact]
    public async Task ApplyODataDiff_WithUnknownApplicationId_Returns404()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var diff = new ImportDiff();
        var request = BuildRequest(HttpMethod.Post, "/api/applications/99999/odata-import/apply", token);
        request.Content = System.Net.Http.Json.JsonContent.Create(diff);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

/// <summary>Spezialisierte WebApplicationFactory für ApplicationImportController-Tests mit gemockten Import-Services.</summary>
public class ApplicationImportControllerTestFactory : ControllerTestFactory
{
    /// <summary>Gemockter Swagger-Import-Service.</summary>
    public readonly Mock<ISwaggerImportService> SwaggerImportMock = new();

    /// <summary>Gemockter OData-Import-Service.</summary>
    public readonly Mock<IODataImportService> ODataImportMock = new();

    /// <summary>Initialisiert die Factory und konfiguriert die gemockten Import-Services.</summary>
    public ApplicationImportControllerTestFactory()
    {
        SwaggerImportMock
            .Setup(s => s.ImportAsync(It.IsAny<Application>()))
            .ReturnsAsync(new ImportDiff
            {
                NewEndpoints = [new Core.Models.Endpoint { Name = "GET Items", RelativePath = "/items", ApplicationId = 0 }]
            });

        ODataImportMock
            .Setup(s => s.ImportAsync(It.IsAny<Application>()))
            .ReturnsAsync(new ImportDiff { ErrorMessage = "Verbindung fehlgeschlagen" });
    }

    /// <inheritdoc/>
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ISwaggerImportService>();
            services.AddSingleton(SwaggerImportMock.Object);

            services.RemoveAll<IODataImportService>();
            services.AddSingleton(ODataImportMock.Object);
        });
    }
}
