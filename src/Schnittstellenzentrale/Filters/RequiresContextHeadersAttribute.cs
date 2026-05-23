namespace Schnittstellenzentrale.Filters;

/// <summary>Markiert eine API-Methode als Anforderer der Custom-Request-Header X-Storage-Mode (und optional X-Owner).</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class RequiresContextHeadersAttribute : Attribute
{
    /// <summary>Gibt an, ob auch der X-Owner-Header erforderlich ist.</summary>
    public bool IncludeOwner { get; }

    /// <summary>Initialisiert das Attribut.</summary>
    public RequiresContextHeadersAttribute(bool includeOwner = false) => IncludeOwner = includeOwner;
}
