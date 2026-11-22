using Xunit.Sdk;

namespace Schnittstellenzentrale.Tests.Infrastructure.Testing;

/// <summary>
/// Markiert eine Testklasse oder -methode als OS-Schnittstellen-Test (Trait
/// <c>Category=OsInterface</c>). Solche Tests rühren echte OS-Ressourcen an
/// (Playwright/Chromium, echte Kestrel-Bindung auf den Ports 5099–5104, Temp-Dateisystem)
/// und laufen in der best-effort-CI-Lane <c>os-interface-tests</c> statt in der
/// blockierenden regulären Lane.
/// </summary>
/// <remarks>
/// <see cref="Xunit.Sdk.ITraitAttribute"/> ist zwingend: xUnit 2.9.3 sammelt Trait-Attribute
/// ausschließlich über <c>GetCustomAttributes(typeof(ITraitAttribute))</c> — ohne das
/// memberlose Marker-Interface würde der <see cref="CategoryTraitDiscoverer"/> nie aufgerufen.
/// </remarks>
[TraitDiscoverer("Schnittstellenzentrale.Tests.Infrastructure.Testing.CategoryTraitDiscoverer", "Schnittstellenzentrale.Tests")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class OsInterfaceAttribute : Attribute, ITraitAttribute
{
    /// <summary>Der vom <see cref="CategoryTraitDiscoverer"/> gelesene Kategorie-Wert.</summary>
    public string Category => TestCategories.OsInterface;
}
