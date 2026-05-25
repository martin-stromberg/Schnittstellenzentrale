using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Schnittstellenzentrale.Filters;

/// <summary>Fügt x-sz-*-Erweiterungsfelder zur Swagger-Operation hinzu, damit der Swagger-Import automatisch Skripte und Bearer-Token setzt.</summary>
public class SzExtensionsOperationFilter : IOperationFilter
{
    private const string PostRequestScriptKey = "x-sz-post-request-script";
    private const string BearerTokenKey = "x-sz-bearer-token";

    private const string AuthenticateScript =
        "sz.environment.set('schnittstellenzentrale.authToken', sz.response.body.asJson().token);";

    private const string TokenRefreshScript =
        "var headerName = 'X-New-Token';\nvar newToken = sz.response.headers[headerName];\nsz.environment.set('schnittstellenzentrale.authToken', newToken);";

    /// <inheritdoc/>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath ?? string.Empty;
        var method = context.ApiDescription.HttpMethod ?? string.Empty;

        if (path.Equals("authenticate", StringComparison.OrdinalIgnoreCase)
            && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            operation.Extensions[PostRequestScriptKey] = new JsonNodeExtension(JsonValue.Create(AuthenticateScript));
        }
        else
        {
            operation.Extensions[BearerTokenKey] = new JsonNodeExtension(JsonValue.Create("{{schnittstellenzentrale.authToken}}"));
            operation.Extensions[PostRequestScriptKey] = new JsonNodeExtension(JsonValue.Create(TokenRefreshScript));
        }
    }
}
