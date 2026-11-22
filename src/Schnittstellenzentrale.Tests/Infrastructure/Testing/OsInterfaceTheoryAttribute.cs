using Xunit.Sdk;

namespace Schnittstellenzentrale.Tests.Infrastructure.Testing;

/// <summary>
/// <see cref="TheoryAttribute"/>-Variante für OS-Schnittstellen-Tests (Trait
/// <c>Category=OsInterface</c>). Vorsorglich vorhanden — aktuell gibt es keinen
/// <c>[Theory]</c>-Anwender in der OS-Schnittstellen-Lane.
/// </summary>
/// <remarks>
/// <see cref="Xunit.Sdk.ITraitAttribute"/> ist zwingend: xUnit 2.9.3 sammelt Trait-Attribute
/// ausschließlich über <c>GetCustomAttributes(typeof(ITraitAttribute))</c> — ohne das
/// memberlose Marker-Interface würde der <see cref="CategoryTraitDiscoverer"/> nie aufgerufen.
/// </remarks>
[TraitDiscoverer("Schnittstellenzentrale.Tests.Infrastructure.Testing.CategoryTraitDiscoverer", "Schnittstellenzentrale.Tests")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class OsInterfaceTheoryAttribute : TheoryAttribute, ITraitAttribute
{
    /// <summary>Der vom <see cref="CategoryTraitDiscoverer"/> gelesene Kategorie-Wert.</summary>
    public string Category => TestCategories.OsInterface;
}
