using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;
using Endpoint = Schnittstellenzentrale.Core.Models.Endpoint;

namespace Schnittstellenzentrale.Tests.Integration;

/// <summary>
/// Inhaltliche Verifikation der SignalR-Benachrichtigungen auf allen mutierenden OData-Pfaden:
/// Team-Modus → genau eine passende Benachrichtigung; Benutzermodus → keine.
/// </summary>
public class ODataNotificationIntegrationTests : IClassFixture<ControllerTestFactory>
{
    private readonly ControllerTestFactory _factory;

    /// <summary>Initialisiert ODataNotificationIntegrationTests.</summary>
    /// <param name="factory">Die von xUnit injizierte <see cref="ControllerTestFactory"/>.</param>
    public ODataNotificationIntegrationTests(ControllerTestFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateClientWithTokenAsync()
    {
        var client = _factory.CreateClient();
        await _factory.ObtainTokenAsync(client);
        return client;
    }

    private async Task<HttpRequestMessage> CreateRequestAsync(
        HttpClient client, HttpMethod method, string url, bool teamMode)
    {
        // Jeder Request benötigt ein frisches Token (Rotation); es wird nur ein Request versendet.
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _factory.ObtainTokenAsync(client));
        if (teamMode)
            request.Headers.Add("X-Storage-Mode", "Team");
        return request;
    }

    private void ResetNotificationMock() => _factory.SignalRNotificationMock.Invocations.Clear();

    private void AssertSingleTreeNotification()
    {
        Assert.Single(_factory.SignalRNotificationMock.Invocations);
        _factory.SignalRNotificationMock.Verify(s => s.NotifyTreeChangedAsync(), Times.Once);
    }

    private void AssertSingleEndpointNotification(int endpointId, int applicationId)
    {
        Assert.Single(_factory.SignalRNotificationMock.Invocations);
        _factory.SignalRNotificationMock.Verify(
            s => s.NotifyEndpointChangedAsync(endpointId, applicationId), Times.Once);
    }

    private void AssertSingleEndpointGroupNotification(int groupId, int applicationId)
    {
        Assert.Single(_factory.SignalRNotificationMock.Invocations);
        _factory.SignalRNotificationMock.Verify(
            s => s.NotifyEndpointGroupChangedAsync(groupId, applicationId), Times.Once);
    }

    private void AssertNoNotification() =>
        Assert.Empty(_factory.SignalRNotificationMock.Invocations);

    // ---------- Applications (Baum-Benachrichtigung) ----------

    /// <summary>PostApplication_TeamMode_SendsTreeChanged</summary>
    [Fact]
    public async Task PostApplication_TeamMode_SendsTreeChanged()
    {
        var client = await CreateClientWithTokenAsync();
        var request = await CreateRequestAsync(client, HttpMethod.Post, "/odatav4/Applications", teamMode: true);
        request.Content = JsonContent.Create(new Application { Name = "NotifyPostApp", BaseUrl = "https://n1.example.com" });

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        AssertSingleTreeNotification();
    }

    /// <summary>PutApplication_TeamMode_SendsTreeChanged</summary>
    [Fact]
    public async Task PutApplication_TeamMode_SendsTreeChanged()
    {
        var client = await CreateClientWithTokenAsync();
        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await repo.AddApplicationAsync(new Application { Name = "NotifyPutApp", BaseUrl = "https://n2.example.com" });
        var saved = await repo.GetApplicationByIdAsync(app.Id);
        Assert.NotNull(saved);

        var request = await CreateRequestAsync(client, HttpMethod.Put, $"/odatav4/Applications({app.Id})", teamMode: true);
        request.Content = JsonContent.Create(new Application
        {
            Name = "NotifyPutAppUpdated",
            BaseUrl = "https://n2.example.com",
            RowVersion = saved.RowVersion
        });

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertSingleTreeNotification();
    }

