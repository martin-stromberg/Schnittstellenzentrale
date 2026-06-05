using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Endpoint = Schnittstellenzentrale.Core.Models.Endpoint;
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
        var configured = _configuration["Api:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(configured))
            return configured;
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx != null)
            return $"{ctx.Request.Scheme}://{ctx.Request.Host}{ctx.Request.PathBase}";
        return "https://localhost:5001";
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

    /// <inheritdoc/>
    public async Task<IList<EndpointGroup>> GetEndpointGroupsAsync(int applicationId)
    {
        var baseUrl = GetBaseUrl();
        var storageMode = _storageModeService.CurrentMode;
        var responses = await SendWithTokenAsync<IList<EndpointGroupResponse>>(
            t => BuildGetRequest($"{baseUrl}/api/endpoint-groups", storageMode, null, t,
                new Dictionary<string, string> { ["applicationId"] = applicationId.ToString() }));

        return responses.Select(MapToEndpointGroup).ToList();
    }

    /// <inheritdoc/>
    public async Task<EndpointGroup?> GetEndpointGroupByIdAsync(int id)
    {
        var baseUrl = GetBaseUrl();
        var response = await SendWithTokenNullableAsync<EndpointGroupResponse>(
            t => BuildGetRequest($"{baseUrl}/api/endpoint-groups/{id}", null, null, t));

        return response == null ? null : MapToEndpointGroup(response);
    }

    /// <inheritdoc/>
    public async Task<EndpointGroup> AddEndpointGroupAsync(EndpointGroup group)
    {
        var baseUrl = GetBaseUrl();
        var storageMode = _storageModeService.CurrentMode;
        var request = new CreateEndpointGroupRequest
        {
            Name = group.Name,
            ApplicationId = group.ApplicationId,
            ParentGroupId = group.ParentGroupId
        };

        var response = await SendWithTokenAsync<EndpointGroupResponse>(
            t => BuildRequestWithBody(HttpMethod.Post, $"{baseUrl}/api/endpoint-groups", request, storageMode, t));

        return MapToEndpointGroup(response);
    }

    /// <inheritdoc/>
    public async Task<EndpointGroup> UpdateEndpointGroupAsync(EndpointGroup group)
    {
        var baseUrl = GetBaseUrl();
        var storageMode = _storageModeService.CurrentMode;
        var request = new UpdateEndpointGroupRequest
        {
            Name = group.Name,
            RowVersion = group.RowVersion
        };

        var response = await SendWithTokenAsync<EndpointGroupResponse>(
            t => BuildRequestWithBody(HttpMethod.Put, $"{baseUrl}/api/endpoint-groups/{group.Id}", request, storageMode, t));

        return MapToEndpointGroup(response);
    }

    /// <inheritdoc/>
    public async Task DeleteEndpointGroupAsync(int id)
    {
        var baseUrl = GetBaseUrl();
        var storageMode = _storageModeService.CurrentMode;

        await SendWithTokenNoContentAsync(
            t => BuildDeleteRequest($"{baseUrl}/api/endpoint-groups/{id}", storageMode, t));
    }

    /// <inheritdoc/>
    public async Task<IList<Endpoint>> GetEndpointsAsync(int applicationId, int? endpointGroupId = null)
    {
        var baseUrl = GetBaseUrl();
        var storageMode = _storageModeService.CurrentMode;
        var queryParams = new Dictionary<string, string> { ["applicationId"] = applicationId.ToString() };
        if (endpointGroupId.HasValue)
            queryParams["groupId"] = endpointGroupId.Value.ToString();

        var responses = await SendWithTokenAsync<IList<EndpointResponse>>(
            t => BuildGetRequest($"{baseUrl}/api/endpoints", storageMode, null, t, queryParams));

        return responses.Select(MapToEndpoint).ToList();
    }

    /// <inheritdoc/>
    public async Task<Endpoint?> GetEndpointByIdAsync(int id)
    {
        var baseUrl = GetBaseUrl();
        var response = await SendWithTokenNullableAsync<EndpointResponse>(
            t => BuildGetRequest($"{baseUrl}/api/endpoints/{id}", null, null, t));

        return response == null ? null : MapToEndpoint(response);
    }

    /// <inheritdoc/>
    public async Task<Endpoint> AddEndpointAsync(Endpoint endpoint)
    {
        var baseUrl = GetBaseUrl();
        var storageMode = _storageModeService.CurrentMode;
        var request = new CreateEndpointRequest
        {
            Name = endpoint.Name,
            RelativePath = endpoint.RelativePath,
            ApplicationId = endpoint.ApplicationId,
            EndpointGroupId = endpoint.EndpointGroupId,
            Method = endpoint.Method,
            BodyMode = endpoint.BodyMode,
            Body = endpoint.Body,
            AuthenticationType = endpoint.AuthenticationType,
            PreRequestScript = endpoint.PreRequestScript,
            PostRequestScript = endpoint.PostRequestScript
        };

        var response = await SendWithTokenAsync<EndpointResponse>(
            t => BuildRequestWithBody(HttpMethod.Post, $"{baseUrl}/api/endpoints", request, storageMode, t));

        return MapToEndpoint(response);
    }

    /// <inheritdoc/>
    public async Task<Endpoint> UpdateEndpointAsync(Endpoint endpoint)
    {
        var baseUrl = GetBaseUrl();
        var storageMode = _storageModeService.CurrentMode;
        var request = new UpdateEndpointRequest
        {
            Name = endpoint.Name,
            RelativePath = endpoint.RelativePath,
            EndpointGroupId = endpoint.EndpointGroupId,
            Method = endpoint.Method,
            BodyMode = endpoint.BodyMode,
            Body = endpoint.Body,
            AuthenticationType = endpoint.AuthenticationType,
            PreRequestScript = endpoint.PreRequestScript,
            PostRequestScript = endpoint.PostRequestScript,
            RowVersion = endpoint.RowVersion,
            Headers = [..endpoint.Headers.Select(h => new UpdateEndpointKeyValueItem { Key = h.Key, Value = h.Value })],
            QueryParameters = [..endpoint.QueryParameters.Select(q => new UpdateEndpointKeyValueItem { Key = q.Key, Value = q.Value })]
        };

        var response = await SendWithTokenAsync<EndpointResponse>(
            t => BuildRequestWithBody(HttpMethod.Put, $"{baseUrl}/api/endpoints/{endpoint.Id}", request, storageMode, t));

        return MapToEndpoint(response);
    }

    /// <inheritdoc/>
    public async Task DeleteEndpointAsync(int id)
    {
        var baseUrl = GetBaseUrl();
        var storageMode = _storageModeService.CurrentMode;

        await SendWithTokenNoContentAsync(
            t => BuildDeleteRequest($"{baseUrl}/api/endpoints/{id}", storageMode, t));
    }

    /// <inheritdoc/>
    public async Task<EndpointHeader> AddHeaderAsync(EndpointHeader header)
    {
        var baseUrl = GetBaseUrl();
        var storageMode = _storageModeService.CurrentMode;
        var request = new AddEndpointKeyValueRequest
        {
            Key = header.Key,
            Value = header.Value,
            EndpointId = header.EndpointId
        };

        var response = await SendWithTokenAsync<EndpointKeyValueResponse>(
            t => BuildRequestWithBody(HttpMethod.Post, $"{baseUrl}/api/endpoints/headers", request, storageMode, t));

        return MapToEndpointHeader(response);
    }

    /// <inheritdoc/>
    public async Task DeleteHeaderAsync(int id)
    {
        var baseUrl = GetBaseUrl();
        var storageMode = _storageModeService.CurrentMode;

        await SendWithTokenNoContentAsync(
            t => BuildDeleteRequest($"{baseUrl}/api/endpoints/headers/{id}", storageMode, t));
    }

    /// <inheritdoc/>
    public async Task<EndpointQueryParameter> AddQueryParameterAsync(EndpointQueryParameter parameter)
    {
        var baseUrl = GetBaseUrl();
        var storageMode = _storageModeService.CurrentMode;
        var request = new AddEndpointKeyValueRequest
        {
            Key = parameter.Key,
            Value = parameter.Value,
            EndpointId = parameter.EndpointId
        };

        var response = await SendWithTokenAsync<EndpointKeyValueResponse>(
            t => BuildRequestWithBody(HttpMethod.Post, $"{baseUrl}/api/endpoints/query-parameters", request, storageMode, t));

        return MapToEndpointQueryParameter(response);
    }

    /// <inheritdoc/>
    public async Task DeleteQueryParameterAsync(int id)
    {
        var baseUrl = GetBaseUrl();
        var storageMode = _storageModeService.CurrentMode;

        await SendWithTokenNoContentAsync(
            t => BuildDeleteRequest($"{baseUrl}/api/endpoints/query-parameters/{id}", storageMode, t));
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

        var token = await GetCurrentTokenAsync();
        var response = await _httpClient.SendAsync(buildRequest(token));

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await SetCurrentTokenAsync(null);

            await EnsureTokenAsync();

            token = await GetCurrentTokenAsync();
            response = await _httpClient.SendAsync(buildRequest(token));
        }

        if (response.Headers.TryGetValues("X-New-Token", out var tokenValues))
        {
            await SetCurrentTokenAsync(tokenValues.FirstOrDefault());
        }

        return response;
    }

    private static HttpRequestMessage BuildGetRequest(string baseUrl, StorageMode? storageMode, string? owner, string token, Dictionary<string, string>? queryParams = null)
    {
        var url = baseUrl;
        if (queryParams != null && queryParams.Count > 0)
        {
            var query = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            url = $"{baseUrl}?{query}";
        }
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        if (storageMode.HasValue)
            httpRequest.Headers.Add("X-Storage-Mode", storageMode.Value.ToString());
        if (owner != null)
            httpRequest.Headers.Add("X-Owner", Uri.EscapeDataString(owner));
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

    private async Task<string> GetCurrentTokenAsync()
    {
        await _tokenLock.WaitAsync();
        try
        {
            return _currentToken!;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task SetCurrentTokenAsync(string? token)
    {
        await _tokenLock.WaitAsync();
        try
        {
            _currentToken = token;
        }
        finally
        {
            _tokenLock.Release();
        }
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
        IsSystem = response.IsSystem,
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
        IsSystem = response.IsSystem,
        RowVersion = response.RowVersion
    };

    private static EndpointHeader MapToEndpointHeader(EndpointKeyValueResponse response) => new()
    {
        Id = response.Id,
        Key = response.Key,
        Value = response.Value,
        EndpointId = response.EndpointId
    };

    private static EndpointQueryParameter MapToEndpointQueryParameter(EndpointKeyValueResponse response) => new()
    {
        Id = response.Id,
        Key = response.Key,
        Value = response.Value,
        EndpointId = response.EndpointId
    };

    private static EndpointGroup MapToEndpointGroup(EndpointGroupResponse response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        ApplicationId = response.ApplicationId,
        ParentGroupId = response.ParentGroupId,
        RowVersion = response.RowVersion
    };

    private static Endpoint MapToEndpoint(EndpointResponse response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        Method = response.Method,
        RelativePath = response.RelativePath,
        Body = response.Body,
        BodyMode = response.BodyMode,
        AuthenticationType = response.AuthenticationType,
        ApplicationId = response.ApplicationId,
        EndpointGroupId = response.EndpointGroupId,
        RowVersion = response.RowVersion,
        PreRequestScript = response.PreRequestScript,
        PostRequestScript = response.PostRequestScript,
        Headers = response.Headers.Select(MapToEndpointHeader).ToList(),
        QueryParameters = response.QueryParameters.Select(MapToEndpointQueryParameter).ToList()
    };
}
