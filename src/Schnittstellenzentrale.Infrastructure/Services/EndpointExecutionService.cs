using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Helpers;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using ScriptType = Schnittstellenzentrale.Core.Enums.ScriptType;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>
/// Führt einen Endpunkt unter Berücksichtigung der konfigurierten Authentifizierung aus
/// und liefert ein <see cref="EndpointExecutionResult"/> mit Statuscode, Antwortdaten und Laufzeitmetriken.
/// </summary>
public class EndpointExecutionService : IEndpointExecutionService
{
    private const string DefaultContentType = "application/json";
    private const int MaxCallDepth = 2;
    private static readonly Regex DoubleBracePlaceholderRegex = new(@"\{\{([^}]+)\}\}", RegexOptions.Compiled);
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICredentialService _credentialService;
    private readonly IActiveEnvironmentService _activeEnvironmentService;
    private readonly IEndpointScriptRunner _scriptRunner;
    private readonly IEndpointRepository _endpointRepository;
    private readonly ISystemEnvironmentRepository _environmentRepository;
    private readonly ISignalRNotificationService _signalRNotificationService;
    private readonly IActivityLogService _activityLogService;
    private readonly IHistoryService _historyService;
    private readonly ILogger<EndpointExecutionService> _logger;

    /// <summary>Initialisiert eine neue Instanz des <see cref="EndpointExecutionService"/>.</summary>
    public EndpointExecutionService(
        IHttpClientFactory httpClientFactory,
        ICredentialService credentialService,
        IActiveEnvironmentService activeEnvironmentService,
        IEndpointScriptRunner scriptRunner,
        IEndpointRepository endpointRepository,
        ISystemEnvironmentRepository environmentRepository,
        ISignalRNotificationService signalRNotificationService,
        IActivityLogService activityLogService,
        IHistoryService historyService,
        ILogger<EndpointExecutionService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _credentialService = credentialService;
        _activeEnvironmentService = activeEnvironmentService;
        _scriptRunner = scriptRunner;
        _endpointRepository = endpointRepository;
        _environmentRepository = environmentRepository;
        _signalRNotificationService = signalRNotificationService;
        _activityLogService = activityLogService;
        _historyService = historyService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<EndpointExecutionResult> ExecuteAsync(Core.Models.Endpoint endpoint)
    {
        return await ExecuteAsync(endpoint, new Dictionary<int, int>());
    }

    private async Task<EndpointExecutionResult> ExecuteAsync(Core.Models.Endpoint endpoint, Dictionary<int, int> callDepth)
    {
        if (endpoint.Application == null)
            return new EndpointExecutionResult
            {
                Success = false,
                ErrorMessage = $"[Endpoint {endpoint.Id} {endpoint.RelativePath}] Application ist nicht geladen."
            };
        callDepth.TryGetValue(endpoint.Id, out var currentDepth);
        if (currentDepth >= MaxCallDepth)
            return new EndpointExecutionResult
            {
                Success = false,
                ErrorMessage = $"[Endpoint {endpoint.Id} {endpoint.RelativePath}] Rekursionsschutz: maximale Aufruftiefe erreicht."
            };

        callDepth[endpoint.Id] = currentDepth + 1;

        try
        {
            if (!string.IsNullOrEmpty(endpoint.PreRequestScript))
            {
                var preContext = BuildScriptContext(endpoint, callDepth, response: null, scriptType: ScriptType.PreRequest);
                var preResult = await _scriptRunner.ExecuteAsync(endpoint.PreRequestScript, preContext);
                if (!preResult.Success)
                    return new EndpointExecutionResult
                    {
                        Success = false,
                        ErrorMessage = preResult.ErrorMessage
                    };
            }

            EndpointExecutionResult result;
            try
            {
                if (endpoint.AuthenticationType == AuthenticationType.NegotiateWithImpersonation)
                    result = await ExecuteImpersonatedAsync(endpoint);
                else
                    result = await ExecuteWithAuthAsync(endpoint);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _activityLogService.Log(
                    ActivityLogCategory.InternalError,
                    $"[Endpoint {endpoint.Id} {endpoint.RelativePath}] {ex.Message}",
                    ex.ToString());
                return new EndpointExecutionResult
                {
                    Success = false,
                    ErrorMessage = $"[Endpoint {endpoint.Id} {endpoint.RelativePath}] {ex.Message}"
                };
            }

            if (!string.IsNullOrEmpty(endpoint.PostRequestScript))
            {
                var responseData = new ScriptResponseData
                {
                    Body = result.ResponseBody,
                    Headers = result.ResponseHeaders ?? new Dictionary<string, string>()
                };
                var postContext = BuildScriptContext(endpoint, callDepth, response: responseData, scriptType: ScriptType.PostRequest);
                var postResult = await _scriptRunner.ExecuteAsync(endpoint.PostRequestScript, postContext);
                if (!postResult.Success)
                {
                    result.Success = false;
                    result.ErrorMessage = string.IsNullOrEmpty(result.ErrorMessage)
                        ? postResult.ErrorMessage
                        : $"{result.ErrorMessage}\n{postResult.ErrorMessage}";
                }
            }

            var maskedVariables = _activeEnvironmentService.ActiveEnvironment?.Variables
                ?? Enumerable.Empty<EnvironmentVariable>();

            if (result.HttpSuccess && result.Success)
            {
                var requestDetails = result.RequestDetails ?? string.Empty;
                var responseDetails = result.ResponseBody ?? string.Empty;
                if (requestDetails.Length > 10240) requestDetails = requestDetails[..10240];
                if (responseDetails.Length > 10240) responseDetails = responseDetails[..10240];
                var details = MaskSensitiveData(
                    $"Request: {requestDetails}\nResponse: {responseDetails}",
                    maskedVariables);
                _activityLogService.Log(
                    ActivityLogCategory.EndpointExecuted,
                    MaskSensitiveData($"{result.RequestDetails} — {result.StatusCode}", maskedVariables),
                    details);
            }
            else if (result.StatusCode.HasValue && !result.HttpSuccess)
            {
                _activityLogService.Log(
                    ActivityLogCategory.HttpError,
                    MaskSensitiveData($"{result.RequestDetails} — {result.StatusCode}", maskedVariables));
            }

            if (!result.Success && result.HttpSuccess)
            {
                _activityLogService.Log(
                    ActivityLogCategory.InternalError,
                    MaskSensitiveData($"{result.RequestDetails} — Post-Script-Fehler: {result.ErrorMessage}", maskedVariables));
            }

            // Absichtlich identisch zur Log-Bedingung: History-Eintrag nur bei vollständig erfolgreichem Aufruf (HTTP + Post-Script).
            if (result.HttpSuccess && result.Success)
                await PersistHistoryEntryAsync(endpoint, result);

            return result;
        }
        finally
        {
            callDepth[endpoint.Id] = callDepth[endpoint.Id] - 1;
        }
    }

    private async Task PersistHistoryEntryAsync(Core.Models.Endpoint endpoint, EndpointExecutionResult result)
    {
        try
        {
            var entry = new Core.Models.EndpointCallHistoryEntry
            {
                ApplicationId = endpoint.ApplicationId,
                EndpointId = endpoint.Id,
                ExecutedAt = DateTime.UtcNow,
                HttpMethod = endpoint.Method.ToString(),
                RelativePath = endpoint.RelativePath,
                StatusCode = result.StatusCode,
                DurationMs = result.DurationMs.HasValue ? (int)Math.Min(result.DurationMs.Value, int.MaxValue) : null
            };
            await _historyService.AddEntryAsync(entry);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "History-Eintrag konnte nicht gespeichert werden.");
        }
    }