    /// <summary>PatchApplication_TeamMode_SendsTreeChanged</summary>
    [Fact]
    public async Task PatchApplication_TeamMode_SendsTreeChanged()
    {
        var client = await CreateClientWithTokenAsync();
        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await repo.AddApplicationAsync(new Application { Name = "NotifyPatchApp", BaseUrl = "https://n3.example.com" });
        var saved = await repo.GetApplicationByIdAsync(app.Id);
        Assert.NotNull(saved);
        var rowVersion = Convert.ToBase64String(saved.RowVersion);

        var request = await CreateRequestAsync(client, HttpMethod.Patch, $"/odatav4/Applications({app.Id})", teamMode: true);
        request.Content = new StringContent(
            $$"""{"name":"NotifyPatchAppUpdated","rowVersion":"{{rowVersion}}"}""",
            System.Text.Encoding.UTF8, "application/json");

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertSingleTreeNotification();
    }

    /// <summary>DeleteApplication_TeamMode_SendsTreeChanged</summary>
    [Fact]
    public async Task DeleteApplication_TeamMode_SendsTreeChanged()
    {
        var client = await CreateClientWithTokenAsync();
        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await repo.AddApplicationAsync(new Application { Name = "NotifyDeleteApp", BaseUrl = "https://n4.example.com" });

        var request = await CreateRequestAsync(client, HttpMethod.Delete, $"/odatav4/Applications({app.Id})", teamMode: true);

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        AssertSingleTreeNotification();
    }

    /// <summary>PostApplication_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task PostApplication_UserMode_SendsNoNotification()
    {
        var client = await CreateClientWithTokenAsync();
        var request = await CreateRequestAsync(client, HttpMethod.Post, "/odatav4/Applications", teamMode: false);
        request.Content = JsonContent.Create(new Application { Name = "NotifyUserApp", BaseUrl = "https://n5.example.com" });

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        AssertNoNotification();
    }

    /// <summary>PutApplication_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task PutApplication_UserMode_SendsNoNotification()
    {
        var client = await CreateClientWithTokenAsync();
        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await repo.AddApplicationAsync(new Application { Name = "NotifyUserPutApp", BaseUrl = "https://n6.example.com", Owner = null });
        var saved = await repo.GetApplicationByIdAsync(app.Id);
        Assert.NotNull(saved);

        var request = await CreateRequestAsync(client, HttpMethod.Put, $"/odatav4/Applications({app.Id})", teamMode: false);
        request.Content = JsonContent.Create(new Application
        {
            Name = "NotifyUserPutAppUpdated",
            BaseUrl = "https://n6.example.com",
            RowVersion = saved.RowVersion
        });

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNoNotification();
    }

    /// <summary>PatchApplication_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task PatchApplication_UserMode_SendsNoNotification()
    {
        var client = await CreateClientWithTokenAsync();
        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await repo.AddApplicationAsync(new Application { Name = "NotifyUserPatchApp", BaseUrl = "https://n7.example.com" });
        var saved = await repo.GetApplicationByIdAsync(app.Id);
        Assert.NotNull(saved);
        var rowVersion = Convert.ToBase64String(saved.RowVersion);

        var request = await CreateRequestAsync(client, HttpMethod.Patch, $"/odatav4/Applications({app.Id})", teamMode: false);
        request.Content = new StringContent(
            $$"""{"name":"NotifyUserPatchAppUpdated","rowVersion":"{{rowVersion}}"}""",
            System.Text.Encoding.UTF8, "application/json");

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNoNotification();
    }

    /// <summary>DeleteApplication_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task DeleteApplication_UserMode_SendsNoNotification()
    {
        var client = await CreateClientWithTokenAsync();
        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await repo.AddApplicationAsync(new Application { Name = "NotifyUserDeleteApp", BaseUrl = "https://n8.example.com" });

        var request = await CreateRequestAsync(client, HttpMethod.Delete, $"/odatav4/Applications({app.Id})", teamMode: false);

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        AssertNoNotification();
    }

    // ---------- ApplicationGroups (Baum-Benachrichtigung) ----------

