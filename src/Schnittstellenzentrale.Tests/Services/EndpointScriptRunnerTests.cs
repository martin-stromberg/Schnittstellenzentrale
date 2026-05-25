using Moq;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>Unit-Tests für <see cref="EndpointScriptRunner"/>.</summary>
public class EndpointScriptRunnerTests
{
    private static ScriptContext CreateContext(
        IActiveEnvironmentService? envService = null,
        ScriptResponseData? response = null)
    {
        var envMock = envService ?? CreateEmptyEnvironmentService();
        return new ScriptContext
        {
            EnvironmentService = envMock,
            Request = new ScriptRequestData
            {
                Url = "http://example.com/api/test",
                Method = "GET",
                Headers = new Dictionary<string, string>(),
                Body = null
            },
            Response = response,
            CallDepth = new Dictionary<int, int>(),
            ExecuteEndpoint = _ => Task.FromResult(new EndpointExecutionResult { Success = true })
        };
    }

    private static IActiveEnvironmentService CreateEmptyEnvironmentService()
    {
        var mock = new Mock<IActiveEnvironmentService>();
        mock.Setup(s => s.ActiveEnvironment).Returns((SystemEnvironment?)null);
        mock.Setup(s => s.ActiveVariables).Returns(new Dictionary<string, string>());
        return mock.Object;
    }

    private static IActiveEnvironmentService CreateEnvironmentServiceWithVariables(Dictionary<string, string> variables)
    {
        var mock = new Mock<IActiveEnvironmentService>();
        var env = new SystemEnvironment
        {
            Id = 1,
            Name = "Test",
            Variables = variables.Select(kv => new EnvironmentVariable { Name = kv.Key, Value = kv.Value }).ToList()
        };
        mock.Setup(s => s.ActiveEnvironment).Returns(env);
        mock.Setup(s => s.ActiveVariables).Returns(variables);
        return mock.Object;
    }

    /// <summary>Syntaxfehler_GibtScriptExecutionResultMitErrorMessage</summary>
    [Fact]
    public async Task Syntaxfehler_GibtScriptExecutionResultMitErrorMessage()
    {
        var runner = new EndpointScriptRunner();
        var context = CreateContext();

        var result = await runner.ExecuteAsync("this is not valid javascript @@@", context);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.NotEmpty(result.ErrorMessage!);
    }

    /// <summary>RuntimeException_GibtScriptExecutionResultMitErrorMessage</summary>
    [Fact]
    public async Task RuntimeException_GibtScriptExecutionResultMitErrorMessage()
    {
        var runner = new EndpointScriptRunner();
        var context = CreateContext();

        var result = await runner.ExecuteAsync("throw new Error('Testfehler');", context);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("Testfehler", result.ErrorMessage!);
    }

    /// <summary>SzEnvironmentGet_LiestVariableAusActiveVariables</summary>
    [Fact]
    public async Task SzEnvironmentGet_LiestVariableAusActiveVariables()
    {
        var runner = new EndpointScriptRunner();
        var envService = CreateEnvironmentServiceWithVariables(new Dictionary<string, string> { ["host"] = "example.com" });
        var context = CreateContext(envService: envService);

        var result = await runner.ExecuteAsync("var val = sz.environment.get('host'); if (val !== 'example.com') throw new Error('falsch: ' + val);", context);

        Assert.True(result.Success);
    }

    /// <summary>SzEnvironmentSet_AktualisiertActiveVariables</summary>
    [Fact]
    public async Task SzEnvironmentSet_AktualisiertActiveVariables()
    {
        var runner = new EndpointScriptRunner();
        SystemEnvironment? capturedEnv = null;
        var mock = new Mock<IActiveEnvironmentService>();
        mock.Setup(s => s.ActiveEnvironment).Returns((SystemEnvironment?)null);
        mock.Setup(s => s.ActiveVariables).Returns(new Dictionary<string, string>());
        mock.Setup(s => s.SetActiveEnvironment(It.IsAny<SystemEnvironment?>()))
            .Callback<SystemEnvironment?>(env => capturedEnv = env);
        var context = CreateContext(envService: mock.Object);

        var result = await runner.ExecuteAsync("sz.environment.set('newVar', 'newValue');", context);

        Assert.True(result.Success);
        mock.Verify(s => s.SetActiveEnvironment(It.IsAny<SystemEnvironment?>()), Times.Once);
        Assert.NotNull(capturedEnv);
        Assert.Contains(capturedEnv!.Variables, v => v.Name == "newVar" && v.Value == "newValue");
    }

    /// <summary>SzRequestUrl_GibtKorrekteUrlZurueck</summary>
    [Fact]
    public async Task SzRequestUrl_GibtKorrekteUrlZurueck()
    {
        var runner = new EndpointScriptRunner();
        var context = CreateContext();
        context.Request.Url = "http://example.com/api/test";

        var result = await runner.ExecuteAsync("if (sz.request.url !== 'http://example.com/api/test') throw new Error('URL falsch: ' + sz.request.url);", context);

        Assert.True(result.Success);
    }

    /// <summary>SzRequestBodyAsJson_ParstJsonKorrekt</summary>
    [Fact]
    public async Task SzRequestBodyAsJson_ParstJsonKorrekt()
    {
        var runner = new EndpointScriptRunner();
        var context = CreateContext();
        context.Request.Body = """{"key":"value","num":7}""";

        var result = await runner.ExecuteAsync(
            "var obj = sz.request.body.asJson(); if (obj.key !== 'value') throw new Error('key falsch'); if (obj.num !== 7) throw new Error('num falsch');",
            context);

        Assert.True(result.Success);
    }

    /// <summary>SzRequestBodyAsXml_ParstXmlKorrekt</summary>
    [Fact]
    public async Task SzRequestBodyAsXml_ParstXmlKorrekt()
    {
        var runner = new EndpointScriptRunner();
        var context = CreateContext();
        context.Request.Body = "<data><item>hello</item></data>";

        var result = await runner.ExecuteAsync(
            "var obj = sz.request.body.asXml(); if (obj.item !== 'hello') throw new Error('item falsch: ' + obj.item);",
            context);

        Assert.True(result.Success);
    }

    /// <summary>SzResponseBodyAsJson_ParstJsonKorrekt</summary>
    [Fact]
    public async Task SzResponseBodyAsJson_ParstJsonKorrekt()
    {
        var runner = new EndpointScriptRunner();
        var response = new ScriptResponseData
        {
            Body = """{"status":"ok","count":42}""",
            Headers = new Dictionary<string, string>()
        };
        var context = CreateContext(response: response);

        var result = await runner.ExecuteAsync(
            "var obj = sz.response.body.asJson(); if (obj.status !== 'ok') throw new Error('status falsch'); if (obj.count !== 42) throw new Error('count falsch');",
            context);

        Assert.True(result.Success);
    }

    /// <summary>SzResponseBodyAsXml_ParstXmlKorrekt</summary>
    [Fact]
    public async Task SzResponseBodyAsXml_ParstXmlKorrekt()
    {
        var runner = new EndpointScriptRunner();
        var response = new ScriptResponseData
        {
            Body = "<root><name>test</name></root>",
            Headers = new Dictionary<string, string>()
        };
        var context = CreateContext(response: response);

        var result = await runner.ExecuteAsync(
            "var obj = sz.response.body.asXml(); if (obj.name !== 'test') throw new Error('name falsch: ' + obj.name);",
            context);

        Assert.True(result.Success);
    }
}