    private ScriptContext BuildScriptContext(
        Core.Models.Endpoint endpoint,
        Dictionary<int, int> callDepth,
        ScriptResponseData? response,
        ScriptType scriptType = ScriptType.PreRequest)
    {
        var variables = _activeEnvironmentService.ActiveVariables;
        var baseUrl = ResolvePlaceholders(endpoint.Application?.BaseUrl ?? string.Empty, variables);
        var relativePath = ResolvePlaceholders(endpoint.RelativePath, variables);

        var requestData = new ScriptRequestData
        {
            Url = CombineUrl(baseUrl, relativePath),
            Method = endpoint.Method.ToString(),
            Headers = endpoint.Headers.ToDictionary(h => h.Key, h => h.Value),
            Body = endpoint.Body
        };

        return new ScriptContext
        {
            EnvironmentService = _activeEnvironmentService,
            Request = requestData,
            Response = response,
            CallDepth = callDepth,
            ExecuteEndpoint = name => ExecuteEndpointByNameAsync(endpoint.ApplicationId, name, callDepth),
            EndpointName = endpoint.Name,
            ScriptType = scriptType
        };
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
        var resolvedUrl = request.RequestUri?.ToString() ?? string.Empty;
        var stopwatch = Stopwatch.StartNew();
        using var response = await client.SendAsync(request);
        stopwatch.Stop();
        return await BuildResult(endpoint, response, stopwatch.ElapsedMilliseconds, resolvedUrl);
    }

