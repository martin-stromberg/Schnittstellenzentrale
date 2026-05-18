using Schnittstellenzentrale.Services;

namespace Schnittstellenzentrale.Tests.Services;

public class TokenStoreTests
{
    [Fact]
    public async Task CreateTokenAsync_ReturnsValidToken()
    {
        var store = new TokenStore();

        var token = await store.CreateTokenAsync("DOMAIN\\user");

        Assert.NotNull(token);
        Assert.True(Guid.TryParse(token.TokenValue, out _));
        Assert.Equal("DOMAIN\\user", token.WindowsUsername);
        Assert.True(token.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task ValidateAndRotateAsync_WithValidToken_ReturnsNewToken()
    {
        var store = new TokenStore();
        var original = await store.CreateTokenAsync("DOMAIN\\user");

        var newToken = await store.ValidateAndRotateAsync(original.TokenValue);

        Assert.NotNull(newToken);
        Assert.NotEqual(original.TokenValue, newToken.TokenValue);
        Assert.True(newToken.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task ValidateAndRotateAsync_AfterRotation_OldTokenIsInvalid()
    {
        var store = new TokenStore();
        var original = await store.CreateTokenAsync("DOMAIN\\user");

        await store.ValidateAndRotateAsync(original.TokenValue);

        var shouldBeNull = await store.ValidateAndRotateAsync(original.TokenValue);
        Assert.Null(shouldBeNull);
    }

    [Fact]
    public async Task ValidateAndRotateAsync_WithExpiredToken_ReturnsNull()
    {
        var store = new TokenStore(TimeSpan.FromMilliseconds(1));

        var token = await store.CreateTokenAsync("DOMAIN\\user");

        await Task.Delay(10);

        var result = await store.ValidateAndRotateAsync(token.TokenValue);

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateAndRotateAsync_WithUnknownToken_ReturnsNull()
    {
        var store = new TokenStore();

        var result = await store.ValidateAndRotateAsync(Guid.NewGuid().ToString());

        Assert.Null(result);
    }
}
