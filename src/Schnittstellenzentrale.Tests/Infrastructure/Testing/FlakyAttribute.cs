using Xunit.Sdk;

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
/// <see cref="Xunit.Sdk.ITraitAttribute"/> ist zwingend: xUnit 2.9.3 sammelt Trait-Attribute
/// ausschließlich über <c>GetCustomAttributes(typeof(ITraitAttribute))</c> — ohne das
/// memberlose Marker-Interface würde der <see cref="CategoryTraitDiscoverer"/> nie aufgerufen.
/// </remarks>
[TraitDiscoverer("Schnittstellenzentrale.Tests.Infrastructure.Testing.CategoryTraitDiscoverer", "Schnittstellenzentrale.Tests")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class FlakyAttribute : Attribute, ITraitAttribute
{
    /// <summary>Der vom <see cref="CategoryTraitDiscoverer"/> gelesene Kategorie-Wert.</summary>
    public string Category => TestCategories.Flaky;
}
