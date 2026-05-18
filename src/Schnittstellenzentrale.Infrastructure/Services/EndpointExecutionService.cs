#pragma warning disable CS1591
using System.Net;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Services;

public class EndpointExecutionService : IEndpointExecutionService
{
    private const string DefaultContentType = "application/json";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHealthCheckService _healthCheckService;
    private readonly ICredentialService _credentialService;

    public EndpointExecutionService(
        IHttpClientFactory httpClientFactory,
        IHealthCheckService healthCheckService,
        ICredentialService credentialService)
    {
        _httpClientFactory = httpClientFactory;
        _healthCheckService = healthCheckService;
        _credentialService = credentialService;
    }

    public async Task<EndpointExecutionResult> ExecuteAsync(Core.Models.Endpoint endpoint)
    {
        if (endpoint.Application == null)
            return new EndpointExecutionResult
            {
                Success = false,
                ErrorMessage = $"[Endpoint {endpoint.Id} {endpoint.RelativePath}] Application ist nicht geladen."
            };

        try
        {
            if (endpoint.AuthenticationType == AuthenticationType.NegotiateWithImpersonation)
                return await ExecuteImpersonatedAsync(endpoint);

            return await ExecuteWithAuthAsync(endpoint);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new EndpointExecutionResult
            {
                Success = false,
                ErrorMessage = $"[Endpoint {endpoint.Id} {endpoint.RelativePath}] {ex.Message}"
            };
        }
    }

    private async Task<EndpointExecutionResult> ExecuteWithAuthAsync(Core.Models.Endpoint endpoint)
    {
        var client = endpoint.AuthenticationType == AuthenticationType.Negotiate
            ? _httpClientFactory.CreateClient("negotiate")
            : _httpClientFactory.CreateClient();

        return await SendAndBuildResultAsync(client, endpoint, applyAuthentication: true);
    }

    private async Task<EndpointExecutionResult> ExecuteImpersonatedAsync(Core.Models.Endpoint endpoint)
    {
        using var windowsIdentity = WindowsIdentity.GetCurrent();

        return await WindowsIdentity.RunImpersonatedAsync(windowsIdentity.AccessToken, async () =>
        {
            var client = _httpClientFactory.CreateClient("negotiate");
            return await SendAndBuildResultAsync(client, endpoint);
        });
    }

    private async Task<EndpointExecutionResult> SendAndBuildResultAsync(
        HttpClient client,
        Core.Models.Endpoint endpoint,
        bool applyAuthentication = false)
    {
        using var request = BuildRequest(endpoint);
        if (applyAuthentication)
            ApplyAuthentication(request, endpoint);

        using var response = await client.SendAsync(request);
        return await BuildResult(endpoint, response);
    }

    private static async Task<EndpointExecutionResult> BuildResult(Core.Models.Endpoint endpoint, HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return new EndpointExecutionResult
        {
            Success = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            RequestDetails = $"{endpoint.Method} {endpoint.Application?.BaseUrl ?? string.Empty}{endpoint.RelativePath}",
            ResponseBody = body
        };
    }

    private HttpRequestMessage BuildRequest(Core.Models.Endpoint endpoint)
    {
        var url = endpoint.Application.BaseUrl.TrimEnd('/') + "/" + endpoint.RelativePath.TrimStart('/');

        if (endpoint.QueryParameters.Any())
        {
            var query = string.Join("&", endpoint.QueryParameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
            url += "?" + query;
        }

        var method = endpoint.Method switch
        {
            Core.Enums.HttpMethod.GET => System.Net.Http.HttpMethod.Get,
            Core.Enums.HttpMethod.POST => System.Net.Http.HttpMethod.Post,
            Core.Enums.HttpMethod.PUT => System.Net.Http.HttpMethod.Put,
            Core.Enums.HttpMethod.DELETE => System.Net.Http.HttpMethod.Delete,
            Core.Enums.HttpMethod.PATCH => System.Net.Http.HttpMethod.Patch,
            Core.Enums.HttpMethod.HEAD => System.Net.Http.HttpMethod.Head,
            Core.Enums.HttpMethod.OPTIONS => System.Net.Http.HttpMethod.Options,
            _ => System.Net.Http.HttpMethod.Get
        };

        var request = new HttpRequestMessage(method, url);

        foreach (var header in endpoint.Headers)
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);

        if (!string.IsNullOrEmpty(endpoint.Body))
        {
            var contentType = endpoint.Headers
                .FirstOrDefault(h => h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                ?.Value ?? DefaultContentType;
            request.Content = new StringContent(endpoint.Body, Encoding.UTF8, contentType);
        }

        return request;
    }

    private void ApplyAuthentication(HttpRequestMessage request, Core.Models.Endpoint endpoint)
    {
        var target = BuildCredentialTarget(endpoint.ApplicationId, endpoint.AuthenticationType);

        switch (endpoint.AuthenticationType)
        {
            case AuthenticationType.Basic:
                var credentials = _credentialService.GetPassword(target);
                if (!string.IsNullOrEmpty(credentials))
                {
                    var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encoded);
                }
                break;

            case AuthenticationType.BearerToken:
                var token = _credentialService.GetPassword(target);
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                break;
        }
    }

    private static string BuildCredentialTarget(int applicationId, AuthenticationType authenticationType)
        => $"Schnittstellenzentrale:{applicationId}:{authenticationType}";
}
