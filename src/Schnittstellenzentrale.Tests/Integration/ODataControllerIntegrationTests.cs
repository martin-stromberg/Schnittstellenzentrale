using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Integration;

/// <summary>Integrationstests für alle vier OData-Controller via <see cref="ControllerTestFactory"/>.</summary>
public class ODataControllerIntegrationTests : IClassFixture<ControllerTestFactory>
{
    private readonly ControllerTestFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Initialisiert ODataControllerIntegrationTests.</summary>
    public ODataControllerIntegrationTests(ControllerTestFactory factory)
    {
        _factory = factory;
    }

    private async Task<string> ObtainTokenAsync(HttpClient client) =>
        await _factory.ObtainTokenAsync(client);

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, string? token = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (token != null)
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    /// <summary>GetMetadata_ReturnsValidCsdl</summary>
    [Fact]
    public async Task GetMetadata_ReturnsValidCsdl()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/odatav4/$metadata");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Applications", content);
        Assert.Contains("ApplicationGroups", content);
        Assert.Contains("Endpoints", content);
        Assert.Contains("EndpointGroups", content);
    }

    /// <summary>GetApplications_WithValidToken_Returns200</summary>
    [Fact]
    public async Task GetApplications_WithValidToken_Returns200()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var request = CreateRequest(HttpMethod.Get, "/odatav4/Applications", token);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>GetApplications_WithoutToken_Returns401</summary>
    [Fact]
    public async Task GetApplications_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/odatav4/Applications");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>PostApplication_WithValidToken_Returns201</summary>
    [Fact]
    public async Task PostApplication_WithValidToken_Returns201()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var request = CreateRequest(HttpMethod.Post, "/odatav4/Applications", token);
        request.Content = JsonContent.Create(new Application
        {
            Name = "ODataPostApp",
            BaseUrl = "https://odata-post.example.com"
        });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("ODataPostApp", content);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/odatav4/Applications(", response.Headers.Location.ToString());
    }

    /// <summary>PutApplication_WithValidToken_Returns200</summary>
    [Fact]
    public async Task PutApplication_WithValidToken_Returns200()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await repo.AddApplicationAsync(new Application { Name = "PutApp", BaseUrl = "https://put.example.com" });

        var request = CreateRequest(HttpMethod.Put, $"/odatav4/Applications({app.Id})", token);
        request.Content = JsonContent.Create(new Application
        {
            Name = "PutAppUpdated",
            BaseUrl = "https://put-updated.example.com"
        });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("PutAppUpdated", content);
    }

    /// <summary>PatchApplication_WithValidToken_Returns200</summary>
    [Fact]
    public async Task PatchApplication_WithValidToken_Returns200()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await repo.AddApplicationAsync(new Application { Name = "PatchApp", BaseUrl = "https://patch.example.com" });

        var request = CreateRequest(HttpMethod.Patch, $"/odatav4/Applications({app.Id})", token);
        request.Content = new StringContent(
            """{"Name":"PatchAppPatched"}""",
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("PatchAppPatched", content);
    }

    /// <summary>DeleteApplication_WithValidToken_Returns204</summary>
    [Fact]
    public async Task DeleteApplication_WithValidToken_Returns204()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await repo.AddApplicationAsync(new Application { Name = "DeleteApp", BaseUrl = "https://delete.example.com" });

        var request = CreateRequest(HttpMethod.Delete, $"/odatav4/Applications({app.Id})", token);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    /// <summary>PutApplication_WithSystemApplication_Returns403</summary>
    [Fact]
    public async Task PutApplication_WithSystemApplication_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await repo.AddApplicationAsync(new Application { Name = "SystemApp", BaseUrl = "https://system.example.com", IsSystem = true });

        var request = CreateRequest(HttpMethod.Put, $"/odatav4/Applications({app.Id})", token);
        request.Content = JsonContent.Create(new Application { Name = "Changed", BaseUrl = "https://system.example.com" });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>DeleteApplication_WithSystemApplication_Returns403</summary>
    [Fact]
    public async Task DeleteApplication_WithSystemApplication_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await repo.AddApplicationAsync(new Application { Name = "SystemApp2", BaseUrl = "https://system2.example.com", IsSystem = true });

        var request = CreateRequest(HttpMethod.Delete, $"/odatav4/Applications({app.Id})", token);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>PutEndpoint_WithSystemApplication_Returns403</summary>
    [Fact]
    public async Task PutEndpoint_WithSystemApplication_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var endpointRepo = _factory.Services.GetRequiredService<IEndpointRepository>();

        var app = await appRepo.AddApplicationAsync(new Application { Name = "SystemApp3", BaseUrl = "https://system3.example.com", IsSystem = true });
        var endpoint = await endpointRepo.AddEndpointAsync(new Core.Models.Endpoint
        {
            Name = "GET Endpoint",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/test",
            ApplicationId = app.Id
        });

        var request = CreateRequest(HttpMethod.Put, $"/odatav4/Endpoints({endpoint.Id})", token);
        request.Content = JsonContent.Create(new Core.Models.Endpoint
        {
            Name = "Changed",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/test",
            ApplicationId = app.Id
        });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>DeleteEndpoint_WithSystemApplication_Returns403</summary>
    [Fact]
    public async Task DeleteEndpoint_WithSystemApplication_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var endpointRepo = _factory.Services.GetRequiredService<IEndpointRepository>();

        var app = await appRepo.AddApplicationAsync(new Application { Name = "SystemApp4", BaseUrl = "https://system4.example.com", IsSystem = true });
        var endpoint = await endpointRepo.AddEndpointAsync(new Core.Models.Endpoint
        {
            Name = "GET Endpoint",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/test",
            ApplicationId = app.Id
        });

        var request = CreateRequest(HttpMethod.Delete, $"/odatav4/Endpoints({endpoint.Id})", token);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>PostEndpoint_WithSystemApplication_Returns403</summary>
    [Fact]
    public async Task PostEndpoint_WithSystemApplication_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await appRepo.AddApplicationAsync(new Application { Name = "SystemApp5", BaseUrl = "https://system5.example.com", IsSystem = true });

        var request = CreateRequest(HttpMethod.Post, "/odatav4/Endpoints", token);
        request.Content = JsonContent.Create(new Core.Models.Endpoint
        {
            Name = "New Endpoint",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/new",
            ApplicationId = app.Id
        });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>PutEndpointGroup_WithSystemApplication_Returns403</summary>
    [Fact]
    public async Task PutEndpointGroup_WithSystemApplication_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var endpointRepo = _factory.Services.GetRequiredService<IEndpointRepository>();

        var app = await appRepo.AddApplicationAsync(new Application { Name = "SystemApp6", BaseUrl = "https://system6.example.com", IsSystem = true });
        var group = await endpointRepo.AddEndpointGroupAsync(new EndpointGroup
        {
            Name = "SystemGroup",
            ApplicationId = app.Id
        });

        var request = CreateRequest(HttpMethod.Put, $"/odatav4/EndpointGroups({group.Id})", token);
        request.Content = JsonContent.Create(new EndpointGroup { Name = "Changed", ApplicationId = app.Id });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>DeleteEndpointGroup_WithSystemApplication_Returns403</summary>
    [Fact]
    public async Task DeleteEndpointGroup_WithSystemApplication_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var endpointRepo = _factory.Services.GetRequiredService<IEndpointRepository>();

        var app = await appRepo.AddApplicationAsync(new Application { Name = "SystemApp7", BaseUrl = "https://system7.example.com", IsSystem = true });
        var group = await endpointRepo.AddEndpointGroupAsync(new EndpointGroup
        {
            Name = "SystemGroup2",
            ApplicationId = app.Id
        });

        var request = CreateRequest(HttpMethod.Delete, $"/odatav4/EndpointGroups({group.Id})", token);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>PutApplication_IsSystem_CannotBeElevatedViaPut</summary>
    [Fact]
    public async Task PutApplication_IsSystem_CannotBeElevatedViaPut()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await repo.AddApplicationAsync(new Application { Name = "ElevateApp", BaseUrl = "https://elevate.example.com", IsSystem = false });

        var request = CreateRequest(HttpMethod.Put, $"/odatav4/Applications({app.Id})", token);
        request.Content = JsonContent.Create(new Application
        {
            Name = "ElevateApp",
            BaseUrl = "https://elevate.example.com",
            IsSystem = true
        });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await repo.GetApplicationByIdAsync(app.Id);
        Assert.False(updated!.IsSystem);
    }

    /// <summary>GetApplications_WithFilter_ReturnsFilteredResult</summary>
    [Fact]
    public async Task GetApplications_WithFilter_ReturnsFilteredResult()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        await repo.AddApplicationAsync(new Application { Name = "FilterableApp", BaseUrl = "https://filterable.example.com" });

        var request = CreateRequest(HttpMethod.Get, "/odatav4/Applications?$filter=Name eq 'FilterableApp'", token);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("FilterableApp", content);
    }

    /// <summary>GetApplications_WithExpand_ReturnsRelatedEntities</summary>
    [Fact]
    public async Task GetApplications_WithExpand_ReturnsRelatedEntities()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        await repo.AddApplicationAsync(new Application { Name = "ExpandApp", BaseUrl = "https://expand.example.com" });

        var request = CreateRequest(HttpMethod.Get, "/odatav4/Applications?$expand=Endpoints,EndpointGroups", token);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>GetApplications_WithSelect_ReturnsSelectedFields</summary>
    [Fact]
    public async Task GetApplications_WithSelect_ReturnsSelectedFields()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        await repo.AddApplicationAsync(new Application { Name = "SelectApp", BaseUrl = "https://select.example.com" });

        var request = CreateRequest(HttpMethod.Get, "/odatav4/Applications?$select=Id,Name", token);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("SelectApp", content);
    }

}