    /// <summary>PostApplicationGroup_TeamMode_SendsTreeChanged</summary>
    [Fact]
    public async Task PostApplicationGroup_TeamMode_SendsTreeChanged()
    {
        var client = await CreateClientWithTokenAsync();
        var request = await CreateRequestAsync(client, HttpMethod.Post, "/odatav4/ApplicationGroups", teamMode: true);
        request.Content = JsonContent.Create(new ApplicationGroup { Name = "NotifyPostGroup" });

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        AssertSingleTreeNotification();
    }

    /// <summary>PutApplicationGroup_TeamMode_SendsTreeChanged</summary>
    [Fact]
    public async Task PutApplicationGroup_TeamMode_SendsTreeChanged()
    {
        var client = await CreateClientWithTokenAsync();
        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "NotifyPutGroup" });
        var saved = await repo.GetGroupByIdAsync(group.Id);
        Assert.NotNull(saved);

        var request = await CreateRequestAsync(client, HttpMethod.Put, $"/odatav4/ApplicationGroups({group.Id})", teamMode: true);
        request.Content = JsonContent.Create(new ApplicationGroup
        {
            Name = "NotifyPutGroupUpdated",
            RowVersion = saved.RowVersion
        });

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertSingleTreeNotification();
    }

    /// <summary>PatchApplicationGroup_TeamMode_SendsTreeChanged</summary>
    [Fact]
    public async Task PatchApplicationGroup_TeamMode_SendsTreeChanged()
    {
        var client = await CreateClientWithTokenAsync();
        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "NotifyPatchGroup" });
        var saved = await repo.GetGroupByIdAsync(group.Id);
        Assert.NotNull(saved);
        var rowVersion = Convert.ToBase64String(saved.RowVersion);

        var request = await CreateRequestAsync(client, HttpMethod.Patch, $"/odatav4/ApplicationGroups({group.Id})", teamMode: true);
        request.Content = new StringContent(
            $$"""{"name":"NotifyPatchGroupUpdated","rowVersion":"{{rowVersion}}"}""",
            System.Text.Encoding.UTF8, "application/json");

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertSingleTreeNotification();
    }

    /// <summary>DeleteApplicationGroup_TeamMode_SendsTreeChanged</summary>
    [Fact]
    public async Task DeleteApplicationGroup_TeamMode_SendsTreeChanged()
    {
        var client = await CreateClientWithTokenAsync();
        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "NotifyDeleteGroup" });

        var request = await CreateRequestAsync(client, HttpMethod.Delete, $"/odatav4/ApplicationGroups({group.Id})", teamMode: true);

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        AssertSingleTreeNotification();
    }

    /// <summary>PostApplicationGroup_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task PostApplicationGroup_UserMode_SendsNoNotification()
    {
        var client = await CreateClientWithTokenAsync();
        var request = await CreateRequestAsync(client, HttpMethod.Post, "/odatav4/ApplicationGroups", teamMode: false);
        request.Content = JsonContent.Create(new ApplicationGroup { Name = "NotifyUserGroup" });

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        AssertNoNotification();
    }

    /// <summary>PutApplicationGroup_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task PutApplicationGroup_UserMode_SendsNoNotification()
    {
        var client = await CreateClientWithTokenAsync();
        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "NotifyUserPutGroup" });
        var saved = await repo.GetGroupByIdAsync(group.Id);
        Assert.NotNull(saved);

        var request = await CreateRequestAsync(client, HttpMethod.Put, $"/odatav4/ApplicationGroups({group.Id})", teamMode: false);
        request.Content = JsonContent.Create(new ApplicationGroup
        {
            Name = "NotifyUserPutGroupUpdated",
            RowVersion = saved.RowVersion
        });

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNoNotification();
    }

    /// <summary>PatchApplicationGroup_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task PatchApplicationGroup_UserMode_SendsNoNotification()
    {
        var client = await CreateClientWithTokenAsync();
        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "NotifyUserPatchGroup" });
        var saved = await repo.GetGroupByIdAsync(group.Id);
        Assert.NotNull(saved);
        var rowVersion = Convert.ToBase64String(saved.RowVersion);

        var request = await CreateRequestAsync(client, HttpMethod.Patch, $"/odatav4/ApplicationGroups({group.Id})", teamMode: false);
        request.Content = new StringContent(
            $$"""{"name":"NotifyUserPatchGroupUpdated","rowVersion":"{{rowVersion}}"}""",
            System.Text.Encoding.UTF8, "application/json");

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNoNotification();
    }

    /// <summary>DeleteApplicationGroup_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task DeleteApplicationGroup_UserMode_SendsNoNotification()
    {
        var client = await CreateClientWithTokenAsync();
        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "NotifyUserDeleteGroup" });

        var request = await CreateRequestAsync(client, HttpMethod.Delete, $"/odatav4/ApplicationGroups({group.Id})", teamMode: false);

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        AssertNoNotification();
    }

    // ---------- Endpoints (entitätsspezifische Benachrichtigung) ----------

    /// <summary>PostEndpoint_TeamMode_SendsEndpointChanged</summary>
    [Fact]
    public async Task PostEndpoint_TeamMode_SendsEndpointChanged()
    {
        var client = await CreateClientWithTokenAsync();
        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await appRepo.AddApplicationAsync(new Application { Name = "NotifyEpPostApp", BaseUrl = "https://e1.example.com" });

        var request = await CreateRequestAsync(client, HttpMethod.Post, "/odatav4/Endpoints", teamMode: true);
        request.Content = JsonContent.Create(new Endpoint
        {
            Name = "GET Notify",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/notify",
            ApplicationId = app.Id
        });

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Single(_factory.SignalRNotificationMock.Invocations);
        _factory.SignalRNotificationMock.Verify(
            s => s.NotifyEndpointChangedAsync(It.Is<int>(id => id > 0), app.Id), Times.Once);
    }

    /// <summary>PutEndpoint_TeamMode_SendsEndpointChanged</summary>
    [Fact]
    public async Task PutEndpoint_TeamMode_SendsEndpointChanged()
    {
        var client = await CreateClientWithTokenAsync();
        var (app, endpoint) = await SeedEndpointAsync("NotifyEpPutApp", "GET Put");
        var saved = await _factory.Services.GetRequiredService<IEndpointRepository>()
            .GetEndpointByIdAsync(endpoint.Id);
        Assert.NotNull(saved);

        var request = await CreateRequestAsync(client, HttpMethod.Put, $"/odatav4/Endpoints({endpoint.Id})", teamMode: true);
        request.Content = JsonContent.Create(new Endpoint
        {
            Name = "GET PutUpdated",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/notify-put",
            ApplicationId = app.Id,
            RowVersion = saved.RowVersion
        });

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertSingleEndpointNotification(endpoint.Id, app.Id);
    }

    /// <summary>PatchEndpoint_TeamMode_SendsEndpointChanged</summary>
    [Fact]
    public async Task PatchEndpoint_TeamMode_SendsEndpointChanged()
    {
        var client = await CreateClientWithTokenAsync();
        var (app, endpoint) = await SeedEndpointAsync("NotifyEpPatchApp", "GET Patch");
        var saved = await _factory.Services.GetRequiredService<IEndpointRepository>()
            .GetEndpointByIdAsync(endpoint.Id);
        Assert.NotNull(saved);
        var rowVersion = Convert.ToBase64String(saved.RowVersion);

        var request = await CreateRequestAsync(client, HttpMethod.Patch, $"/odatav4/Endpoints({endpoint.Id})", teamMode: true);
        request.Content = new StringContent(
            $$"""{"name":"GET PatchUpdated","rowVersion":"{{rowVersion}}"}""",
            System.Text.Encoding.UTF8, "application/json");

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertSingleEndpointNotification(endpoint.Id, app.Id);
    }

    /// <summary>DeleteEndpoint_TeamMode_SendsEndpointChanged</summary>
    [Fact]
    public async Task DeleteEndpoint_TeamMode_SendsEndpointChanged()
    {
        var client = await CreateClientWithTokenAsync();
        var (app, endpoint) = await SeedEndpointAsync("NotifyEpDeleteApp", "GET Delete");

        var request = await CreateRequestAsync(client, HttpMethod.Delete, $"/odatav4/Endpoints({endpoint.Id})", teamMode: true);

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        AssertSingleEndpointNotification(endpoint.Id, app.Id);
    }

    /// <summary>PostEndpoint_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task PostEndpoint_UserMode_SendsNoNotification()
    {
        var client = await CreateClientWithTokenAsync();
        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await appRepo.AddApplicationAsync(new Application { Name = "NotifyEpUserApp", BaseUrl = "https://e5.example.com" });

        var request = await CreateRequestAsync(client, HttpMethod.Post, "/odatav4/Endpoints", teamMode: false);
        request.Content = JsonContent.Create(new Endpoint
        {
            Name = "GET NotifyUser",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/notify-user",
            ApplicationId = app.Id
        });

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        AssertNoNotification();
    }

    /// <summary>PutEndpoint_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task PutEndpoint_UserMode_SendsNoNotification()
    {
        var client = await CreateClientWithTokenAsync();
        var (app, endpoint) = await SeedEndpointAsync("NotifyEpUserPutApp", "GET UserPut");
        var saved = await _factory.Services.GetRequiredService<IEndpointRepository>()
            .GetEndpointByIdAsync(endpoint.Id);
        Assert.NotNull(saved);

        var request = await CreateRequestAsync(client, HttpMethod.Put, $"/odatav4/Endpoints({endpoint.Id})", teamMode: false);
        request.Content = JsonContent.Create(new Endpoint
        {
            Name = "GET UserPutUpdated",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/notify-user-put",
            ApplicationId = app.Id,
            RowVersion = saved.RowVersion
        });

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNoNotification();
    }

    /// <summary>PatchEndpoint_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task PatchEndpoint_UserMode_SendsNoNotification()
    {
        var client = await CreateClientWithTokenAsync();
        var (app, endpoint) = await SeedEndpointAsync("NotifyEpUserPatchApp", "GET UserPatch");
        var saved = await _factory.Services.GetRequiredService<IEndpointRepository>()
            .GetEndpointByIdAsync(endpoint.Id);
        Assert.NotNull(saved);
        var rowVersion = Convert.ToBase64String(saved.RowVersion);

        var request = await CreateRequestAsync(client, HttpMethod.Patch, $"/odatav4/Endpoints({endpoint.Id})", teamMode: false);
        request.Content = new StringContent(
            $$"""{"name":"GET UserPatchUpdated","rowVersion":"{{rowVersion}}"}""",
            System.Text.Encoding.UTF8, "application/json");

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNoNotification();
    }

    /// <summary>DeleteEndpoint_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task DeleteEndpoint_UserMode_SendsNoNotification()
    {
        var client = await CreateClientWithTokenAsync();
        var (app, endpoint) = await SeedEndpointAsync("NotifyEpUserDeleteApp", "GET UserDelete");
        _ = app;

        var request = await CreateRequestAsync(client, HttpMethod.Delete, $"/odatav4/Endpoints({endpoint.Id})", teamMode: false);

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        AssertNoNotification();
    }

    // ---------- EndpointGroups (entitätsspezifische Benachrichtigung) ----------

    /// <summary>PostEndpointGroup_TeamMode_SendsEndpointGroupChanged</summary>
    [Fact]
    public async Task PostEndpointGroup_TeamMode_SendsEndpointGroupChanged()
    {
        var client = await CreateClientWithTokenAsync();
        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await appRepo.AddApplicationAsync(new Application { Name = "NotifyEgPostApp", BaseUrl = "https://g1.example.com" });

        var request = await CreateRequestAsync(client, HttpMethod.Post, "/odatav4/EndpointGroups", teamMode: true);
        request.Content = JsonContent.Create(new EndpointGroup { Name = "NotifyGroup", ApplicationId = app.Id });

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Single(_factory.SignalRNotificationMock.Invocations);
        _factory.SignalRNotificationMock.Verify(
            s => s.NotifyEndpointGroupChangedAsync(It.Is<int>(id => id > 0), app.Id), Times.Once);
    }

    /// <summary>PutEndpointGroup_TeamMode_SendsEndpointGroupChanged</summary>
    [Fact]
    public async Task PutEndpointGroup_TeamMode_SendsEndpointGroupChanged()
    {
        var client = await CreateClientWithTokenAsync();
        var (app, group) = await SeedEndpointGroupAsync("NotifyEgPutApp", "NotifyEgPut");
        var saved = await _factory.Services.GetRequiredService<IEndpointRepository>()
            .GetEndpointGroupByIdAsync(group.Id);
        Assert.NotNull(saved);

        var request = await CreateRequestAsync(client, HttpMethod.Put, $"/odatav4/EndpointGroups({group.Id})", teamMode: true);
        request.Content = JsonContent.Create(new EndpointGroup
        {
            Name = "NotifyEgPutUpdated",
            ApplicationId = app.Id,
            RowVersion = saved.RowVersion
        });

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertSingleEndpointGroupNotification(group.Id, app.Id);
    }

    /// <summary>PatchEndpointGroup_TeamMode_SendsEndpointGroupChanged</summary>
    [Fact]
    public async Task PatchEndpointGroup_TeamMode_SendsEndpointGroupChanged()
    {
        var client = await CreateClientWithTokenAsync();
        var (app, group) = await SeedEndpointGroupAsync("NotifyEgPatchApp", "NotifyEgPatch");
        var saved = await _factory.Services.GetRequiredService<IEndpointRepository>()
            .GetEndpointGroupByIdAsync(group.Id);
        Assert.NotNull(saved);
        var rowVersion = Convert.ToBase64String(saved.RowVersion);

        var request = await CreateRequestAsync(client, HttpMethod.Patch, $"/odatav4/EndpointGroups({group.Id})", teamMode: true);
        request.Content = new StringContent(
            $$"""{"name":"NotifyEgPatchUpdated","rowVersion":"{{rowVersion}}"}""",
            System.Text.Encoding.UTF8, "application/json");

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertSingleEndpointGroupNotification(group.Id, app.Id);
    }

    /// <summary>DeleteEndpointGroup_TeamMode_SendsEndpointGroupChanged</summary>
    [Fact]
    public async Task DeleteEndpointGroup_TeamMode_SendsEndpointGroupChanged()
    {
        var client = await CreateClientWithTokenAsync();
        var (app, group) = await SeedEndpointGroupAsync("NotifyEgDeleteApp", "NotifyEgDelete");

        var request = await CreateRequestAsync(client, HttpMethod.Delete, $"/odatav4/EndpointGroups({group.Id})", teamMode: true);

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        AssertSingleEndpointGroupNotification(group.Id, app.Id);
    }

    /// <summary>PostEndpointGroup_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task PostEndpointGroup_UserMode_SendsNoNotification()
    {
        var client = await CreateClientWithTokenAsync();
        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await appRepo.AddApplicationAsync(new Application { Name = "NotifyEgUserApp", BaseUrl = "https://g5.example.com" });

        var request = await CreateRequestAsync(client, HttpMethod.Post, "/odatav4/EndpointGroups", teamMode: false);
        request.Content = JsonContent.Create(new EndpointGroup { Name = "NotifyUserGroup", ApplicationId = app.Id });

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        AssertNoNotification();
    }

    /// <summary>PutEndpointGroup_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task PutEndpointGroup_UserMode_SendsNoNotification()
    {
        var client = await CreateClientWithTokenAsync();
        var (app, group) = await SeedEndpointGroupAsync("NotifyEgUserPutApp", "NotifyEgUserPut");
        var saved = await _factory.Services.GetRequiredService<IEndpointRepository>()
            .GetEndpointGroupByIdAsync(group.Id);
        Assert.NotNull(saved);

        var request = await CreateRequestAsync(client, HttpMethod.Put, $"/odatav4/EndpointGroups({group.Id})", teamMode: false);
        request.Content = JsonContent.Create(new EndpointGroup
        {
            Name = "NotifyEgUserPutUpdated",
            ApplicationId = app.Id,
            RowVersion = saved.RowVersion
        });

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNoNotification();
    }

    /// <summary>PatchEndpointGroup_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task PatchEndpointGroup_UserMode_SendsNoNotification()
    {
        var client = await CreateClientWithTokenAsync();
        var (app, group) = await SeedEndpointGroupAsync("NotifyEgUserPatchApp", "NotifyEgUserPatch");
        _ = app;
        var saved = await _factory.Services.GetRequiredService<IEndpointRepository>()
            .GetEndpointGroupByIdAsync(group.Id);
        Assert.NotNull(saved);
        var rowVersion = Convert.ToBase64String(saved.RowVersion);

        var request = await CreateRequestAsync(client, HttpMethod.Patch, $"/odatav4/EndpointGroups({group.Id})", teamMode: false);
        request.Content = new StringContent(
            $$"""{"name":"NotifyEgUserPatchUpdated","rowVersion":"{{rowVersion}}"}""",
            System.Text.Encoding.UTF8, "application/json");

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertNoNotification();
    }

    /// <summary>DeleteEndpointGroup_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task DeleteEndpointGroup_UserMode_SendsNoNotification()
    {
        var client = await CreateClientWithTokenAsync();
        var (app, group) = await SeedEndpointGroupAsync("NotifyEgUserDeleteApp", "NotifyEgUserDelete");
        _ = app;

        var request = await CreateRequestAsync(client, HttpMethod.Delete, $"/odatav4/EndpointGroups({group.Id})", teamMode: false);

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        AssertNoNotification();
    }

    // ---------- Fehlschlagende Vorgänge senden nichts ----------

    /// <summary>DeleteApplication_NotFound_SendsNoNotification</summary>
    [Fact]
    public async Task DeleteApplication_NotFound_SendsNoNotification()
    {
        var client = await CreateClientWithTokenAsync();
        var request = await CreateRequestAsync(client, HttpMethod.Delete, "/odatav4/Applications(99999)", teamMode: true);

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        AssertNoNotification();
    }

    /// <summary>PatchEndpoint_WithoutRowVersion_SendsNoNotification</summary>
    [Fact]
    public async Task PatchEndpoint_WithoutRowVersion_SendsNoNotification()
    {
        var client = await CreateClientWithTokenAsync();
        var (_, endpoint) = await SeedEndpointAsync("NotifyEpBadPatchApp", "GET BadPatch");

        var request = await CreateRequestAsync(client, HttpMethod.Patch, $"/odatav4/Endpoints({endpoint.Id})", teamMode: true);
        request.Content = new StringContent(
            """{"name":"GET BadPatch"}""",
            System.Text.Encoding.UTF8, "application/json");

        ResetNotificationMock();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        AssertNoNotification();
    }

    // ---------- Hilfsmethoden ----------

    private async Task<(Application App, Endpoint Endpoint)> SeedEndpointAsync(string appName, string endpointName)
    {
        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var endpointRepo = _factory.Services.GetRequiredService<IEndpointRepository>();
        var app = await appRepo.AddApplicationAsync(new Application { Name = appName, BaseUrl = "https://seed.example.com" });
        var endpoint = await endpointRepo.AddEndpointAsync(new Endpoint
        {
            Name = endpointName,
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/seed",
            ApplicationId = app.Id
        });
        return (app, endpoint);
    }

    private async Task<(Application App, EndpointGroup Group)> SeedEndpointGroupAsync(string appName, string groupName)
    {
        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var endpointRepo = _factory.Services.GetRequiredService<IEndpointRepository>();
        var app = await appRepo.AddApplicationAsync(new Application { Name = appName, BaseUrl = "https://seed-g.example.com" });
        var group = await endpointRepo.AddEndpointGroupAsync(new EndpointGroup
        {
            Name = groupName,
            ApplicationId = app.Id
        });
        return (app, group);
    }
}
