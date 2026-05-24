using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Services;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>ApplicationApiClientTests</summary>
public class ApplicationApiClientTests
{
    private const string BaseUrl = "https://localhost:5001";

    private static HttpClient CreateHttpClient(Mock<HttpMessageHandler> handlerMock)
        => new(handlerMock.Object);

    private static (
        ApplicationApiClient client,
        Mock<HttpMessageHandler> handlerMock,
        Mock<ITokenStore> tokenStoreMock)
    CreateClient(
        string initialToken,
        string newToken,
        HttpStatusCode dataStatusCode,
        string dataResponseJson,
        StorageMode storageMode = StorageMode.Team)
    {
        var handlerMock = CreateHandlerMock(newToken, dataStatusCode, dataResponseJson);
        var httpClient = CreateHttpClient(handlerMock);

        var tokenStoreMock = new Mock<ITokenStore>();
        tokenStoreMock
            .Setup(s => s.CreateTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(new AuthToken
            {
                TokenValue = initialToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                WindowsUsername = "testuser"
            });

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Api:BaseUrl"]).Returns(BaseUrl);

        var storageModeServiceMock = new Mock<IStorageModeService>();
        storageModeServiceMock.Setup(s => s.CurrentMode).Returns(storageMode);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.GetCurrentUserName()).Returns("testuser");

        var apiClient = new ApplicationApiClient(
            httpClient,
            httpContextAccessorMock.Object,
            configurationMock.Object,
            tokenStoreMock.Object,
            storageModeServiceMock.Object,
            currentUserServiceMock.Object);

        return (apiClient, handlerMock, tokenStoreMock);
    }

