using System.Runtime.CompilerServices;
using Xunit.v3;

namespace Schnittstellenzentrale.Tests.Infrastructure.Testing;

/// <summary>
/// <see cref="TheoryAttribute"/>-Variante für OS-Schnittstellen-Tests (Trait
/// <c>Category=OsInterface</c>). Vorsorglich vorhanden — aktuell gibt es keinen
/// <c>[Theory]</c>-Anwender in der OS-Schnittstellen-Lane.
/// </summary>
/// <remarks>
/// xUnit v3 kennt keine <c>ITraitDiscoverer</c> mehr: Attribute beschreiben ihre
/// Traits selbst über <see cref="ITraitAttribute.GetTraits"/>. Da dieses Attribut
/// zugleich das Theory-Attribut der Methode ist, liefert es den Trait direkt mit.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class OsInterfaceTheoryAttribute : TheoryAttribute, ITraitAttribute
{
    /// <summary>Initialisiert das Attribut und reicht die Source-Informationen des
    /// Aufrufortes an <see cref="TheoryAttribute"/> weiter (xUnit3003).</summary>
    /// <param name="sourceFilePath">Pfad der Datei mit der Testmethode (Compiler-gefüllt).</param>
    /// <param name="sourceLineNumber">Zeilennummer der Testmethode (Compiler-gefüllt).</param>
    public OsInterfaceTheoryAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
    }

    /// <summary>Liefert das <c>("Category", "OsInterface")</c>-Trait-Paar.</summary>
    /// <returns>Die Trait-Paare dieses Attributs.</returns>
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
        => [new KeyValuePair<string, string>("Category", TestCategories.OsInterface)];
}
