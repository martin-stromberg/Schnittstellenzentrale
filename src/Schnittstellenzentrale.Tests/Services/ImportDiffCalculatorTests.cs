using System.Text.Json;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>ImportDiffCalculatorTests</summary>
public class ImportDiffCalculatorTests
{
    /// <summary>Calculate_WhenPostRequestScriptDiffers_ReturnsChangedEndpoint</summary>
    [Fact]
    public void Calculate_WhenPostRequestScriptDiffers_ReturnsChangedEndpoint()
    {
        var existing = new List<Endpoint>
        {
            new() { Id = 1, Name = "getItems", Method = Core.Enums.HttpMethod.GET, RelativePath = "/items", ApplicationId = 1, PostRequestScript = "old" }
        };
        var imported = new List<Endpoint>
        {
            new() { Name = "getItems", Method = Core.Enums.HttpMethod.GET, RelativePath = "/items", ApplicationId = 1, PostRequestScript = "new" }
        };

        var diff = ImportDiffCalculator.Calculate(existing, imported);

        Assert.Contains(diff.ChangedEndpoints, e => e.RelativePath == "/items" && e.Method == Core.Enums.HttpMethod.GET);
    }

    /// <summary>Calculate_WhenPreRequestScriptDiffers_ReturnsChangedEndpoint</summary>
    [Fact]
    public void Calculate_WhenPreRequestScriptDiffers_ReturnsChangedEndpoint()
    {
        var existing = new List<Endpoint>
        {
            new() { Id = 1, Name = "getItems", Method = Core.Enums.HttpMethod.GET, RelativePath = "/items", ApplicationId = 1, PreRequestScript = "old" }
        };
        var imported = new List<Endpoint>
        {
            new() { Name = "getItems", Method = Core.Enums.HttpMethod.GET, RelativePath = "/items", ApplicationId = 1, PreRequestScript = "new" }
        };

        var diff = ImportDiffCalculator.Calculate(existing, imported);

        Assert.Contains(diff.ChangedEndpoints, e => e.RelativePath == "/items" && e.Method == Core.Enums.HttpMethod.GET);
    }

    /// <summary>Calculate_MergedEndpoint_ContainsScriptsFromImport</summary>
    [Fact]
    public void Calculate_MergedEndpoint_ContainsScriptsFromImport()
    {
        var existing = new List<Endpoint>
        {
            new() { Id = 1, Name = "getItems", Method = Core.Enums.HttpMethod.GET, RelativePath = "/items", ApplicationId = 1 }
        };
        var imported = new List<Endpoint>
        {
            new() { Name = "getItems", Method = Core.Enums.HttpMethod.GET, RelativePath = "/items", ApplicationId = 1, PreRequestScript = "pre", PostRequestScript = "post" }
        };

        var diff = ImportDiffCalculator.Calculate(existing, imported);

        var changed = diff.ChangedEndpoints.FirstOrDefault(e => e.RelativePath == "/items");
        Assert.NotNull(changed);
        Assert.Equal("pre", changed.PreRequestScript);
        Assert.Equal("post", changed.PostRequestScript);
    }

    /// <summary>Calculate_WhenImportedEndpointHasNullScripts_OverwritesExistingScripts</summary>
    [Fact]
    public void Calculate_WhenImportedEndpointHasNullScripts_OverwritesExistingScripts()
    {
        var existing = new List<Endpoint>
        {
            new() { Id = 1, Name = "getItems", Method = Core.Enums.HttpMethod.GET, RelativePath = "/items", ApplicationId = 1, PreRequestScript = "oldPre", PostRequestScript = "oldPost" }
        };
        var imported = new List<Endpoint>
        {
            new() { Name = "newName", Method = Core.Enums.HttpMethod.GET, RelativePath = "/items", ApplicationId = 1, PreRequestScript = null, PostRequestScript = null }
        };

        var diff = ImportDiffCalculator.Calculate(existing, imported);

        var changed = diff.ChangedEndpoints.FirstOrDefault(e => e.RelativePath == "/items");
        Assert.NotNull(changed);
        Assert.Null(changed.PreRequestScript);
        Assert.Null(changed.PostRequestScript);
    }

    /// <summary>Calculate_WhenExistingEndpointHasHeadersWithBackReference_ChangedEndpointIsSerializableWithoutCircularReference</summary>
    [Fact]
    public void Calculate_WhenExistingEndpointHasHeadersWithBackReference_ChangedEndpointIsSerializableWithoutCircularReference()
    {
        var existingEndpoint = new Endpoint { Id = 1, Name = "getItems", Method = Core.Enums.HttpMethod.GET, RelativePath = "/items", ApplicationId = 1 };
        var header = new EndpointHeader { Key = "X-Test", Value = "v", EndpointId = 1 };
        header.Endpoint = existingEndpoint; // simulates EF Core relationship fixup
        existingEndpoint.Headers.Add(header);

        var existing = new List<Endpoint> { existingEndpoint };
        var imported = new List<Endpoint>
        {
            new() { Name = "newName", Method = Core.Enums.HttpMethod.GET, RelativePath = "/items", ApplicationId = 1 }
        };

        var diff = ImportDiffCalculator.Calculate(existing, imported);

        var changed = diff.ChangedEndpoints.FirstOrDefault(e => e.RelativePath == "/items");
        Assert.NotNull(changed);
        Assert.Single(changed.Headers);
        Assert.Null(changed.Headers.First().Endpoint); // back-reference must not be copied
        var json = JsonSerializer.Serialize(diff); // must not throw
        Assert.NotNull(json);
    }
}
