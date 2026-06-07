using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Core.Contracts;
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

    /// <summary>PatchApplication_WithInvalidBase64IconData_Returns400</summary>
    [Fact]
    public async Task PatchApplication_WithInvalidBase64IconData_Returns400()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await repo.AddApplicationAsync(new Application { Name = "IconPatchApp", BaseUrl = "https://iconpatch.example.com" });

        var request = CreateRequest(HttpMethod.Patch, $"/odatav4/Applications({app.Id})", token);
        request.Content = new StringContent(
            """{"IconData":"!!!ungueltig!!!"}""",
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>PatchApplication_WithValidBase64IconData_Returns200</summary>
    [Fact]
    public async Task PatchApplication_WithValidBase64IconData_Returns200()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await repo.AddApplicationAsync(new Application { Name = "IconPatchValidApp", BaseUrl = "https://iconpatchvalid.example.com" });

        var validBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
        var request = CreateRequest(HttpMethod.Patch, $"/odatav4/Applications({app.Id})", token);
        request.Content = new StringContent(
            $$$"""{"IconData":"{{{validBase64}}}"}""",
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>PatchApplicationGroup_WithValidBase64IconData_Returns200AndPersistsIconData</summary>
    [Fact]
    public async Task PatchApplicationGroup_WithValidBase64IconData_Returns200AndPersistsIconData()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
        var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "IconGroupPatchValid" });

        var validBase64 = Convert.ToBase64String(new byte[] { 10, 20, 30, 40 });
        var request = CreateRequest(HttpMethod.Patch, $"/odatav4/ApplicationGroups({group.Id})", token);
        request.Content = new StringContent(
            $$$"""{"IconData":"{{{validBase64}}}"}""",
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var readScope = _factory.Services.CreateScope();
        var readRepo = readScope.ServiceProvider.GetRequiredService<IApplicationRepository>();
        var updated = await readRepo.GetGroupByIdAsync(group.Id);
        Assert.NotNull(updated!.IconData);
        Assert.Equal(new byte[] { 10, 20, 30, 40 }, updated.IconData);
    }

    /// <summary>PatchApplicationGroup_WithInvalidBase64IconData_Returns400</summary>
    [Fact]
    public async Task PatchApplicationGroup_WithInvalidBase64IconData_Returns400()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "IconGroupPatchInvalid" });

        var request = CreateRequest(HttpMethod.Patch, $"/odatav4/ApplicationGroups({group.Id})", token);
        request.Content = new StringContent(
            """{"IconData":"!!!ungueltig!!!"}""",
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>ODataAuthenticate_Get_ReturnsToken</summary>
    [Fact]
    public async Task ODataAuthenticate_Get_ReturnsToken()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/odatav4/authenticate");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthenticateResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.NotEmpty(body.Token);
    }

    /// <summary>ODataAuthenticate_Post_ReturnsToken</summary>
    [Fact]
    public async Task ODataAuthenticate_Post_ReturnsToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/odatav4/authenticate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthenticateResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.NotEmpty(body.Token);
    }

    /// <summary>ODataAuthenticate_TokenCanBeUsedForODataRequests</summary>
    [Fact]
    public async Task ODataAuthenticate_TokenCanBeUsedForODataRequests()
    {
        var client = _factory.CreateClient();

        var authResponse = await client.GetAsync("/odatav4/authenticate");
        Assert.Equal(HttpStatusCode.OK, authResponse.StatusCode);
        var body = await authResponse.Content.ReadFromJsonAsync<AuthenticateResponse>(JsonOptions);
        var token = body!.Token;

        var request = CreateRequest(HttpMethod.Get, "/odatav4/Applications", token);
        var dataResponse = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, dataResponse.StatusCode);
    }

    /// <summary>PostApplication_WithIsSystemTrue_IsSystemIsFalseInDb</summary>
    [Fact]
    public async Task PostApplication_WithIsSystemTrue_IsSystemIsFalseInDb()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var request = CreateRequest(HttpMethod.Post, "/odatav4/Applications", token);
        request.Content = JsonContent.Create(new Application
        {
            Name = "IsSystemPostApp",
            BaseUrl = "https://issystem-post.example.com",
            IsSystem = true
        });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var saved = JsonSerializer.Deserialize<Application>(content, JsonOptions);
        Assert.NotNull(saved);
        Assert.False(saved.IsSystem);
    }

    /// <summary>PostApplicationGroup_WithIsSystemTrue_IsSystemIsFalseInDb</summary>
    [Fact]
    public async Task PostApplicationGroup_WithIsSystemTrue_IsSystemIsFalseInDb()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var request = CreateRequest(HttpMethod.Post, "/odatav4/ApplicationGroups", token);
        request.Content = JsonContent.Create(new ApplicationGroup
        {
            Name = "IsSystemPostGroup",
            IsSystem = true
        });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var saved = JsonSerializer.Deserialize<ApplicationGroup>(content, JsonOptions);
        Assert.NotNull(saved);
        Assert.False(saved.IsSystem);
    }

    /// <summary>PutApplication_WithRowVersion_AppliesConcurrencyCheck</summary>
    [Fact]
    public async Task PutApplication_WithRowVersion_AppliesConcurrencyCheck()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await repo.AddApplicationAsync(new Application { Name = "ConcurrencyApp", BaseUrl = "https://concurrency.example.com" });

        var savedApp = await repo.GetApplicationByIdAsync(app.Id);
        Assert.NotNull(savedApp);

        var request = CreateRequest(HttpMethod.Put, $"/odatav4/Applications({app.Id})", token);
        request.Content = JsonContent.Create(new Application
        {
            Name = "ConcurrencyAppUpdated",
            BaseUrl = "https://concurrency.example.com",
            RowVersion = savedApp.RowVersion
        });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>PutEndpoint_WithEndpointGroupFromDifferentApplication_Returns400</summary>
    [Fact]
    public async Task PutEndpoint_WithEndpointGroupFromDifferentApplication_Returns400()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var endpointRepo = _factory.Services.GetRequiredService<IEndpointRepository>();

        var app1 = await appRepo.AddApplicationAsync(new Application { Name = "App1CrossGroup", BaseUrl = "https://app1-cross.example.com" });
        var app2 = await appRepo.AddApplicationAsync(new Application { Name = "App2CrossGroup", BaseUrl = "https://app2-cross.example.com" });

        var endpoint = await endpointRepo.AddEndpointAsync(new Core.Models.Endpoint
        {
            Name = "GET Test",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/test",
            ApplicationId = app1.Id
        });

        var foreignGroup = await endpointRepo.AddEndpointGroupAsync(new EndpointGroup
        {
            Name = "ForeignGroup",
            ApplicationId = app2.Id
        });

        var request = CreateRequest(HttpMethod.Put, $"/odatav4/Endpoints({endpoint.Id})", token);
        request.Content = JsonContent.Create(new Core.Models.Endpoint
        {
            Name = "GET Test",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/test",
            ApplicationId = app1.Id,
            EndpointGroupId = foreignGroup.Id
        });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>PutEndpoint_WithEndpointGroupFromSameApplication_Returns200</summary>
    [Fact]
    public async Task PutEndpoint_WithEndpointGroupFromSameApplication_Returns200()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var endpointRepo = _factory.Services.GetRequiredService<IEndpointRepository>();

        var app = await appRepo.AddApplicationAsync(new Application { Name = "AppSameGroup", BaseUrl = "https://app-same-group.example.com" });

        var endpoint = await endpointRepo.AddEndpointAsync(new Core.Models.Endpoint
        {
            Name = "GET SameGroup",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/same",
            ApplicationId = app.Id
        });

        var ownGroup = await endpointRepo.AddEndpointGroupAsync(new EndpointGroup
        {
            Name = "OwnGroup",
            ApplicationId = app.Id
        });

        var request = CreateRequest(HttpMethod.Put, $"/odatav4/Endpoints({endpoint.Id})", token);
        request.Content = JsonContent.Create(new Core.Models.Endpoint
        {
            Name = "GET SameGroup",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/same",
            ApplicationId = app.Id,
            EndpointGroupId = ownGroup.Id
        });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>PatchEndpoint_WithEndpointGroupFromDifferentApplication_Returns400</summary>
    [Fact]
    public async Task PatchEndpoint_WithEndpointGroupFromDifferentApplication_Returns400()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var endpointRepo = _factory.Services.GetRequiredService<IEndpointRepository>();

        var app1 = await appRepo.AddApplicationAsync(new Application { Name = "PatchApp1CrossGroup", BaseUrl = "https://patch-app1-cross.example.com" });
        var app2 = await appRepo.AddApplicationAsync(new Application { Name = "PatchApp2CrossGroup", BaseUrl = "https://patch-app2-cross.example.com" });

        var endpoint = await endpointRepo.AddEndpointAsync(new Core.Models.Endpoint
        {
            Name = "GET PatchCross",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/patch-cross",
            ApplicationId = app1.Id
        });

        var foreignGroup = await endpointRepo.AddEndpointGroupAsync(new EndpointGroup
        {
            Name = "PatchForeignGroup",
            ApplicationId = app2.Id
        });

        var request = CreateRequest(HttpMethod.Patch, $"/odatav4/Endpoints({endpoint.Id})", token);
        request.Content = new StringContent(
            $$$"""{"EndpointGroupId":{{{foreignGroup.Id}}}}""",
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>PatchEndpoint_WithEndpointGroupFromSameApplication_Returns200</summary>
    [Fact]
    public async Task PatchEndpoint_WithEndpointGroupFromSameApplication_Returns200()
    {
        var client = _factory.CreateClient();
        var token = await ObtainTokenAsync(client);

        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var endpointRepo = _factory.Services.GetRequiredService<IEndpointRepository>();

        var app = await appRepo.AddApplicationAsync(new Application { Name = "PatchAppSameGroup", BaseUrl = "https://patch-app-same-group.example.com" });

        var endpoint = await endpointRepo.AddEndpointAsync(new Core.Models.Endpoint
        {
            Name = "GET PatchSame",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/patch-same",
            ApplicationId = app.Id
        });

        var ownGroup = await endpointRepo.AddEndpointGroupAsync(new EndpointGroup
        {
            Name = "PatchOwnGroup",
            ApplicationId = app.Id
        });

        var request = CreateRequest(HttpMethod.Patch, $"/odatav4/Endpoints({endpoint.Id})", token);
        request.Content = new StringContent(
            $$$"""{"EndpointGroupId":{{{ownGroup.Id}}}}""",
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

}
