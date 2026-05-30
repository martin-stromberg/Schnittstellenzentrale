using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Schnittstellenzentrale.Services;

/// <summary>HTTP-basierter Client für die interne Application-API.</summary>
public class ApplicationApiClient : IApplicationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ITokenStore _tokenStore;
    private readonly IStorageModeService _storageModeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _currentToken;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ApplicationApiClient"/>.</summary>
    public ApplicationApiClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ITokenStore tokenStore,
        IStorageModeService storageModeService,
        ICurrentUserService currentUserService)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _tokenStore = tokenStore;
        _storageModeService = storageModeService;
        _currentUserService = currentUserService;
    }

    private string GetBaseUrl()
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx != null)
            return $"{ctx.Request.Scheme}://{ctx.Request.Host}";
        return _configuration["Api:BaseUrl"] ?? "https://localhost:5001";
    }

    /// <inheritdoc/>
    public async Task<IList<ApplicationGroup>> GetGroupsAsync(StorageMode storageMode, string owner)
    {
        var baseUrl = GetBaseUrl();
        var responses = await SendWithTokenAsync<IList<ApplicationGroupResponse>>(
            t => BuildGetRequest($"{baseUrl}/api/application-groups", storageMode, owner, t));

        return responses.Select(MapToApplicationGroup).ToList();
    }

    /// <inheritdoc/>
    public async Task<ApplicationGroup?> GetGroupByIdAsync(int id)
    {
        var baseUrl = GetBaseUrl();
        var response = await SendWithTokenNullableAsync<ApplicationGroupResponse>(
            t => BuildGetRequest($"{baseUrl}/api/application-groups/{id}", null, null, t));

        return response == null ? null : MapToApplicationGroup(response);
    }

    /// <inheritdoc/>
    public async Task<ApplicationGroup> AddGroupAsync(ApplicationGroup group)
    {
        var baseUrl = GetBaseUrl();
        var storageMode = _storageModeService.CurrentMode;
        var request = new CreateApplicationGroupRequest { Name = group.Name };

        var response = await SendWithTokenAsync<ApplicationGroupResponse>(
            t => BuildRequestWithBody(HttpMethod.Post, $"{baseUrl}/api/application-groups", request, storageMode, t));

        return MapToApplicationGroup(response);
    }

    /// <inheritdoc/>
    public async Task<ApplicationGroup> UpdateGroupAsync(ApplicationGroup group)
    {
        var baseUrl = GetBaseUrl();
        var storageMode = _storageModeService.CurrentMode;
        var request = new UpdateApplicationGroupRequest { Name = group.Name };

        var response = await SendWithTokenAsync<ApplicationGroupResponse>(
            t => BuildRequestWithBody(HttpMethod.Put, $"{baseUrl}/api/application-groups/{group.Id}", request, storageMode, t));

        return MapToApplicationGroup(response);
    }

    /// <inheritdoc/>
    public async Task DeleteGroupAsync(int id)
    {
        var baseUrl = GetBaseUrl();
        var storageMode = _storageModeService.CurrentMode;

        await SendWithTokenNoContentAsync(
            t => BuildDeleteRequest($"{baseUrl}/api/application-groups/{id}", storageMode, t));
    }

    /// <inheritdoc/>
    public async Task<IList<Application>> GetUngroupedApplicationsAsync(StorageMode storageMode, string owner)
    {
        var baseUrl = GetBaseUrl();
        var responses = await SendWithTokenAsync<IList<ApplicationResponse>>(
            t => BuildGetRequest($"{baseUrl}/api/applications/ungrouped", storageMode, owner, t));

        return responses.Select(MapToApplication).ToList();
    }

    /// <inheritdoc/>
    public async Task<Application?> GetApplicationByIdAsync(int id)
    {
        var baseUrl = GetBaseUrl();
        var response = await SendWithTokenNullableAsync<ApplicationResponse>(
            t => BuildGetRequest($"{baseUrl}/api/applications/{id}", null, null, t));

        return response == null ? null : MapToApplication(response);
    }

    /// <inheritdoc/>
    public async Task<Application> AddApplicationAsync(Application application)
    {
        var baseUrl = GetBaseUrl();
        var storageMode = _storageModeService.CurrentMode;
        var request = new CreateApplicationRequest
        {
            Name = application.Name,
            BaseUrl = application.BaseUrl,
            Description = application.Description,
            InterfaceUrl = application.InterfaceUrl,
            ApplicationGroupId = application.ApplicationGroupId,
            Owner = application.Owner
        };

        var response = await SendWithTokenAsync<ApplicationResponse>(
            t => BuildRequestWithBody(HttpMethod.Post, $"{baseUrl}/api/applications", request, storageMode, t));

        return MapToApplication(response);
    }

    /// <inheritdoc/>
    public async Task<Application> UpdateApplicationAsync(Application application)
    {
        var baseUrl = GetBaseUrl();
        var storageMode = _storageModeService.CurrentMode;
        var request = new UpdateApplicationRequest
        {
            Name = application.Name,
            BaseUrl = application.BaseUrl,
            Description = application.Description,
            InterfaceUrl = application.InterfaceUrl,
            ApplicationGroupId = application.ApplicationGroupId,
            Owner = application.Owner
        };

        var response = await SendWithTokenAsync<ApplicationResponse>(
            t => BuildRequestWithBody(HttpMethod.Put, $"{baseUrl}/api/applications/{application.Id}", request, storageMode, t));

        return MapToApplication(response);
    }

    /// <inheritdoc/>
    public async Task DeleteApplicationAsync(int id)
    {
        var baseUrl = GetBaseUrl();
        var storageMode = _storageModeService.CurrentMode;

        await SendWithTokenNoContentAsync(
            t => BuildDeleteRequest($"{baseUrl}/api/applications/{id}", storageMode, t));
    }

    private async Task<TResponse> SendWithTokenAsync<TResponse>(Func<string, HttpRequestMessage> buildRequest)
    {
        var response = await ExecuteWithTokenAsync(buildRequest);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>()
            ?? throw new InvalidOperationException("Response deserialization returned null.");
    }

    private async Task<TResponse?> SendWithTokenNullableAsync<TResponse>(Func<string, HttpRequestMessage> buildRequest)
        where TResponse : class
    {
        var response = await ExecuteWithTokenAsync(buildRequest);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    private async Task SendWithTokenNoContentAsync(Func<string, HttpRequestMessage> buildRequest)
    {
        var response = await ExecuteWithTokenAsync(buildRequest);
        response.EnsureSuccessStatusCode();
    }

    private async Task<HttpResponseMessage> ExecuteWithTokenAsync(Func<string, HttpRequestMessage> buildRequest)
    {
        await EnsureTokenAsync();

        string token;
        await _tokenLock.WaitAsync();
        try
        {
            token = _currentToken!;
        }
        finally
        {
            _tokenLock.Release();
        }

        var response = await _httpClient.SendAsync(buildRequest(token));

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _tokenLock.WaitAsync();
            try
            {
                _currentToken = null;
            }
            finally
            {
                _tokenLock.Release();
            }

            await EnsureTokenAsync();

            await _tokenLock.WaitAsync();
            try
            {
                token = _currentToken!;
            }
            finally
            {
                _tokenLock.Release();
            }

            response = await _httpClient.SendAsync(buildRequest(token));
        }

        if (response.Headers.TryGetValues("X-New-Token", out var tokenValues))
        {
            await _tokenLock.WaitAsync();
            try
            {
                _currentToken = tokenValues.FirstOrDefault();
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        return response;
    }

    private static HttpRequestMessage BuildGetRequest(string relativeUrl, StorageMode? storageMode, string? owner, string token)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, relativeUrl);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        if (storageMode.HasValue)
            httpRequest.Headers.Add("X-Storage-Mode", storageMode.Value.ToString());
        if (owner != null)
            httpRequest.Headers.Add("X-Owner", owner);
        return httpRequest;
    }

    private static HttpRequestMessage BuildRequestWithBody<TBody>(HttpMethod method, string relativeUrl, TBody body, StorageMode storageMode, string token)
    {
        var httpRequest = new HttpRequestMessage(method, relativeUrl);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        httpRequest.Headers.Add("X-Storage-Mode", storageMode.ToString());
        httpRequest.Content = JsonContent.Create(body);
        return httpRequest;
    }

    private static HttpRequestMessage BuildDeleteRequest(string relativeUrl, StorageMode storageMode, string token)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, relativeUrl);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        httpRequest.Headers.Add("X-Storage-Mode", storageMode.ToString());
        return httpRequest;
    }

    private async Task EnsureTokenAsync()
    {
        await _tokenLock.WaitAsync();
        try
        {
            if (!string.IsNullOrEmpty(_currentToken))
                return;

            var username = _currentUserService.GetCurrentUserName();
            var authToken = await _tokenStore.CreateTokenAsync(username);
            _currentToken = authToken.TokenValue;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private static ApplicationGroup MapToApplicationGroup(ApplicationGroupResponse response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        Description = response.Description,
        Subtitle = response.Subtitle,
        IconData = response.IconData,
        RowVersion = response.RowVersion,
        Applications = response.Applications.Select(MapToApplication).ToList()
    };

    private static Application MapToApplication(ApplicationResponse response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        BaseUrl = response.BaseUrl,
        ApplicationGroupId = response.ApplicationGroupId,
        Description = response.Description,
        InterfaceUrl = response.InterfaceUrl,
        InterfaceType = (Core.Enums.InterfaceType)response.InterfaceType,
        Owner = response.Owner,
        Subtitle = response.Subtitle,
        IconData = response.IconData,
        RowVersion = response.RowVersion
    };
}
