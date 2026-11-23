using Xunit.Abstractions;
using Xunit.Sdk;

namespace Schnittstellenzentrale.Tests.Infrastructure.Testing;

/// <summary>
/// xUnit-v2-Trait-Discoverer für die Convenience-Attribute <see cref="OsInterfaceAttribute"/>,
/// <see cref="OsInterfaceFactAttribute"/>, <see cref="OsInterfaceTheoryAttribute"/> und
/// <see cref="FlakyAttribute"/>. Liest die <c>Category</c>-Property des jeweiligen Attributs
/// und emittiert daraus ein <c>("Category", wert)</c>-Trait-Paar, damit
/// <c>--filter "Category=OsInterface"</c> bzw. <c>"Category=Flaky"</c> greifen.
/// </summary>
public sealed class CategoryTraitDiscoverer : ITraitDiscoverer
{
    /// <inheritdoc />
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        var category = traitAttribute.GetNamedArgument<string>("Category");
        if (!string.IsNullOrEmpty(category))
            yield return new KeyValuePair<string, string>("Category", category);
    }
}
