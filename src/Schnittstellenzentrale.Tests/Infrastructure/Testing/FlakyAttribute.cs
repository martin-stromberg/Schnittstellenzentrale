using Xunit.v3;

namespace Schnittstellenzentrale.Tests.Infrastructure.Testing;

/// <summary>
/// Markiert einen OS-nahen Test als nachweislich umgebungsflaky und damit berechtigt
/// für den einmaligen automatischen Retry in der best-effort-Lane (Trait
/// <c>Category=Flaky</c>).
/// </summary>
/// <remarks>
/// Vergaberegeln:
/// <list type="bullet">
/// <item>Nur in Kombination mit <see cref="OsInterfaceAttribute"/> vergeben — nie in der
/// regulären Lane (abgesichert durch <see cref="FlakyConventionTests"/>).</item>
/// <item>Nur bei einem dokumentierten Flake-Ereignis (konkreter CI-/Lokal-Fehlschlag mit
/// Fehlerbild) oder einem Analyzer-Befund aus <c>scripts/Analyze-FlakyTestResults.ps1</c>.</item>
/// <item>Die Markierung wird entfernt, sobald die Ursache behoben ist.</item>
/// </list>
/// xUnit v3 kennt keine <c>ITraitDiscoverer</c> mehr: Attribute beschreiben ihre
/// Traits selbst über <see cref="ITraitAttribute.GetTraits"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class FlakyAttribute : Attribute, ITraitAttribute
{
    /// <summary>Liefert das <c>("Category", "Flaky")</c>-Trait-Paar.</summary>
    /// <returns>Die Trait-Paare dieses Attributs.</returns>
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
        => [new KeyValuePair<string, string>("Category", TestCategories.Flaky)];
}
