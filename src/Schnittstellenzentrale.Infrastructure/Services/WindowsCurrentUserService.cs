using System.Security.Principal;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Infrastructure.Services;

public class WindowsCurrentUserService : ICurrentUserService
{
    public string GetCurrentUserName()
    {
        using var identity = WindowsIdentity.GetCurrent();
        return identity.Name;
    }
}