    private static Mock<HttpMessageHandler> CreateHandlerMock(
        string newToken,
        HttpStatusCode dataStatusCode,
        string dataResponseJson)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                var response = new HttpResponseMessage(dataStatusCode)
                {
                    Content = new StringContent(dataResponseJson, System.Text.Encoding.UTF8, "application/json")
                };
                response.Headers.Add("X-New-Token", newToken);
                return response;
            });

        return handlerMock;
    }

    /// <summary>AddGroupAsync_IssuesTokenViaTokenStoreAndSendsCorrectRequest_ReturnsResponse</summary>
    [Fact]
    public async Task AddGroupAsync_IssuesTokenViaTokenStoreAndSendsCorrectRequest_ReturnsResponse()
    {
        var authToken = Guid.NewGuid().ToString();
        var newToken = Guid.NewGuid().ToString();
        var groupResponse = new ApplicationGroupResponse { Id = 1, Name = "TestGruppe" };
        var groupResponseJson = JsonSerializer.Serialize(groupResponse);

        var (apiClient, handlerMock, tokenStoreMock) = CreateClient(authToken, newToken, HttpStatusCode.Created, groupResponseJson);

        var result = await apiClient.AddGroupAsync(new ApplicationGroup { Name = "TestGruppe" });

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("TestGruppe", result.Name);

        tokenStoreMock.Verify(s => s.CreateTokenAsync("testuser"), Times.Once());

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsolutePath == "/api/application-groups" &&
                r.Headers.Authorization != null &&
                r.Headers.Authorization.Parameter == authToken &&
                r.Headers.Contains("X-Storage-Mode")),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>AddGroupAsync_TokenIssuedOnlyOnceForMultipleCalls</summary>
    [Fact]
    public async Task AddGroupAsync_TokenIssuedOnlyOnceForMultipleCalls()
    {
        var authToken = Guid.NewGuid().ToString();
        var newToken = Guid.NewGuid().ToString();
        var groupResponse = new ApplicationGroupResponse { Id = 1, Name = "TestGruppe" };
        var groupResponseJson = JsonSerializer.Serialize(groupResponse);

        var (apiClient, handlerMock, tokenStoreMock) = CreateClient(authToken, newToken, HttpStatusCode.Created, groupResponseJson);

        await apiClient.AddGroupAsync(new ApplicationGroup { Name = "TestGruppe" });
        await apiClient.AddGroupAsync(new ApplicationGroup { Name = "ZweiterAufruf" });

        tokenStoreMock.Verify(s => s.CreateTokenAsync(It.IsAny<string>()), Times.Once());

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath == "/api/application-groups"),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>AddApplicationAsync_IssuesTokenAndSendsCorrectRequest_ReturnsResponse</summary>
    [Fact]
    public async Task AddApplicationAsync_IssuesTokenAndSendsCorrectRequest_ReturnsResponse()
    {
        var authToken = Guid.NewGuid().ToString();
        var newToken = Guid.NewGuid().ToString();
        var appResponse = new ApplicationResponse { Id = 2, Name = "TestApp", BaseUrl = "https://example.com" };
        var appResponseJson = JsonSerializer.Serialize(appResponse);

        var (apiClient, handlerMock, tokenStoreMock) = CreateClient(authToken, newToken, HttpStatusCode.Created, appResponseJson);

        var result = await apiClient.AddApplicationAsync(new Application { Name = "TestApp", BaseUrl = "https://example.com" });

        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal("TestApp", result.Name);
        Assert.Equal("https://example.com", result.BaseUrl);

        tokenStoreMock.Verify(s => s.CreateTokenAsync("testuser"), Times.Once());

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsolutePath == "/api/applications" &&
                r.Headers.Authorization != null &&
                r.Headers.Authorization.Parameter == authToken &&
                r.Headers.Contains("X-Storage-Mode")),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>GetGroupsAsync_SendsCorrectHeadersAndReturnsMappedList</summary>
    [Fact]
    public async Task GetGroupsAsync_SendsCorrectHeadersAndReturnsMappedList()
    {
        var authToken = Guid.NewGuid().ToString();
        var newToken = Guid.NewGuid().ToString();
        var groupList = new List<ApplicationGroupResponse>
        {
            new() { Id = 1, Name = "Gruppe1" },
            new() { Id = 2, Name = "Gruppe2" }
        };
        var responseJson = JsonSerializer.Serialize(groupList);

        var (apiClient, handlerMock, _) = CreateClient(authToken, newToken, HttpStatusCode.OK, responseJson);

        var result = await apiClient.GetGroupsAsync(StorageMode.Team, "testowner");

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Gruppe1", result[0].Name);
        Assert.Equal("Gruppe2", result[1].Name);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsolutePath == "/api/application-groups" &&
                r.Method == HttpMethod.Get &&
                r.Headers.Contains("X-Storage-Mode") &&
                r.Headers.Contains("X-Owner")),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>GetUngroupedApplicationsAsync_SendsCorrectHeadersAndReturnsMappedList</summary>
    [Fact]
    public async Task GetUngroupedApplicationsAsync_SendsCorrectHeadersAndReturnsMappedList()
    {
        var authToken = Guid.NewGuid().ToString();
        var newToken = Guid.NewGuid().ToString();
        var appList = new List<ApplicationResponse>
        {
            new() { Id = 1, Name = "App1", BaseUrl = "https://app1.example.com" }
        };
        var responseJson = JsonSerializer.Serialize(appList);

        var (apiClient, handlerMock, _) = CreateClient(authToken, newToken, HttpStatusCode.OK, responseJson);

        var result = await apiClient.GetUngroupedApplicationsAsync(StorageMode.User, "testowner");

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("App1", result[0].Name);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsolutePath == "/api/applications/ungrouped" &&
                r.Method == HttpMethod.Get &&
                r.Headers.Contains("X-Storage-Mode") &&
                r.Headers.Contains("X-Owner")),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>GetApplicationByIdAsync_ReturnsNullOn404</summary>
    [Fact]
    public async Task GetApplicationByIdAsync_ReturnsNullOn404()
    {
        var authToken = Guid.NewGuid().ToString();
        var newToken = Guid.NewGuid().ToString();

        var (apiClient, _, _) = CreateClient(authToken, newToken, HttpStatusCode.NotFound, "null");

        var result = await apiClient.GetApplicationByIdAsync(999);

        Assert.Null(result);
    }

    /// <summary>UpdateGroupAsync_SendsCorrectPutRequestAndReturnsMappedGroup</summary>
    [Fact]
    public async Task UpdateGroupAsync_SendsCorrectPutRequestAndReturnsMappedGroup()
    {
        var authToken = Guid.NewGuid().ToString();
        var newToken = Guid.NewGuid().ToString();
        var groupResponse = new ApplicationGroupResponse { Id = 5, Name = "UmbenenntGruppe" };
        var responseJson = JsonSerializer.Serialize(groupResponse);

        var (apiClient, handlerMock, _) = CreateClient(authToken, newToken, HttpStatusCode.OK, responseJson);

        var group = new ApplicationGroup { Id = 5, Name = "UmbenenntGruppe" };
        var result = await apiClient.UpdateGroupAsync(group);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("UmbenenntGruppe", result.Name);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsolutePath == "/api/application-groups/5" &&
                r.Method == HttpMethod.Put &&
                r.Headers.Contains("X-Storage-Mode")),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>DeleteGroupAsync_SendsCorrectDeleteRequest</summary>
    [Fact]
    public async Task DeleteGroupAsync_SendsCorrectDeleteRequest()
    {
        var authToken = Guid.NewGuid().ToString();
        var newToken = Guid.NewGuid().ToString();

        var (apiClient, handlerMock, _) = CreateClient(authToken, newToken, HttpStatusCode.NoContent, "");

        await apiClient.DeleteGroupAsync(3);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsolutePath == "/api/application-groups/3" &&
                r.Method == HttpMethod.Delete),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>UpdateApplicationAsync_SendsCorrectPutRequestAndReturnsMappedApplication</summary>
    [Fact]
    public async Task UpdateApplicationAsync_SendsCorrectPutRequestAndReturnsMappedApplication()
    {
        var authToken = Guid.NewGuid().ToString();
        var newToken = Guid.NewGuid().ToString();
        var appResponse = new ApplicationResponse
        {
            Id = 7,
            Name = "UpdatedApp",
            BaseUrl = "https://updated.example.com",
            Description = "Neue Beschreibung",
            InterfaceUrl = "https://updated.example.com/swagger",
            InterfaceType = (int)InterfaceType.Rest,
            Owner = "owner1"
        };
        var responseJson = JsonSerializer.Serialize(appResponse);

        var (apiClient, handlerMock, _) = CreateClient(authToken, newToken, HttpStatusCode.OK, responseJson);

        var application = new Application
        {
            Id = 7,
            Name = "UpdatedApp",
            BaseUrl = "https://updated.example.com",
            Description = "Neue Beschreibung",
            InterfaceUrl = "https://updated.example.com/swagger",
            InterfaceType = InterfaceType.Rest,
            Owner = "owner1"
        };
        var result = await apiClient.UpdateApplicationAsync(application);

        Assert.NotNull(result);
        Assert.Equal(7, result.Id);
        Assert.Equal("UpdatedApp", result.Name);
        Assert.Equal("https://updated.example.com", result.BaseUrl);
        Assert.Equal("Neue Beschreibung", result.Description);
        Assert.Equal(InterfaceType.Rest, result.InterfaceType);
        Assert.Equal("owner1", result.Owner);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsolutePath == "/api/applications/7" &&
                r.Method == HttpMethod.Put &&
                r.Headers.Contains("X-Storage-Mode")),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>DeleteApplicationAsync_SendsCorrectDeleteRequest</summary>
    [Fact]
    public async Task DeleteApplicationAsync_SendsCorrectDeleteRequest()
    {
        var authToken = Guid.NewGuid().ToString();
        var newToken = Guid.NewGuid().ToString();

        var (apiClient, handlerMock, _) = CreateClient(authToken, newToken, HttpStatusCode.NoContent, "");

        await apiClient.DeleteApplicationAsync(42);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsolutePath == "/api/applications/42" &&
                r.Method == HttpMethod.Delete),
            ItExpr.IsAny<CancellationToken>());
    }
}
