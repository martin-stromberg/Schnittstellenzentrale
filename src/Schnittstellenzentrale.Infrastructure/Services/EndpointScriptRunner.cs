using Jint;
using Jint.Native;
using Jint.Runtime;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Führt JavaScript-Skripte über Jint aus und stellt das <c>sz</c>-API-Objekt bereit.</summary>
public class EndpointScriptRunner : IEndpointScriptRunner
{
    private const int ScriptTimeoutMs = 5000;
    private readonly ISystemEnvironmentRepository _environmentRepository;
    private readonly ISignalRNotificationService _signalRNotificationService;
    private readonly IActivityLogService _activityLogService;

    /// <summary>Initialisiert eine neue Instanz des <see cref="EndpointScriptRunner"/>.</summary>
    public EndpointScriptRunner(
        ISystemEnvironmentRepository environmentRepository,
        ISignalRNotificationService signalRNotificationService,
        IActivityLogService activityLogService)
    {
        _environmentRepository = environmentRepository;
        _signalRNotificationService = signalRNotificationService;
        _activityLogService = activityLogService;
    }

    /// <inheritdoc/>
    public Task<ScriptExecutionResult> ExecuteAsync(string script, ScriptContext context)
    {
        var scriptName = !string.IsNullOrEmpty(context.EndpointName)
            ? context.EndpointName
            : "Skript";
        _activityLogService.Log(ActivityLogCategory.ScriptExecuted, $"Skript ausgeführt: {scriptName}");

        try
        {
            var engine = new Engine(options =>
            {
                options.TimeoutInterval(TimeSpan.FromMilliseconds(ScriptTimeoutMs));
                options.LimitMemory(4 * 1024 * 1024);
            });

            RegisterSzObject(engine, context);

            engine.Execute(script);

            return Task.FromResult(new ScriptExecutionResult { Success = true });
        }
        catch (TimeoutException ex)
        {
            _activityLogService.Log(
                ActivityLogCategory.InternalError,
                $"Skript-Timeout in Skript: {scriptName}",
                ex.ToString());
            return Task.FromResult(new ScriptExecutionResult
            {
                Success = false,
                ErrorMessage = $"Skript-Timeout nach {ScriptTimeoutMs} ms: {ex.Message}"
            });
        }
        catch (JavaScriptException ex)
        {
            _activityLogService.Log(
                ActivityLogCategory.InternalError,
                $"JavaScript-Fehler in Skript: {scriptName}",
                ex.ToString());
            return Task.FromResult(new ScriptExecutionResult
            {
                Success = false,
                ErrorMessage = $"JavaScript-Fehler: {ex.Message}"
            });
        }
        catch (Exception ex)
        {
            _activityLogService.Log(
                ActivityLogCategory.InternalError,
                $"Skriptausführung fehlgeschlagen: {scriptName}",
                ex.ToString());
            return Task.FromResult(new ScriptExecutionResult
            {
                Success = false,
                ErrorMessage = $"Skriptausführung fehlgeschlagen: {ex.Message}"
            });
        }
    }

    private void RegisterSzObject(Engine engine, ScriptContext context)
    {
        var sz = new JsObject(engine);

        sz.FastSetDataProperty("environment", BuildEnvironmentObject(engine, context));
        sz.FastSetDataProperty("request", BuildRequestObject(engine, context.Request));

        if (context.Response != null)
            sz.FastSetDataProperty("response", BuildResponseObject(engine, context.Response));

        var console = new JsObject(engine);
        console.FastSetDataProperty("write", JsValue.FromObject(engine, (string text) =>
        {
            _activityLogService.Log(ActivityLogCategory.ScriptConsoleOutput, text);
            return JsValue.Undefined;
        }));
        sz.FastSetDataProperty("console", console);

        sz.FastSetDataProperty("execute", JsValue.FromObject(engine, (string name) =>
        {
            EndpointExecutionResult result;
            try
            {
                // Jint callbacks sind synchron — await ist nicht möglich. Task.Run + GetAwaiter().GetResult()
                // ist das etablierte Muster, um async-Methoden aus synchronen Jint-Lambdas heraus zu blockieren.
                result = Task.Run(() => context.ExecuteEndpoint(name)).GetAwaiter().GetResult();
            }
            catch (AggregateException aggEx)
            {
                var inner = aggEx.GetBaseException();
                result = new EndpointExecutionResult
                {
                    Success = false,
                    ErrorMessage = $"sz.execute fehlgeschlagen: {inner.Message}"
                };
            }
            var resultObj = new JsObject(engine);
            resultObj.FastSetDataProperty("success", result.Success ? JsBoolean.True : JsBoolean.False);
            resultObj.FastSetDataProperty("statusCode", result.StatusCode.HasValue
                ? JsValue.FromObject(engine, result.StatusCode.Value)
                : JsValue.Null);
            resultObj.FastSetDataProperty("responseBody", result.ResponseBody != null
                ? JsValue.FromObject(engine, result.ResponseBody)
                : JsValue.Null);
            resultObj.FastSetDataProperty("errorMessage", result.ErrorMessage != null
                ? JsValue.FromObject(engine, result.ErrorMessage)
                : JsValue.Null);
            return resultObj;
        }));

        engine.SetValue("sz", sz);
    }

