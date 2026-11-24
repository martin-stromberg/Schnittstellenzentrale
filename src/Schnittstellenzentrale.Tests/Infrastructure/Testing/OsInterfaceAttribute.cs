using Xunit.v3;

namespace Schnittstellenzentrale.Tests.Infrastructure.Testing;

/// <summary>
/// Markiert eine Testklasse oder -methode als OS-Schnittstellen-Test (Trait
/// <c>Category=OsInterface</c>). Solche Tests rühren echte OS-Ressourcen an
/// (Playwright/Chromium, echte Kestrel-Bindung auf den Ports 5099–5104, Temp-Dateisystem)
/// und laufen in der best-effort-CI-Lane <c>os-interface-tests</c> statt in der
/// blockierenden regulären Lane.
/// </summary>
/// <remarks>
/// xUnit v3 kennt keine <c>ITraitDiscoverer</c> mehr: Attribute beschreiben ihre
/// Traits selbst über <see cref="ITraitAttribute.GetTraits"/>. Der VSTest-Adapter
/// (<c>xunit.runner.visualstudio</c>) überführt die emittierten Trait-Paare in
/// VSTest-Traits, sodass <c>dotnet test --filter "Category=OsInterface"</c>
/// unverändert greift.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class OsInterfaceAttribute : Attribute, ITraitAttribute
{
    /// <summary>Liefert das <c>("Category", "OsInterface")</c>-Trait-Paar.</summary>
    /// <returns>Die Trait-Paare dieses Attributs.</returns>
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
        => [new KeyValuePair<string, string>("Category", TestCategories.OsInterface)];
}