    private static async Task<EndpointExecutionResult> BuildResult(Core.Models.Endpoint endpoint, HttpResponseMessage response, long durationMs, string resolvedUrl)
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
            HttpSuccess = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            RequestDetails = $"{endpoint.Method} {resolvedUrl}",
            ResponseBody = body,
            ResponseHeaders = responseHeaders,
            DurationMs = durationMs,
            ResponseSizeBytes = Encoding.UTF8.GetByteCount(body)
        };
    }

    private HttpRequestMessage BuildRequest(Core.Models.Endpoint endpoint)
    {
        var variables = _activeEnvironmentService.ActiveVariables;

        var baseUrl = ResolvePlaceholders(endpoint.Application.BaseUrl, variables);
        var relativePath = ResolvePlaceholders(endpoint.RelativePath, variables);

        var resolvedQueryParameters = endpoint.QueryParameters
            .Select(p => (ResolvePlaceholders(p.Key, variables), ResolvePlaceholders(p.Value, variables)));

        var resolvedPath = EndpointUrlBuilder.Resolve(relativePath, resolvedQueryParameters);

        var url = CombineUrl(baseUrl, resolvedPath);

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
            request.Headers.TryAddWithoutValidation(
                ResolvePlaceholders(header.Key, variables),
                ResolvePlaceholders(header.Value, variables));

        var resolvedBody = string.IsNullOrEmpty(endpoint.Body)
            ? endpoint.Body
            : ResolvePlaceholders(endpoint.Body, variables);

        if (!string.IsNullOrEmpty(resolvedBody))
        {
            var contentType = endpoint.Headers
                .FirstOrDefault(h => h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                ?.Value ?? DefaultContentType;
            request.Content = new StringContent(resolvedBody, Encoding.UTF8, contentType);
        }

        return request;
    }

    private static string ResolvePlaceholders(string input, IReadOnlyDictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(input))
            return input ?? string.Empty;

        return DoubleBracePlaceholderRegex.Replace(input, match =>
        {
            var name = match.Groups[1].Value;
            return variables.TryGetValue(name, out var value) ? value : string.Empty;
        });
    }

    private async Task<EndpointExecutionResult> ExecuteEndpointByNameAsync(int applicationId, string name, Dictionary<int, int> callDepth)
    {
        var matches = await _endpointRepository.GetEndpointByNameAsync(applicationId, name);
        if (matches.Count == 0)
            return new EndpointExecutionResult
            {
                Success = false,
                ErrorMessage = $"sz.execute: Kein Endpunkt mit dem Namen \"{name}\" gefunden."
            };
        if (matches.Count > 1)
            return new EndpointExecutionResult
            {
                Success = false,
                ErrorMessage = $"sz.execute: Mehrdeutiger Endpunktname \"{name}\" — {matches.Count} Treffer gefunden."
            };
        return await ExecuteAsync(matches[0], callDepth);
    }

    private static string MaskSensitiveData(string text, IEnumerable<EnvironmentVariable> variables)
    {
        foreach (var variable in variables.Where(v => v.IsValueMasked && !string.IsNullOrEmpty(v.Value)))
        {
            text = text.Replace(variable.Value, "***", StringComparison.OrdinalIgnoreCase);
            var urlEncoded = Uri.EscapeDataString(variable.Value!);
            if (!string.Equals(urlEncoded, variable.Value, StringComparison.OrdinalIgnoreCase))
                text = text.Replace(urlEncoded, "***", StringComparison.OrdinalIgnoreCase);
        }
        return text;
    }

    private static string CombineUrl(string baseUrl, string relativePath)
        => baseUrl.TrimEnd('/') + "/" + relativePath.TrimStart('/');

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
                {
                    var resolvedToken = ResolvePlaceholders(token, _activeEnvironmentService.ActiveVariables);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", resolvedToken);
                }
                break;
        }
    }
}
