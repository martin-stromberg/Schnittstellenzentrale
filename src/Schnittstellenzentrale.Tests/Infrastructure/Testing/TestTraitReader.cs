using System.Reflection;

namespace Schnittstellenzentrale.Tests.Infrastructure.Testing;

/// <summary>
/// Liest die effektiven Trait-Paare eines Typs oder Members per Reflection — identisch zur
/// Semantik des <see cref="CategoryTraitDiscoverer"/> und des eingebauten
/// <c>Xunit.Sdk.TraitDiscoverer</c>:
/// <list type="bullet">
/// <item><see cref="TraitAttribute"/>-Vorkommen liefern <c>(Name, Value)</c> aus ihren
/// Konstruktor-Argumenten (TraitAttribute hat in xUnit v2 keine öffentlichen Properties;
/// gelesen wird daher über <see cref="MemberInfo.CustomAttributes"/>).</item>
/// <item>Attribute mit lesbarer <c>Category</c>-Property liefern <c>("Category", wert)</c>
/// (deckt <see cref="OsInterfaceAttribute"/>, <see cref="OsInterfaceFactAttribute"/>,
/// <see cref="OsInterfaceTheoryAttribute"/> und <see cref="FlakyAttribute"/> ab).</item>
/// </list>
/// Dient den Meta-Tests <see cref="FlakyConventionTests"/> und <see cref="TraitParityTests"/>
/// als gemeinsame Auswertung beider zulässigen Schreibweisen.
/// </summary>
internal static class TestTraitReader
{
    /// <summary>Liefert alle Trait-Paare, die an <paramref name="member"/> hängen
    /// (bei Typen inkl. vererbter Klassen-Attribute).</summary>
    /// <param name="member">Der auszuwertende Typ oder Member.</param>
    /// <returns>Die Trait-Paare als Schlüssel-Wert-Sequenz.</returns>
    internal static IEnumerable<KeyValuePair<string, string>> GetTraits(MemberInfo member)
    {
        // (a) [Trait("name", "value")]: Name/Wert existieren nur als Ctor-Argumente —
        // TraitAttribute macht sie in xUnit v2 nicht als öffentliche Property lesbar.
        // Gelesen wird daher über CustomAttributeData, und zwar über die Vererbungskette
        // (Konsistenz mit Zweig (b)/GetCustomAttributes(inherit: true), das vererbte
        // Klassen-Traits ebenfalls erfasst — xUnit vererbt Klassen-Traits auf Methoden).
        foreach (var attributeData in EnumerateCustomAttributeDataIncludingInherited(member))
        {
            if (attributeData.AttributeType != typeof(TraitAttribute))
                continue;
            var args = attributeData.ConstructorArguments;
            if (args.Count >= 2 && args[0].Value is string name && args[1].Value is string value)
                yield return new KeyValuePair<string, string>(name, value);
        }

        // (b) Attribute mit lesbarer Category-Property (OsInterface*, Flaky).
        foreach (var attribute in member.GetCustomAttributes(inherit: true))
        {
            var categoryProperty = attribute.GetType().GetProperty("Category", BindingFlags.Public | BindingFlags.Instance);
            if (categoryProperty is { CanRead: true }
                && categoryProperty.PropertyType == typeof(string)
                && categoryProperty.GetValue(attribute) is string category
                && !string.IsNullOrEmpty(category))
            {
                yield return new KeyValuePair<string, string>("Category", category);
            }
        }
    }

    /// <summary>Prüft, ob <paramref name="member"/> effektiv <c>Category=OsInterface</c>
    /// trägt (direkt oder — bei Methoden — über die Klassen-Traits, die der Aufrufer
    /// mitgeben kann).</summary>
    /// <param name="member">Der auszuwertende Typ oder Member.</param>
    /// <returns><c>true</c>, wenn ein <c>("Category", "OsInterface")</c>-Trait vorliegt.</returns>
    internal static bool HasOsInterface(MemberInfo member)
        => GetTraits(member).Any(trait => trait.Key == "Category" && trait.Value == TestCategories.OsInterface);

    /// <summary>Liefert die <see cref="CustomAttributeData"/> von <paramref name="member"/>
    /// inkl. der Vererbungskette — dieselbe Semantik wie
    /// <see cref="MemberInfo.GetCustomAttributes(bool)"/> mit <c>inherit: true</c>:
    /// für Typen die <see cref="Type.BaseType"/>-Kette, für Methoden die
    /// <see cref="MethodInfo.GetBaseDefinition"/>-Kette (Overrides). Nötig, weil
    /// <see cref="MemberInfo.CustomAttributes"/> nur direkt deklarierte Attribute liefert
    /// und <see cref="TraitAttribute"/> keine öffentlichen Name/Value-Properties besitzt,
    /// die man an instanziierten Attributen lesen könnte.</summary>
    /// <param name="member">Der auszuwertende Typ oder Member.</param>
    /// <returns>Die Attribut-Metadaten inkl. vererbter Vorkommen.</returns>
    private static IEnumerable<CustomAttributeData> EnumerateCustomAttributeDataIncludingInherited(MemberInfo member)
    {
        switch (member)
        {
            case Type type:
                for (var current = type; current is not null; current = current.BaseType)
                {
                    foreach (var attributeData in current.CustomAttributes)
                        yield return attributeData;
                }
                yield break;
            case MethodInfo method:
                var currentMethod = method;
                while (true)
                {
                    foreach (var attributeData in currentMethod.CustomAttributes)
                        yield return attributeData;
                    var baseDefinition = currentMethod.GetBaseDefinition();
                    if (baseDefinition == currentMethod)
                        yield break;
                    currentMethod = baseDefinition;
                }
            default:
                foreach (var attributeData in member.CustomAttributes)
                    yield return attributeData;
                yield break;
        }
    }
}
