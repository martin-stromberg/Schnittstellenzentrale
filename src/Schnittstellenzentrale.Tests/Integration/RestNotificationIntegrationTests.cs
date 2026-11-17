using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;
using Endpoint = Schnittstellenzentrale.Core.Models.Endpoint;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Schnittstellenzentrale.Tests.Integration;

/// <summary>
/// Inhaltliche Verifikation der SignalR-Benachrichtigungen auf mutierenden REST-Pfaden:
/// Header-/Query-Parameter-Kindmutationen sowie die Policy-Verdrahtung der Basis-Mutationen.
/// </summary>
public class RestNotificationIntegrationTests : IClassFixture<ControllerTestFactory>
{
    private readonly ControllerTestFactory _factory;

    /// <summary>Initialisiert RestNotificationIntegrationTests.</summary>
    /// <param name="factory">Die von xUnit injizierte <see cref="ControllerTestFactory"/>.</param>
    public RestNotificationIntegrationTests(ControllerTestFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpRequestMessage> CreateRequestAsync(
        HttpClient client, HttpMethod method, string url, bool teamMode)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _factory.ObtainTokenAsync(client));
        if (teamMode)
            request.Headers.Add("X-Storage-Mode", "Team");
        return request;
    }

    private async Task<(Application App, Endpoint Endpoint)> SeedEndpointAsync(string appName)
    {
        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var endpointRepo = _factory.Services.GetRequiredService<IEndpointRepository>();
        var app = await appRepo.AddApplicationAsync(new Application { Name = appName, BaseUrl = "https://rest-notify.example.com" });
        var endpoint = await endpointRepo.AddEndpointAsync(new Endpoint
        {
            Name = "GET Notify",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/rest-notify",
            ApplicationId = app.Id
        });
        return (app, endpoint);
    }

    /// <summary>PostEndpointHeader_TeamMode_SendsEndpointChanged</summary>
    [Fact]
    public async Task PostEndpointHeader_TeamMode_SendsEndpointChanged()
    {
        var client = _factory.CreateClient();
        var (app, endpoint) = await SeedEndpointAsync("RestHeaderApp");

        var request = await CreateRequestAsync(client, HttpMethod.Post, "/api/endpoints/headers", teamMode: true);
        request.Content = JsonContent.Create(new AddEndpointKeyValueRequest
        {
            Key = "X-Test",
            Value = "1",
            EndpointId = endpoint.Id
        });

        _factory.SignalRNotificationMock.Invocations.Clear();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Single(_factory.SignalRNotificationMock.Invocations);
        _factory.SignalRNotificationMock.Verify(
            s => s.NotifyEndpointChangedAsync(endpoint.Id, app.Id), Times.Once);
    }

    /// <summary>PostEndpointHeader_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task PostEndpointHeader_UserMode_SendsNoNotification()
    {
        var client = _factory.CreateClient();
        var (_, endpoint) = await SeedEndpointAsync("RestHeaderUserApp");

        var request = await CreateRequestAsync(client, HttpMethod.Post, "/api/endpoints/headers", teamMode: false);
        request.Content = JsonContent.Create(new AddEndpointKeyValueRequest
        {
            Key = "X-Test",
            Value = "1",
            EndpointId = endpoint.Id
        });

        _factory.SignalRNotificationMock.Invocations.Clear();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Empty(_factory.SignalRNotificationMock.Invocations);
    }

    /// <summary>DeleteEndpointHeader_TeamMode_SendsEndpointChanged</summary>
    [Fact]
    public async Task DeleteEndpointHeader_TeamMode_SendsEndpointChanged()
    {
        var client = _factory.CreateClient();
        var (app, endpoint) = await SeedEndpointAsync("RestHeaderDeleteApp");
        var endpointRepo = _factory.Services.GetRequiredService<IEndpointRepository>();
        var header = await endpointRepo.AddHeaderAsync(new EndpointHeader
        {
            Key = "X-Del",
            Value = "1",
            EndpointId = endpoint.Id
        });

        var request = await CreateRequestAsync(client, HttpMethod.Delete, $"/api/endpoints/headers/{header.Id}", teamMode: true);

        _factory.SignalRNotificationMock.Invocations.Clear();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Single(_factory.SignalRNotificationMock.Invocations);
        _factory.SignalRNotificationMock.Verify(
            s => s.NotifyEndpointChangedAsync(endpoint.Id, app.Id), Times.Once);
    }

    /// <summary>DeleteEndpointHeader_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task DeleteEndpointHeader_UserMode_SendsNoNotification()
    {
        var client = _factory.CreateClient();
        var (_, endpoint) = await SeedEndpointAsync("RestHeaderDeleteUserApp");
        var endpointRepo = _factory.Services.GetRequiredService<IEndpointRepository>();
        var header = await endpointRepo.AddHeaderAsync(new EndpointHeader
        {
            Key = "X-Del",
            Value = "1",
            EndpointId = endpoint.Id
        });

        var request = await CreateRequestAsync(client, HttpMethod.Delete, $"/api/endpoints/headers/{header.Id}", teamMode: false);

        _factory.SignalRNotificationMock.Invocations.Clear();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(_factory.SignalRNotificationMock.Invocations);
    }

    /// <summary>PostEndpointQueryParameter_TeamMode_SendsEndpointChanged</summary>
    [Fact]
    public async Task PostEndpointQueryParameter_TeamMode_SendsEndpointChanged()
    {
        var client = _factory.CreateClient();
        var (app, endpoint) = await SeedEndpointAsync("RestQueryApp");

        var request = await CreateRequestAsync(client, HttpMethod.Post, "/api/endpoints/query-parameters", teamMode: true);
        request.Content = JsonContent.Create(new AddEndpointKeyValueRequest
        {
            Key = "q",
            Value = "1",
            EndpointId = endpoint.Id
        });

        _factory.SignalRNotificationMock.Invocations.Clear();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Single(_factory.SignalRNotificationMock.Invocations);
        _factory.SignalRNotificationMock.Verify(
            s => s.NotifyEndpointChangedAsync(endpoint.Id, app.Id), Times.Once);
    }

    /// <summary>PostEndpointQueryParameter_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task PostEndpointQueryParameter_UserMode_SendsNoNotification()
    {
        var client = _factory.CreateClient();
        var (_, endpoint) = await SeedEndpointAsync("RestQueryUserApp");

        var request = await CreateRequestAsync(client, HttpMethod.Post, "/api/endpoints/query-parameters", teamMode: false);
        request.Content = JsonContent.Create(new AddEndpointKeyValueRequest
        {
            Key = "q",
            Value = "1",
            EndpointId = endpoint.Id
        });

        _factory.SignalRNotificationMock.Invocations.Clear();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Empty(_factory.SignalRNotificationMock.Invocations);
    }

    /// <summary>DeleteEndpointQueryParameter_TeamMode_SendsEndpointChanged</summary>
    [Fact]
    public async Task DeleteEndpointQueryParameter_TeamMode_SendsEndpointChanged()
    {
        var client = _factory.CreateClient();
        var (app, endpoint) = await SeedEndpointAsync("RestQueryDeleteApp");
        var endpointRepo = _factory.Services.GetRequiredService<IEndpointRepository>();
        var parameter = await endpointRepo.AddQueryParameterAsync(new EndpointQueryParameter
        {
            Key = "q",
            Value = "1",
            EndpointId = endpoint.Id
        });

        var request = await CreateRequestAsync(client, HttpMethod.Delete, $"/api/endpoints/query-parameters/{parameter.Id}", teamMode: true);

        _factory.SignalRNotificationMock.Invocations.Clear();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Single(_factory.SignalRNotificationMock.Invocations);
        _factory.SignalRNotificationMock.Verify(
            s => s.NotifyEndpointChangedAsync(endpoint.Id, app.Id), Times.Once);
    }

    /// <summary>DeleteEndpointQueryParameter_UserMode_SendsNoNotification</summary>
    [Fact]
    public async Task DeleteEndpointQueryParameter_UserMode_SendsNoNotification()
    {
        var client = _factory.CreateClient();
        var (_, endpoint) = await SeedEndpointAsync("RestQueryDeleteUserApp");
        var endpointRepo = _factory.Services.GetRequiredService<IEndpointRepository>();
        var parameter = await endpointRepo.AddQueryParameterAsync(new EndpointQueryParameter
        {
            Key = "q",
            Value = "1",
            EndpointId = endpoint.Id
        });

        var request = await CreateRequestAsync(client, HttpMethod.Delete, $"/api/endpoints/query-parameters/{parameter.Id}", teamMode: false);

        _factory.SignalRNotificationMock.Invocations.Clear();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(_factory.SignalRNotificationMock.Invocations);
    }

    /// <summary>PostApplication_TeamMode_SendsExactlyOneTreeChanged (Policy-Verdrahtung der REST-Controller)</summary>
    [Fact]
    public async Task PostApplication_TeamMode_SendsExactlyOneTreeChanged()
    {
        var client = _factory.CreateClient();

        var request = await CreateRequestAsync(client, HttpMethod.Post, "/api/applications", teamMode: true);
        request.Content = JsonContent.Create(new CreateApplicationRequest
        {
            Name = "RestPolicyApp",
            BaseUrl = "https://rest-policy.example.com"
        });

        _factory.SignalRNotificationMock.Invocations.Clear();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Single(_factory.SignalRNotificationMock.Invocations);
        _factory.SignalRNotificationMock.Verify(s => s.NotifyTreeChangedAsync(), Times.Once);
    }
}
