using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Schnittstellenzentrale.Filters;

/// <summary>Fügt die Custom-Request-Header X-Storage-Mode und X-Owner zur Swagger-Operation hinzu.</summary>
public class ContextHeadersOperationFilter : IOperationFilter
{
    /// <inheritdoc/>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var attr = context.MethodInfo.GetCustomAttribute<RequiresContextHeadersAttribute>();
        if (attr == null)
            return;

        (operation.Parameters ??= []).Add(new OpenApiParameter
        {
            Name = "X-Storage-Mode",
            In = ParameterLocation.Header,
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Enum = [JsonValue.Create("Team"), JsonValue.Create("User")],
                Default = JsonValue.Create("Team")
            }
        });

        if (attr.IncludeOwner)
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Owner",
                In = ParameterLocation.Header,
                Required = true,
                Schema = new OpenApiSchema { Type = JsonSchemaType.String }
            });
    }
}
