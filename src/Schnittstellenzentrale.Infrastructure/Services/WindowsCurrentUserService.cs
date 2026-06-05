#pragma warning disable CS1591
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>
/// Implementierung von <see cref="ICurrentUserService"/>, die den aktuellen Benutzer basierend auf der Windows-Identität zurückgibt. Diese Implementierung verwendet die Windows-Authentifizierung, um den Benutzernamen zu ermitteln. Wenn der Benutzer über den HTTP-Kontext authentifiziert ist, wird dessen Name zurückgegeben. Andernfalls wird die Windows-Identität des aktuellen Prozesses abgerufen und deren Name zurückgegeben.
/// </summary>
public class WindowsCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    /// <summary>
    /// Initialisiert eine neue Instanz von <see cref="WindowsCurrentUserService"/>. Das übergebenen <see cref="IHttpContextAccessor"/> wird nicht verwendet, da die Windows-Identität direkt über <see cref="WindowsIdentity.GetCurrent"/> abgerufen wird.
    /// </summary>
    /// <param name="httpContextAccessor">Der HTTP-Kontext-Accessor, der für die Ermittlung des aktuellen Benutzers verwendet werden kann.</param>
    public WindowsCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    /// <summary>
    /// Gibt den Namen des aktuell angemeldeten Benutzers zurück. Zunächst wird versucht, den Benutzernamen aus dem aktuellen HTTP-Kontext zu ermitteln. Wenn der Benutzer authentifiziert ist und einen Namen hat, wird dieser zurückgegeben. Andernfalls wird die Windows-Identität des aktuellen Prozesses abgerufen und deren Name zurückgegeben.
    /// </summary>
    /// <returns></returns>
    public string GetCurrentUserName()
    {
        var identity = _httpContextAccessor.HttpContext?.User?.Identity;
        if (identity?.IsAuthenticated == true && !string.IsNullOrEmpty(identity.Name))
            return identity.Name;

        using var windowsIdentity = WindowsIdentity.GetCurrent();
        return windowsIdentity.Name;
    }
}
