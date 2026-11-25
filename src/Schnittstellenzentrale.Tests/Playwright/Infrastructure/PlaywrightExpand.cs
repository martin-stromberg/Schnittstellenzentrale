using Microsoft.Playwright;

namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>
/// Retry-Helfer für das Aufklappen von Baumknoten in den Playwright-Tests.
/// </summary>
internal static class PlaywrightExpand
{
    /// <summary>
    /// Klickt <paramref name="clickTarget"/> bis zu dreimal, bis <paramref name="waitTarget"/>
    /// sichtbar wird, und verifiziert das Ergebnis abschließend.
    /// </summary>
    /// <remarks>
    /// Zwei Flake-Ursachen werden abgefedert: Ein Klick während eines SignalR-Reconnects
    /// (z. B. nach <c>Page.ReloadAsync()</c>) kann verloren gehen — Blazor Server verwirft
    /// Events ohne aktiven Circuit —, und ein bereits persistierter Expand-Zustand dürfte
    /// durch einen Blindklick nicht wieder zugeklappt werden (Toggle-Semantik). Deshalb
    /// steht die Sichtbarkeitsprüfung auf <paramref name="waitTarget"/> vor jedem Klick —
    /// sie ersetzt die früheren blinden Chevron-Doppelklicks.
    /// Semantik-Hinweis: Ist der Knoten bereits expandiert, aber
    /// <paramref name="waitTarget"/> ein noch nicht geladenes Kindelement (z. B. frisch
    /// importierte Endpunkte), führt das Muster wie beabsichtigt zu Zuklappen +
    /// Aufklappen — das zweite Aufklappen löst <c>ReloadApplicationDataAsync</c> aus und
    /// lädt die Daten nach.
    /// Wird <paramref name="waitTarget"/> auch nach drei Klicks nicht sichtbar, schlägt
    /// der finale <c>WaitForAsync()</c> mit Default-Timeout fehl und liefert die
    /// eigentliche Fehlermeldung.
    /// </remarks>
    /// <param name="waitTarget">Das Element, dessen Sichtbarkeit den Expand-Erfolg markiert.</param>
    /// <param name="clickTarget">Das Element, das zum Expandieren geklickt wird.</param>
    internal static async Task ExpandWithRetryAsync(ILocator waitTarget, ILocator clickTarget)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await waitTarget.WaitForAsync(new() { Timeout = 5_000 });
                return;
            }
            catch (TimeoutException)
            {
            }

            await clickTarget.ClickAsync();
        }

        await waitTarget.WaitForAsync();
    }
}
