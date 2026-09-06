using Bunit;
using Microsoft.AspNetCore.Components;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für das Formatierungs- und Modusverhalten von <see cref="RequestBodyPanel"/>.</summary>
public class RequestBodyPanelTests : BunitContext
{
    /// <summary>Der Formatieren-Button ist bei <see cref="BodyMode.PlainText"/> deaktiviert, da Text nicht formatierbar ist.</summary>
    [Fact]
    public void FormatButton_PlainText_IstDeaktiviert()
    {
        var cut = Render<RequestBodyPanel>(parameters => parameters
            .Add(x => x.BodyMode, BodyMode.PlainText)
            .Add(x => x.Body, "einfacher Text"));

        var formatButton = cut.Find("button.sz-btn-secondary");

        Assert.True(formatButton.HasAttribute("disabled"));
    }

    /// <summary>Der Formatieren-Button ist bei <see cref="BodyMode.Xml"/> aktiviert.</summary>
    [Fact]
    public void FormatButton_Xml_IstAktiviert()
    {
        var cut = Render<RequestBodyPanel>(parameters => parameters
            .Add(x => x.BodyMode, BodyMode.Xml)
            .Add(x => x.Body, "<root><a>1</a></root>"));

        var formatButton = cut.Find("button.sz-btn-secondary");

        Assert.False(formatButton.HasAttribute("disabled"));
    }

    /// <summary>Formatieren im <see cref="BodyMode.Xml"/>-Modus formatiert den XML-Body und meldet ihn über <c>BodyChanged</c>.</summary>
    [Fact]
    public async Task FormatBody_Xml_FormatiertBody_UndMeldetAenderung()
    {
        string? changedBody = null;
        var cut = Render<RequestBodyPanel>(parameters => parameters
            .Add(x => x.BodyMode, BodyMode.Xml)
            .Add(x => x.Body, "<root><a>1</a></root>")
            .Add(x => x.BodyChanged, EventCallback.Factory.Create<string?>(this, value => changedBody = value)));

        var formatButton = cut.Find("button.sz-btn-secondary");
        await cut.InvokeAsync(() => formatButton.Click());

        Assert.NotNull(changedBody);
        Assert.Contains("\n", changedBody);
        Assert.Contains("<a>1</a>", changedBody);
    }

    /// <summary>Formatieren mit ungültigem XML zeigt eine Fehlermeldung im Panel an.</summary>
    [Fact]
    public async Task FormatBody_Xml_UngueltigesXml_ZeigtFehlermeldung()
    {
        var cut = Render<RequestBodyPanel>(parameters => parameters
            .Add(x => x.BodyMode, BodyMode.Xml)
            .Add(x => x.Body, "<root><a>1</root>"));

        var formatButton = cut.Find("button.sz-btn-secondary");
        await cut.InvokeAsync(() => formatButton.Click());

        var error = cut.Find("div.sz-error");
        Assert.Contains("Formatieren fehlgeschlagen", error.TextContent);
    }

    /// <summary>Die Auswahl eines Modus im Dropdown löst <c>BodyModeChanged</c> mit dem gewählten <see cref="BodyMode"/> aus.</summary>
    /// <param name="selectedValue">Der im Dropdown gewählte Wert.</param>
    /// <param name="expected">Der erwartete <see cref="BodyMode"/>-Wert.</param>
    [Theory]
    [InlineData("Xml", BodyMode.Xml)]
    [InlineData("PlainText", BodyMode.PlainText)]
    public async Task BodyModeChanged_Auswahl_MeldetNeuenModus(string selectedValue, BodyMode expected)
    {
        BodyMode? notified = null;
        var cut = Render<RequestBodyPanel>(parameters => parameters
            .Add(x => x.BodyMode, BodyMode.None)
            .Add(x => x.BodyModeChanged, EventCallback.Factory.Create<BodyMode>(this, mode => notified = mode)));

        var select = cut.Find("select.sz-form-select");
        await cut.InvokeAsync(() => select.Change(selectedValue));

        Assert.Equal(expected, notified);
    }

    /// <summary>Die Textarea ist bei <see cref="BodyMode.PlainText"/> editierbar und meldet Eingaben über <c>BodyChanged</c>.</summary>
    [Fact]
    public async Task BodyInput_PlainText_IstEditierbar_UndMeldetEingabe()
    {
        string? changedBody = null;
        var cut = Render<RequestBodyPanel>(parameters => parameters
            .Add(x => x.BodyMode, BodyMode.PlainText)
            .Add(x => x.BodyChanged, EventCallback.Factory.Create<string?>(this, value => changedBody = value)));

        var textarea = cut.Find("textarea.sz-body-textarea");
        Assert.False(textarea.HasAttribute("disabled"));

        await cut.InvokeAsync(() => textarea.Input("Hallo Welt"));

        Assert.Equal("Hallo Welt", changedBody);
    }
}
