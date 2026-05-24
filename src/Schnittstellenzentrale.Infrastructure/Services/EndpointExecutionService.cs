using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Helpers;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>
/// Führt einen Endpunkt unter Berücksichtigung der konfigurierten Authentifizierung aus
/// und liefert ein <see cref="EndpointExecutionResult"/> mit Statuscode, Antwortdaten und Laufzeitmetriken.
/// </summary>
public class EndpointExecutionService : IEndpointExecutionService
{
    private const string DefaultContentType = "application/json";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHealthCheckService _healthCheckService;
    private readonly ICredentialService _credentialService;

    /// <summary>Initialisiert eine neue Instanz des <see cref="EndpointExecutionService"/>.</summary>
    public EndpointExecutionService(
        IHttpClientFactory httpClientFactory,
        IHealthCheckService healthCheckService,
        ICredentialService credentialService)
    {
        _httpClientFactory = httpClientFactory;
        _healthCheckService = healthCheckService;
        _credentialService = credentialService;
    }

    /// <inheritdoc/>
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

        using var request = BuildRequest(endpoint);
        ApplyAuthentication(request, endpoint);
        return await SendAndBuildResultAsync(client, endpoint, request);
    }

    private async Task<EndpointExecutionResult> ExecuteImpersonatedAsync(Core.Models.Endpoint endpoint)
    {
        using var windowsIdentity = WindowsIdentity.GetCurrent();

        return await WindowsIdentity.RunImpersonatedAsync(windowsIdentity.AccessToken, async () =>
        {
            var client = _httpClientFactory.CreateClient("negotiate");
            using var request = BuildRequest(endpoint);
            return await SendAndBuildResultAsync(client, endpoint, request);
        });
    }

    private static async Task<EndpointExecutionResult> SendAndBuildResultAsync(
        HttpClient client,
        Core.Models.Endpoint endpoint,
        HttpRequestMessage request)
    {
        var stopwatch = Stopwatch.StartNew();
        using var response = await client.SendAsync(request);
        stopwatch.Stop();
        return await BuildResult(endpoint, response, stopwatch.ElapsedMilliseconds);
    }

    private static async Task<EndpointExecutionResult> BuildResult(Core.Models.Endpoint endpoint, HttpResponseMessage response, long durationMs)
    {
        var body = await response.Content.ReadAsStringAsync();
        var responseHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers)
            responseHeaders[header.Key] = string.Join(", ", header.Value);
        foreach (var header in response.Content.Headers)
            responseHeaders[header.Key] = string.Join(", ", header.Value);

        return new EndpointExecutionResult
        {
            Success = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            RequestDetails = $"{endpoint.Method} {endpoint.Application?.BaseUrl ?? string.Empty}{endpoint.RelativePath}",
            ResponseBody = body,
            ResponseHeaders = responseHeaders,
            DurationMs = durationMs,
            ResponseSizeBytes = Encoding.UTF8.GetByteCount(body)
        };
    }

    private HttpRequestMessage BuildRequest(Core.Models.Endpoint endpoint)
    {
        var resolvedPath = EndpointUrlBuilder.Resolve(
            endpoint.RelativePath,
            endpoint.QueryParameters.Select(p => (p.Key, p.Value)));

        var url = endpoint.Application.BaseUrl.TrimEnd('/') + "/" + resolvedPath.TrimStart('/');

        var method = endpoint.Method switch
        {
            Core.Enums.HttpMethod.GET => System.Net.Http.HttpMethod.Get,
            Core.Enums.HttpMethod.POST => System.Net.Http.HttpMethod.Post,
            Core.Enums.HttpMethod.PUT => System.Net.Http.HttpMethod.Put,
            Core.Enums.HttpMethod.DELETE => System.Net.Http.HttpMethod.Delete,
            Core.Enums.HttpMethod.PATCH => System.Net.Http.HttpMethod.Patch,
            Core.Enums.HttpMethod.HEAD => System.Net.Http.HttpMethod.Head,
            Core.Enums.HttpMethod.OPTIONS => System.Net.Http.HttpMethod.Options,
            _ => throw new ArgumentOutOfRangeException(nameof(endpoint.Method), endpoint.Method, null)
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
        var target = CredentialTargetHelper.Build(endpoint.ApplicationId, endpoint.AuthenticationType);

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

}
