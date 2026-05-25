using Jint;
using Jint.Native;
using Jint.Runtime;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Führt JavaScript-Skripte über Jint aus und stellt das <c>sz</c>-API-Objekt bereit.</summary>
public class EndpointScriptRunner : IEndpointScriptRunner
{
    private const int ScriptTimeoutMs = 5000;

    /// <inheritdoc/>
    public Task<ScriptExecutionResult> ExecuteAsync(string script, ScriptContext context)
    {
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
            return Task.FromResult(new ScriptExecutionResult
            {
                Success = false,
                ErrorMessage = $"Skript-Timeout nach {ScriptTimeoutMs} ms: {ex.Message}"
            });
        }
        catch (JavaScriptException ex)
        {
            return Task.FromResult(new ScriptExecutionResult
            {
                Success = false,
                ErrorMessage = $"JavaScript-Fehler: {ex.Message}"
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ScriptExecutionResult
            {
                Success = false,
                ErrorMessage = $"Skriptausführung fehlgeschlagen: {ex.Message}"
            });
        }
    }

    private static void RegisterSzObject(Engine engine, ScriptContext context)
    {
        var sz = new JsObject(engine);

        sz.FastSetDataProperty("environment", BuildEnvironmentObject(engine, context));
        sz.FastSetDataProperty("request", BuildRequestObject(engine, context.Request));

        if (context.Response != null)
            sz.FastSetDataProperty("response", BuildResponseObject(engine, context.Response));

        sz.FastSetDataProperty("execute", JsValue.FromObject(engine, (string name) =>
        {
            var result = Task.Run(() => context.ExecuteEndpoint(name)).GetAwaiter().GetResult();
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

    private static JsObject BuildEnvironmentObject(Engine engine, ScriptContext context)
    {
        var env = new JsObject(engine);

        env.FastSetDataProperty("get", JsValue.FromObject(engine, (string name) =>
        {
            context.EnvironmentService.ActiveVariables.TryGetValue(name, out var value);
            return value != null ? JsValue.FromObject(engine, value) : JsValue.Null;
        }));

        env.FastSetDataProperty("set", JsValue.FromObject(engine, (string name, string value) =>
        {
            var activeEnv = context.EnvironmentService.ActiveEnvironment;
            var updatedVariables = context.EnvironmentService.ActiveVariables
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            updatedVariables[name] = value;

            var updatedEnv = activeEnv != null
                ? new SystemEnvironment
                {
                    Id = activeEnv.Id,
                    Name = activeEnv.Name,
                    Mode = activeEnv.Mode,
                    Owner = activeEnv.Owner,
                    Variables = updatedVariables
                        .Select(kv => new EnvironmentVariable { Name = kv.Key, Value = kv.Value })
                        .ToList()
                }
                : new SystemEnvironment
                {
                    Name = string.Empty,
                    Variables = updatedVariables
                        .Select(kv => new EnvironmentVariable { Name = kv.Key, Value = kv.Value })
                        .ToList()
                };

            context.EnvironmentService.SetActiveEnvironment(updatedEnv);
            return JsValue.Undefined;
        }));

        return env;
    }

    private static JsObject BuildRequestObject(Engine engine, ScriptRequestData request)
    {
        var req = new JsObject(engine);

        req.FastSetDataProperty("url", JsValue.FromObject(engine, request.Url));
        req.FastSetDataProperty("method", JsValue.FromObject(engine, request.Method));
        req.FastSetDataProperty("body", BuildBodyObject(engine, request));

        var headersObj = new JsObject(engine);
        foreach (var header in request.Headers)
            headersObj.FastSetDataProperty(header.Key, JsValue.FromObject(engine, header.Value));
        req.FastSetDataProperty("headers", headersObj);

        return req;
    }

    private static JsObject BuildResponseObject(Engine engine, ScriptResponseData response)
    {
        var resp = new JsObject(engine);

        resp.FastSetDataProperty("body", BuildBodyObject(engine, response));

        var headersObj = new JsObject(engine);
        foreach (var header in response.Headers)
            headersObj.FastSetDataProperty(header.Key, JsValue.FromObject(engine, header.Value));
        resp.FastSetDataProperty("headers", headersObj);

        return resp;
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
