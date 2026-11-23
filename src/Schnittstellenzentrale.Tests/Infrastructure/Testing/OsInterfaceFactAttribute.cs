using Xunit.Sdk;

namespace Schnittstellenzentrale.Tests.Infrastructure.Testing;

/// <summary>
/// <see cref="FactAttribute"/>-Variante für OS-Schnittstellen-Tests (Trait
/// <c>Category=OsInterface</c>) — für Einzelmethoden-Ausnahmen in sonst regulären Klassen.
/// Klassenweite Markierung ist über <see cref="OsInterfaceAttribute"/> der Standard.
/// </summary>
/// <remarks>
/// <see cref="Xunit.Sdk.ITraitAttribute"/> ist zwingend: xUnit 2.9.3 sammelt Trait-Attribute
/// ausschließlich über <c>GetCustomAttributes(typeof(ITraitAttribute))</c> — ohne das
/// memberlose Marker-Interface würde der <see cref="CategoryTraitDiscoverer"/> nie aufgerufen.
/// </remarks>
[TraitDiscoverer("Schnittstellenzentrale.Tests.Infrastructure.Testing.CategoryTraitDiscoverer", "Schnittstellenzentrale.Tests")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class OsInterfaceFactAttribute : FactAttribute, ITraitAttribute
{
    /// <summary>Der vom <see cref="CategoryTraitDiscoverer"/> gelesene Kategorie-Wert.</summary>
    public string Category => TestCategories.OsInterface;
}
