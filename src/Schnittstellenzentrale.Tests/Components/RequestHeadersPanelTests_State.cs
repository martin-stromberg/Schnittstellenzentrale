using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für Änderungs-, Add- und Remove-Verhalten von <see cref="RequestHeadersPanel"/>.</summary>
public class RequestHeadersPanelTests_State : BunitContext
{
    /// <summary>Registriert Lokalisierung für Komponententests.</summary>
    public RequestHeadersPanelTests_State()
    {
        Services.AddSingleton(TestMockFactory.CreateFakeLocalizer());
    }

    /// <summary>Das Ändern des Header-Keys aktualisiert den Eintrag, deaktiviert Auto-Flag und signalisiert Änderung.</summary>
    [Fact]
    public async Task OnKeyChanged_UpdatesEntry_ClearsAutoFlag_AndNotifies()
    {
        var headers = new List<RequestHeadersPanel.HeaderEntry>
        {
            new() { Id = 1, Key = "Content-Type", Value = "application/json", IsAutoContentType = true }
        };
        var changedCount = 0;

        var cut = Render<RequestHeadersPanel>(parameters => parameters
            .Add(x => x.Headers, headers)
            .Add(x => x.OnChanged, EventCallback.Factory.Create(this, () => changedCount++)));

        var keyInput = cut.FindAll("input").First();
        await cut.InvokeAsync(() => keyInput.Input("X-Test"));

        Assert.Equal("X-Test", headers[0].Key);
        Assert.False(headers[0].IsAutoContentType);
        Assert.Equal(1, changedCount);
    }

    /// <summary>Der Add-Button legt einen neuen Header an und löst Added-/Changed-Callbacks aus.</summary>
    [Fact]
    public async Task AddHeader_AddsEntry_InvokesOnHeaderAdded_AndOnChanged()
    {
        var headers = new List<RequestHeadersPanel.HeaderEntry>();
        RequestHeadersPanel.HeaderEntry? added = null;
        var changedCount = 0;

        var cut = Render<RequestHeadersPanel>(parameters => parameters
            .Add(x => x.Headers, headers)
            .Add(x => x.OnHeaderAdded, EventCallback.Factory.Create<RequestHeadersPanel.HeaderEntry>(this, entry => added = entry))
            .Add(x => x.OnChanged, EventCallback.Factory.Create(this, () => changedCount++)));

        var addButton = cut.Find("button.sz-links-add-btn");
        await cut.InvokeAsync(() => addButton.Click());

        Assert.Single(headers);
        Assert.NotNull(added);
        Assert.Equal(headers[0], added);
        Assert.Equal(1, changedCount);
    }

    /// <summary>Der Remove-Button entfernt den Header und löst Removed-/Changed-Callbacks aus.</summary>
    [Fact]
    public async Task RemoveHeader_RemovesEntry_InvokesOnHeaderRemoved_AndOnChanged()
    {
        var entry = new RequestHeadersPanel.HeaderEntry { Id = 7, Key = "X-Delete", Value = "v" };
        var headers = new List<RequestHeadersPanel.HeaderEntry> { entry };
        RequestHeadersPanel.HeaderEntry? removed = null;
        var changedCount = 0;

        var cut = Render<RequestHeadersPanel>(parameters => parameters
            .Add(x => x.Headers, headers)
            .Add(x => x.OnHeaderRemoved, EventCallback.Factory.Create<RequestHeadersPanel.HeaderEntry>(this, value => removed = value))
            .Add(x => x.OnChanged, EventCallback.Factory.Create(this, () => changedCount++)));

        var removeButton = cut.Find("button.sz-btn-danger");
        await cut.InvokeAsync(() => removeButton.Click());

        Assert.Empty(headers);
        Assert.Equal(entry, removed);
        Assert.Equal(1, changedCount);
    }
}