    private JsObject BuildEnvironmentObject(Engine engine, ScriptContext context)
    {
        var env = new JsObject(engine);

        env.FastSetDataProperty("get", JsValue.FromObject(engine, (string name) =>
        {
            context.EnvironmentService.ActiveVariables.TryGetValue(name, out var value);
            return value != null ? JsValue.FromObject(engine, value) : JsValue.Null;
        }));

        env.FastSetDataProperty("set", JsValue.FromObject(engine, (string name, string value) =>
        {
            try
            {
                ApplyEnvironmentSet(context, name, value);
            }
            catch (Exception ex)
            {
                throw new JavaScriptException($"sz.environment.set fehlgeschlagen: {ex.Message}");
            }
            return JsValue.Undefined;
        }));

        return env;
    }

    private void ApplyEnvironmentSet(ScriptContext context, string name, string value)
    {
        var activeEnv = context.EnvironmentService.ActiveEnvironment;
        var baseVariables = activeEnv?.Variables ?? [];
        var updatedVariables = baseVariables
            .Select(v => new EnvironmentVariable
            {
                Id = v.Id,
                Name = v.Name,
                Value = v.Name == name ? value : v.Value,
                IsValueMasked = v.IsValueMasked
            })
            .ToList();

        if (!baseVariables.Any(v => v.Name == name))
            updatedVariables.Add(new EnvironmentVariable { Id = 0, Name = name, Value = value, IsValueMasked = false });

        var updatedEnv = activeEnv != null
            ? new SystemEnvironment
            {
                Id = activeEnv.Id,
                Name = activeEnv.Name,
                Mode = activeEnv.Mode,
                Owner = activeEnv.Owner,
                Variables = updatedVariables
            }
            : new SystemEnvironment
            {
                Name = string.Empty,
                Variables = updatedVariables
            };

        context.EnvironmentService.SetActiveEnvironment(updatedEnv);

        if (activeEnv != null)
        {
            // Jint callbacks sind synchron — await ist nicht möglich. Task.Run + GetAwaiter().GetResult()
            // ist das etablierte Muster, um async-Methoden aus synchronen Jint-Lambdas heraus zu blockieren.
            Task.Run(() => PersistVariableAsync(activeEnv.Id, name, value)).GetAwaiter().GetResult();
        }
    }

    private async Task PersistVariableAsync(int environmentId, string name, string value)
    {
        await _environmentRepository.UpdateVariableAsync(environmentId, name, value);
        await _signalRNotificationService.NotifyEnvironmentChangedAsync();
    }

    private static JsObject BuildRequestObject(Engine engine, ScriptRequestData request)
    {
        var req = new JsObject(engine);

        req.FastSetDataProperty("url", JsValue.FromObject(engine, request.Url));
        req.FastSetDataProperty("method", JsValue.FromObject(engine, request.Method));
        req.FastSetDataProperty("body", BuildBodyObject(engine, request));
        req.FastSetDataProperty("headers", BuildHeadersObject(engine, request.Headers));

        return req;
    }

    private static JsObject BuildResponseObject(Engine engine, ScriptResponseData response)
    {
        var resp = new JsObject(engine);

        resp.FastSetDataProperty("body", BuildBodyObject(engine, response));
        resp.FastSetDataProperty("headers", BuildHeadersObject(engine, response.Headers));

        return resp;
    }

    private static JsObject BuildHeadersObject(Engine engine, IEnumerable<KeyValuePair<string, string>> headers)
    {
        var headersObj = new JsObject(engine);
        foreach (var header in headers)
            headersObj.FastSetDataProperty(header.Key, JsValue.FromObject(engine, header.Value));
        return headersObj;
    }

    private static JsObject BuildBodyObject(Engine engine, ScriptRequestData request)
        => BuildBodyObjectCore(engine, () => request.AsJson(), () => request.AsXml(), request.Body);

    private static JsObject BuildBodyObject(Engine engine, ScriptResponseData response)
        => BuildBodyObjectCore(engine, () => response.AsJson(), () => response.AsXml(), response.Body);

    private static JsObject BuildBodyObjectCore(Engine engine, Func<object?> getJson, Func<object?> getXml, string? rawValue)
    {
        var body = new JsObject(engine);

        body.FastSetDataProperty("asJson", JsValue.FromObject(engine, () =>
        {
            var parsed = getJson();
            return parsed != null ? JsValue.FromObject(engine, parsed) : JsValue.Null;
        }));

        body.FastSetDataProperty("asXml", JsValue.FromObject(engine, () =>
        {
            var parsed = getXml();
            return parsed != null ? JsValue.FromObject(engine, parsed) : JsValue.Null;
        }));

        body.FastSetDataProperty("raw", rawValue != null
            ? JsValue.FromObject(engine, rawValue)
            : JsValue.Null);

        return body;
    }
}
