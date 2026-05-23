using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Schnittstellenzentrale.Filters;

/// <summary>Setzt pro Operation das passende Security-Scheme anhand des [Authorize]-Attributs.</summary>
public class SecurityOperationFilter : IOperationFilter
{
    /// <inheritdoc/>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAllowAnonymous =
            context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ||
            (context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ?? false);

        if (hasAllowAnonymous)
            return;

        var hasAuthorize =
            context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
            (context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ?? false);

        var schemeId = hasAuthorize ? "Negotiate" : "Bearer";

        (operation.Security ??= []).Add(new OpenApiSecurityRequirement
        {
            { new OpenApiSecuritySchemeReference(schemeId, null), new List<string>() }
        });
    }
}
