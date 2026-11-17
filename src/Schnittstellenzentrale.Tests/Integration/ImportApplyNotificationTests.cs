using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;
using Endpoint = Schnittstellenzentrale.Core.Models.Endpoint;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Schnittstellenzentrale.Tests.Integration;

/// <summary>Verifikation der Benachrichtigung auf den Import-Apply-Endpunkten (genau eine TreeChanged bei tatsächlichen Änderungen).</summary>
public class ImportApplyNotificationTests : IClassFixture<ApplicationImportControllerTestFactory>
{
    private readonly ApplicationImportControllerTestFactory _factory;

    /// <summary>Initialisiert ImportApplyNotificationTests.</summary>
    /// <param name="factory">Die von xUnit injizierte <see cref="ApplicationImportControllerTestFactory"/>.</param>
    public ImportApplyNotificationTests(ApplicationImportControllerTestFactory factory)
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

    /// <summary>ApplyODataDiff_TeamModeWithChanges_SendsTreeChanged</summary>
    [Fact]
    public async Task ApplyODataDiff_TeamModeWithChanges_SendsTreeChanged()
    {
        var client = _factory.CreateClient();
        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await appRepo.AddApplicationAsync(new Application
        {
            Name = "ODataApplyNotifyApp",
            BaseUrl = "https://odata-apply-notify.example.com",
            InterfaceUrl = "https://odata-apply-notify.example.com/$metadata",
            InterfaceType = InterfaceType.OData
        });

        _factory.ODataImportMock
            .Setup(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>()))
            .Returns(Task.CompletedTask);

        var diff = new ImportDiff
        {
            NewEndpoints =
            [
                new Endpoint { Name = "GET Items", RelativePath = "/items", ApplicationId = app.Id }
            ]
        };
        var request = await CreateRequestAsync(client, HttpMethod.Post, $"/api/applications/{app.Id}/odata-import/apply", teamMode: true);
        request.Content = JsonContent.Create(diff);

        _factory.SignalRNotificationMock.Invocations.Clear();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Single(_factory.SignalRNotificationMock.Invocations);
        _factory.SignalRNotificationMock.Verify(s => s.NotifyTreeChangedAsync(), Times.Once);
    }

    /// <summary>ApplyODataDiff_TeamModeWithEmptyDiff_SendsNoNotification</summary>
    [Fact]
    public async Task ApplyODataDiff_TeamModeWithEmptyDiff_SendsNoNotification()
    {
        var client = _factory.CreateClient();
        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await appRepo.AddApplicationAsync(new Application
        {
            Name = "ODataApplyEmptyNotifyApp",
            BaseUrl = "https://odata-apply-empty.example.com",
            InterfaceUrl = "https://odata-apply-empty.example.com/$metadata",
            InterfaceType = InterfaceType.OData
        });

        _factory.ODataImportMock
            .Setup(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>()))
            .Returns(Task.CompletedTask);

        var request = await CreateRequestAsync(client, HttpMethod.Post, $"/api/applications/{app.Id}/odata-import/apply", teamMode: true);
        request.Content = JsonContent.Create(new ImportDiff());

        _factory.SignalRNotificationMock.Invocations.Clear();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(_factory.SignalRNotificationMock.Invocations);
    }

    /// <summary>ApplyODataDiff_UserModeWithChanges_SendsNoNotification</summary>
    [Fact]
    public async Task ApplyODataDiff_UserModeWithChanges_SendsNoNotification()
    {
        var client = _factory.CreateClient();
        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await appRepo.AddApplicationAsync(new Application
        {
            Name = "ODataApplyUserNotifyApp",
            BaseUrl = "https://odata-apply-user.example.com",
            InterfaceUrl = "https://odata-apply-user.example.com/$metadata",
            InterfaceType = InterfaceType.OData
        });

        _factory.ODataImportMock
            .Setup(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>()))
            .Returns(Task.CompletedTask);

        var diff = new ImportDiff
        {
            NewEndpoints =
            [
                new Endpoint { Name = "GET Items", RelativePath = "/items", ApplicationId = app.Id }
            ]
        };
        var request = await CreateRequestAsync(client, HttpMethod.Post, $"/api/applications/{app.Id}/odata-import/apply", teamMode: false);
        request.Content = JsonContent.Create(diff);

        _factory.SignalRNotificationMock.Invocations.Clear();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(_factory.SignalRNotificationMock.Invocations);
    }

    /// <summary>ApplySwaggerDiff_TeamModeWithChanges_SendsTreeChanged</summary>
    [Fact]
    public async Task ApplySwaggerDiff_TeamModeWithChanges_SendsTreeChanged()
    {
        var client = _factory.CreateClient();
        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await appRepo.AddApplicationAsync(new Application
        {
            Name = "SwaggerApplyNotifyApp",
            BaseUrl = "https://swagger-apply-notify.example.com",
            InterfaceUrl = "https://swagger-apply-notify.example.com/swagger.json",
            InterfaceType = InterfaceType.Rest
        });

        _factory.SwaggerImportMock
            .Setup(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var diff = new ImportDiff
        {
            ChangedEndpoints =
            [
                new Endpoint { Name = "GET Items", RelativePath = "/items", ApplicationId = app.Id }
            ]
        };
        var request = await CreateRequestAsync(client, HttpMethod.Post, $"/api/applications/{app.Id}/swagger-import/apply", teamMode: true);
        request.Content = JsonContent.Create(diff);

        _factory.SignalRNotificationMock.Invocations.Clear();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Single(_factory.SignalRNotificationMock.Invocations);
        _factory.SignalRNotificationMock.Verify(s => s.NotifyTreeChangedAsync(), Times.Once);
    }

    /// <summary>ApplySwaggerDiff_UserModeWithChanges_SendsNoNotification</summary>
    [Fact]
    public async Task ApplySwaggerDiff_UserModeWithChanges_SendsNoNotification()
    {
        var client = _factory.CreateClient();
        var appRepo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var app = await appRepo.AddApplicationAsync(new Application
        {
            Name = "SwaggerApplyUserNotifyApp",
            BaseUrl = "https://swagger-apply-user.example.com",
            InterfaceUrl = "https://swagger-apply-user.example.com/swagger.json",
            InterfaceType = InterfaceType.Rest
        });

        _factory.SwaggerImportMock
            .Setup(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var diff = new ImportDiff
        {
            ChangedEndpoints =
            [
                new Endpoint { Name = "GET Items", RelativePath = "/items", ApplicationId = app.Id }
            ]
        };
        var request = await CreateRequestAsync(client, HttpMethod.Post, $"/api/applications/{app.Id}/swagger-import/apply", teamMode: false);
        request.Content = JsonContent.Create(diff);

        _factory.SignalRNotificationMock.Invocations.Clear();
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(_factory.SignalRNotificationMock.Invocations);
    }
}
