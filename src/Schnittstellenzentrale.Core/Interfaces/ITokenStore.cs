using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

public interface ITokenStore
{
    Task<AuthToken> CreateTokenAsync(string username);
    Task<AuthToken?> ValidateAndRotateAsync(string tokenString);
}
