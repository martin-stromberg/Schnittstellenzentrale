using System.Reflection;

namespace Schnittstellenzentrale.Tests.Infrastructure.Testing;

/// <summary>Sichert die Konvention ab, dass die <c>Flaky</c>-Kategorie nur in Kombination
/// mit <c>OsInterface</c> vergeben wird — ein Verstoß würde einen retry-berechtigten Test
/// in die blockierende reguläre Lane schieben. Läuft bewusst in der regulären Lane
/// (reiner Assembly-Scan, kein OS-Bezug).</summary>
public sealed class FlakyConventionTests
{
    /// <summary>Jede Methode oder Klasse, deren Traits <c>Category=Flaky</c> liefern,
    /// muss zusätzlich <c>Category=OsInterface</c> liefern. Klassen-Traits vererben
    /// dabei auf ihre Methoden mit.</summary>
    [Fact]
    public void FlakyMarkedTests_AlwaysCarryOsInterface()
    {
        var violations = new List<string>();
        var assembly = typeof(FlakyConventionTests).Assembly;
        foreach (var type in assembly.GetTypes())
        {
            var typeTraits = TestTraitReader.GetTraits(type).ToArray();
            if (HasCategory(typeTraits, TestCategories.Flaky) && !HasCategory(typeTraits, TestCategories.OsInterface))
                violations.Add(type.FullName ?? type.Name);
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                var methodTraits = TestTraitReader.GetTraits(method).Concat(typeTraits).ToArray();
                if (HasCategory(methodTraits, TestCategories.Flaky) && !HasCategory(methodTraits, TestCategories.OsInterface))
                    violations.Add($"{type.FullName}.{method.Name}");
            }
        }

        Assert.True(violations.Count == 0,
            "Flaky darf nur in Kombination mit OsInterface vergeben werden. Verstöße: "
            + string.Join(", ", violations));
    }

    private static bool HasCategory(IEnumerable<KeyValuePair<string, string>> traits, string value)
        => traits.Any(trait => trait.Key == "Category" && trait.Value == value);
}
