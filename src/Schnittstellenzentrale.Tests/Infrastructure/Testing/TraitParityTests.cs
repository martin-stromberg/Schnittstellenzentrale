using System.Reflection;

namespace Schnittstellenzentrale.Tests.Infrastructure.Testing;

/// <summary>Erzwingt Trait-Parität zum früheren Namensfilter <c>FullyQualifiedName~Playwright</c>:
/// jede Testklasse, die dieser Filter erfasst hätte, muss <c>Category=OsInterface</c> tragen —
/// klassenweise oder auf jeder einzelnen Testmethode. Verhindert stille Regressionen beim
/// Filterwechsel, wenn neue Playwright-Testklassen ohne Tag hinzukommen. Läuft bewusst in
/// der regulären Lane (reiner Assembly-Scan).</summary>
/// <remarks>Begrenzung: Nur Typen mit <c>"Playwright"</c> im <see cref="Type.FullName"/>
/// werden geprüft. Umgebungssensitive Tests außerhalb des Playwright-Namensraums
/// (z. B. <c>Services/ImpressumServiceTests</c>) werden per Audit manuell zugeordnet —
/// siehe <c>docs/help/stabilitaet/os-interface-tests.md</c>.</remarks>
public sealed class TraitParityTests
{
    /// <summary>Jeder Typ der Test-Assembly, dessen <see cref="Type.FullName"/>
    /// <c>"Playwright"</c> enthält und der mindestens eine Testmethode besitzt, muss
    /// <c>Category=OsInterface</c> tragen — auf Klassenebene oder auf jeder Testmethode.</summary>
    [Fact]
    public void PlaywrightTestClasses_AlwaysCarryOsInterface()
    {
        var violations = new List<string>();
        var assembly = typeof(TraitParityTests).Assembly;
        foreach (var type in assembly.GetTypes())
        {
            if (type.FullName is null || !type.FullName.Contains("Playwright", StringComparison.Ordinal))
                continue;

            // FactAttribute deckt auch TheoryAttribute ab (Theory erbt von Fact in xUnit v2).
            var testMethods = type
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(method => method.GetCustomAttributes(inherit: true).Any(attribute => attribute is FactAttribute))
                .ToArray();
            if (testMethods.Length == 0)
                continue;

            if (TestTraitReader.HasOsInterface(type))
                continue;

            var untaggedMethods = testMethods
                .Where(method => !TestTraitReader.HasOsInterface(method))
                .Select(method => method.Name)
                .ToArray();
            if (untaggedMethods.Length > 0)
            {
                violations.Add(
                    $"{type.FullName} — Methoden ohne Category=OsInterface: {string.Join(", ", untaggedMethods)}");
            }
        }

        Assert.True(violations.Count == 0,
            "Jede Playwright-Testklasse muss Category=OsInterface tragen ([OsInterface] auf Klasse "
            + "oder [OsInterfaceFact]/[OsInterfaceTheory]/[Trait(\"Category\",\"OsInterface\")] auf jeder Methode). Verstöße: "
            + string.Join(" | ", violations));
    }
}
