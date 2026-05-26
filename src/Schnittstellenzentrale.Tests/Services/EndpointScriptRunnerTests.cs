using Moq;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>Unit-Tests für <see cref="EndpointScriptRunner"/>.</summary>
public class EndpointScriptRunnerTests
{
    private static Mock<IActivityLogService> CreateActivityLogServiceMock()
    {
        return new Mock<IActivityLogService>();
    }

    private static EndpointScriptRunner CreateRunner(
        ISystemEnvironmentRepository? environmentRepository = null,
        ISignalRNotificationService? signalRNotificationService = null,
        IActivityLogService? activityLogService = null)
        => new(
            environmentRepository ?? CreateEnvironmentRepositoryMock().Object,
            signalRNotificationService ?? CreateSignalRNotificationServiceMock().Object,
            activityLogService ?? CreateActivityLogServiceMock().Object);

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

    private static Mock<ISystemEnvironmentRepository> CreateEnvironmentRepositoryMock()
    {
        var mock = new Mock<ISystemEnvironmentRepository>();
        mock.Setup(r => r.UpdateVariableAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    private static Mock<ISignalRNotificationService> CreateSignalRNotificationServiceMock()
    {
        var mock = new Mock<ISignalRNotificationService>();
        mock.Setup(s => s.NotifyEnvironmentChangedAsync()).Returns(Task.CompletedTask);
        return mock;
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
        var runner = CreateRunner();
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
        var runner = CreateRunner();
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
        var runner = CreateRunner();
        var envService = CreateEnvironmentServiceWithVariables(new Dictionary<string, string> { ["host"] = "example.com" });
        var context = CreateContext(envService: envService);

        var result = await runner.ExecuteAsync("var val = sz.environment.get('host'); if (val !== 'example.com') throw new Error('falsch: ' + val);", context);

        Assert.True(result.Success);
    }

    /// <summary>SzEnvironmentSet_AktualisiertActiveVariables</summary>
    [Fact]
    public async Task SzEnvironmentSet_AktualisiertActiveVariables()
    {
        var runner = CreateRunner();
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
        var runner = CreateRunner();
        var context = CreateContext();
        context.Request.Url = "http://example.com/api/test";

        var result = await runner.ExecuteAsync("if (sz.request.url !== 'http://example.com/api/test') throw new Error('URL falsch: ' + sz.request.url);", context);

        Assert.True(result.Success);
    }

    /// <summary>SzRequestBodyAsJson_ParstJsonKorrekt</summary>
    [Fact]
    public async Task SzRequestBodyAsJson_ParstJsonKorrekt()
    {
        var runner = CreateRunner();
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
        var runner = CreateRunner();
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
        var runner = CreateRunner();
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
        var runner = CreateRunner();
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

    /// <summary>SzEnvironmentSet_MitAktiverSystemumgebung_PersistiertVariable</summary>
    [Fact]
    public async Task SzEnvironmentSet_MitAktiverSystemumgebung_PersistiertVariable()
    {
        var repoMock = CreateEnvironmentRepositoryMock();
        var runner = CreateRunner(environmentRepository: repoMock.Object);
        var envService = CreateEnvironmentServiceWithVariables(new Dictionary<string, string> { ["host"] = "old" });
        var context = CreateContext(envService: envService);

        var result = await runner.ExecuteAsync("sz.environment.set('host', 'new');", context);

        Assert.True(result.Success);
        repoMock.Verify(r => r.UpdateVariableAsync(1, "host", "new"), Times.Once);
    }

    /// <summary>SzEnvironmentSet_OhneAktiveSystemumgebung_PersistiertNicht</summary>
    [Fact]
    public async Task SzEnvironmentSet_OhneAktiveSystemumgebung_PersistiertNicht()
    {
        var repoMock = CreateEnvironmentRepositoryMock();
        var runner = CreateRunner(environmentRepository: repoMock.Object);
        var envService = CreateEmptyEnvironmentService();
        var context = CreateContext(envService: envService);

        var result = await runner.ExecuteAsync("sz.environment.set('newVar', 'value');", context);

        Assert.True(result.Success);
        repoMock.Verify(r => r.UpdateVariableAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>SzEnvironmentSet_MitAktiverSystemumgebung_BenachrichtigtSignalR</summary>
    [Fact]
    public async Task SzEnvironmentSet_MitAktiverSystemumgebung_BenachrichtigtSignalR()
    {
        var signalRMock = CreateSignalRNotificationServiceMock();
        var runner = CreateRunner(signalRNotificationService: signalRMock.Object);
        var envService = CreateEnvironmentServiceWithVariables(new Dictionary<string, string> { ["key"] = "val" });
        var context = CreateContext(envService: envService);

        var result = await runner.ExecuteAsync("sz.environment.set('key', 'newval');", context);

        Assert.True(result.Success);
        signalRMock.Verify(s => s.NotifyEnvironmentChangedAsync(), Times.Once);
    }

    /// <summary>SzEnvironmentSet_UebernehmtIsValueMasked_AusBestehendenVariablen</summary>
    [Fact]
    public async Task SzEnvironmentSet_UebernehmtIsValueMasked_AusBestehendenVariablen()
    {
        var runner = CreateRunner();
        SystemEnvironment? capturedEnv = null;
        var mock = new Mock<IActiveEnvironmentService>();
        var env = new SystemEnvironment
        {
            Id = 5,
            Name = "Test",
            Variables = [new EnvironmentVariable { Id = 10, Name = "secret", Value = "old", IsValueMasked = true }]
        };
        mock.Setup(s => s.ActiveEnvironment).Returns(env);
        mock.Setup(s => s.ActiveVariables).Returns(new Dictionary<string, string> { ["secret"] = "old" });
        mock.Setup(s => s.SetActiveEnvironment(It.IsAny<SystemEnvironment?>()))
            .Callback<SystemEnvironment?>(e => capturedEnv = e);

        var context = CreateContext(envService: mock.Object);

        var result = await runner.ExecuteAsync("sz.environment.set('secret', 'new');", context);

        Assert.True(result.Success);
        Assert.NotNull(capturedEnv);
        var variable = capturedEnv!.Variables.FirstOrDefault(v => v.Name == "secret");
        Assert.NotNull(variable);
        Assert.True(variable!.IsValueMasked);
    }

    /// <summary>SzEnvironmentSet_UebernehmtId_AusBestehendenVariablen</summary>
    [Fact]
    public async Task SzEnvironmentSet_UebernehmtId_AusBestehendenVariablen()
    {
        var runner = CreateRunner();
        SystemEnvironment? capturedEnv = null;
        var mock = new Mock<IActiveEnvironmentService>();
        var env = new SystemEnvironment
        {
            Id = 5,
            Name = "Test",
            Variables = [new EnvironmentVariable { Id = 42, Name = "myvar", Value = "old", IsValueMasked = false }]
        };
        mock.Setup(s => s.ActiveEnvironment).Returns(env);
        mock.Setup(s => s.ActiveVariables).Returns(new Dictionary<string, string> { ["myvar"] = "old" });
        mock.Setup(s => s.SetActiveEnvironment(It.IsAny<SystemEnvironment?>()))
            .Callback<SystemEnvironment?>(e => capturedEnv = e);

        var context = CreateContext(envService: mock.Object);

        var result = await runner.ExecuteAsync("sz.environment.set('myvar', 'new');", context);

        Assert.True(result.Success);
        Assert.NotNull(capturedEnv);
        var variable = capturedEnv!.Variables.FirstOrDefault(v => v.Name == "myvar");
        Assert.NotNull(variable);
        Assert.Equal(42, variable!.Id);
    }

    /// <summary>SzEnvironmentSet_DatenbankFehler_GibtScriptExecutionResultMitFehler</summary>
    [Fact]
    public async Task SzEnvironmentSet_DatenbankFehler_GibtScriptExecutionResultMitFehler()
    {
        var repoMock = new Mock<ISystemEnvironmentRepository>();
        repoMock.Setup(r => r.UpdateVariableAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("DB-Fehler"));
        var runner = CreateRunner(environmentRepository: repoMock.Object);
        var envService = CreateEnvironmentServiceWithVariables(new Dictionary<string, string> { ["x"] = "1" });
        var context = CreateContext(envService: envService);

        var result = await runner.ExecuteAsync("sz.environment.set('x', '2');", context);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("DB-Fehler", result.ErrorMessage!);
    }

    /// <summary>SzEnvironmentSet_SignalRFehler_GibtScriptExecutionResultMitFehler</summary>
    [Fact]
    public async Task SzEnvironmentSet_SignalRFehler_GibtScriptExecutionResultMitFehler()
    {
        var signalRMock = new Mock<ISignalRNotificationService>();
        signalRMock.Setup(s => s.NotifyEnvironmentChangedAsync())
            .ThrowsAsync(new InvalidOperationException("SignalR-Fehler"));
        var runner = CreateRunner(signalRNotificationService: signalRMock.Object);
        var envService = CreateEnvironmentServiceWithVariables(new Dictionary<string, string> { ["x"] = "1" });
        var context = CreateContext(envService: envService);

        var result = await runner.ExecuteAsync("sz.environment.set('x', '2');", context);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("SignalR-Fehler", result.ErrorMessage!);
    }

    /// <summary>ExecuteAsync_ProtokolliertScriptExecuted</summary>
    [Fact]
    public async Task ExecuteAsync_ProtokolliertScriptExecuted()
    {
        var logMock = CreateActivityLogServiceMock();
        var runner = CreateRunner(activityLogService: logMock.Object);
        var context = CreateContext();
        context.EndpointName = "TestEndpunkt";

        await runner.ExecuteAsync("var x = 1;", context);

        logMock.Verify(l => l.Log(ActivityLogCategory.ScriptExecuted, It.Is<string>(m => m.Contains("TestEndpunkt")), It.IsAny<string?>()), Times.Once);
    }

    /// <summary>SzConsoleWrite_ProtokolliertScriptConsoleOutput</summary>
    [Fact]
    public async Task SzConsoleWrite_ProtokolliertScriptConsoleOutput()
    {
        var logMock = CreateActivityLogServiceMock();
        var runner = CreateRunner(activityLogService: logMock.Object);
        var context = CreateContext();

        await runner.ExecuteAsync("sz.console.write('Hallo Protokoll');", context);

        logMock.Verify(l => l.Log(ActivityLogCategory.ScriptConsoleOutput, "Hallo Protokoll", It.IsAny<string?>()), Times.Once);
    }

    /// <summary>ExecuteAsync_JavaScriptException_ProtokolliertInternalError</summary>
    [Fact]
    public async Task ExecuteAsync_JavaScriptException_ProtokolliertInternalError()
    {
        var logMock = CreateActivityLogServiceMock();
        var runner = CreateRunner(activityLogService: logMock.Object);
        var context = CreateContext();

        await runner.ExecuteAsync("throw new Error('Skriptfehler');", context);

        logMock.Verify(l => l.Log(ActivityLogCategory.InternalError, It.IsAny<string>(), It.IsAny<string?>()), Times.Once);
    }
}
