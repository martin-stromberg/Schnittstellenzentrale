using System.Collections.Concurrent;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Services;

/// <summary>Speichert und verwaltet Authentifizierungstoken im Arbeitsspeicher.</summary>
public class TokenStore : ITokenStore
{
    private readonly TimeSpan _tokenLifetime;
    private readonly ConcurrentDictionary<string, AuthToken> _tokens = new();

    /// <summary>Initialisiert eine neue Instanz von <see cref="TokenStore"/> mit der Standard-Token-Lebensdauer.</summary>
    public TokenStore() : this(TimeSpan.FromMinutes(5))
    {
    }

    /// <summary>Initialisiert eine neue Instanz von <see cref="TokenStore"/>.</summary>
    /// <param name="tokenLifetime">Die Lebensdauer eines Tokens.</param>
    public TokenStore(TimeSpan tokenLifetime)
    {
        _tokenLifetime = tokenLifetime;
    }

    /// <summary>Erstellt einen neuen Authentifizierungstoken.</summary>
    /// <param name="username">Der Benutzername.</param>
    /// <returns>Der erstellte Token.</returns>
    public Task<AuthToken> CreateTokenAsync(string username)
    {
        RemoveExpiredTokens();

        var token = new AuthToken
        {
            TokenValue = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.Add(_tokenLifetime),
            WindowsUsername = username
        };

        _tokens[token.TokenValue] = token;
        return Task.FromResult(token);
    }

    /// <summary>Validiert und rotiert einen vorhandenen Token.</summary>
    /// <param name="tokenString">Der Token-String.</param>
    /// <returns>Der rotierte Token oder <c>null</c>, wenn der Token ungültig ist.</returns>
    public Task<AuthToken?> ValidateAndRotateAsync(string tokenString)
    {
        RemoveExpiredTokens();

        if (!_tokens.TryRemove(tokenString, out var existingToken))
            return Task.FromResult<AuthToken?>(null);

        if (existingToken.ExpiresAt <= DateTime.UtcNow)
            return Task.FromResult<AuthToken?>(null);

        var newToken = new AuthToken
        {
            TokenValue = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.Add(_tokenLifetime),
            WindowsUsername = existingToken.WindowsUsername
        };

        _tokens[newToken.TokenValue] = newToken;
        return Task.FromResult<AuthToken?>(newToken);
    }

    private void RemoveExpiredTokens()
    {
        var now = DateTime.UtcNow;
        foreach (var key in _tokens.Keys)
        {
            if (_tokens.TryGetValue(key, out var token) && token.ExpiresAt <= now)
                _tokens.TryRemove(key, out _);
        }
    }
}
