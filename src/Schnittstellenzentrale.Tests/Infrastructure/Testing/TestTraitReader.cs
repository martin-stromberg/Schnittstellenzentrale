using System.Reflection;

namespace Schnittstellenzentrale.Tests.Infrastructure.Testing;

/// <summary>
/// Liest die effektiven Trait-Paare eines Typs oder Members per Reflection — identisch zur
/// Semantik der xUnit-v3-Trait-Aufsammlung:
/// <list type="bullet">
/// <item><see cref="TraitAttribute"/>-Vorkommen liefern <c>(Name, Value)</c> aus ihren
/// Konstruktor-Argumenten (TraitAttribute hat keine öffentlichen Name/Value-Properties;
/// gelesen wird daher über <see cref="MemberInfo.CustomAttributes"/>).</item>
/// <item>Attribute, die <see cref="Xunit.v3.ITraitAttribute"/> implementieren, liefern
/// ihre Paare direkt über <c>GetTraits()</c> (deckt <see cref="OsInterfaceAttribute"/>,
/// <see cref="OsInterfaceFactAttribute"/>, <see cref="OsInterfaceTheoryAttribute"/> und
/// <see cref="FlakyAttribute"/> ab).</item>
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
        // (a) [Trait("name", "value")]: Name/Wert werden über CustomAttributeData gelesen,
        // und zwar über die Vererbungskette (Konsistenz mit Zweig
        // (b)/GetCustomAttributes(inherit: true), das vererbte Klassen-Traits ebenfalls
        // erfasst — xUnit vererbt Klassen-Traits auf Methoden).
        foreach (var attributeData in EnumerateCustomAttributeDataIncludingInherited(member))
        {
            if (attributeData.AttributeType != typeof(TraitAttribute))
                continue;
            var args = attributeData.ConstructorArguments;
            if (args.Count >= 2 && args[0].Value is string name && args[1].Value is string value)
                yield return new KeyValuePair<string, string>(name, value);
        }

        // (b) Attribute mit ITraitAttribute.GetTraits() (OsInterface*, Flaky).
        foreach (var attribute in member.GetCustomAttributes(inherit: true))
        {
            if (attribute is Xunit.v3.ITraitAttribute traitAttribute)
            {
                foreach (var trait in traitAttribute.GetTraits())
                    yield return trait;
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
