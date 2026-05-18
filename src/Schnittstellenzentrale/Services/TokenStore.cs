#pragma warning disable CS1591
using System.Collections.Concurrent;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Services;

public class TokenStore : ITokenStore
{
    private readonly TimeSpan _tokenLifetime;
    private readonly ConcurrentDictionary<string, AuthToken> _tokens = new();

    public TokenStore() : this(TimeSpan.FromMinutes(5))
    {
    }

    public TokenStore(TimeSpan tokenLifetime)
    {
        _tokenLifetime = tokenLifetime;
    }

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
