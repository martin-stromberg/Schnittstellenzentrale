using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Integration;

/// <summary>AuthControllerIntegrationTests</summary>
public class AuthControllerIntegrationTests : IClassFixture<ControllerTestFactory>
{
    private readonly ControllerTestFactory _factory;

    /// <summary>Initialisiert AuthControllerIntegrationTests.</summary>
    public AuthControllerIntegrationTests(ControllerTestFactory factory)
    {
        _factory = factory;
    }

    /// <summary>Authenticate_WithValidWindowsIdentity_Returns200WithToken</summary>
    [Fact]
    public async Task Authenticate_WithValidWindowsIdentity_Returns200WithToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/authenticate", null);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthenticateResponse>();
        Assert.NotNull(body);
        Assert.True(Guid.TryParse(body.Token, out _));
    }

    /// <summary>Authenticate_CreatesTokenInTokenStore</summary>
    [Fact]
    public async Task Authenticate_CreatesTokenInTokenStore()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/authenticate", null);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<AuthenticateResponse>();
        Assert.NotNull(body);

        var tokenStore = _factory.Services.GetRequiredService<ITokenStore>();
        var rotated = await tokenStore.ValidateAndRotateAsync(body.Token);
        Assert.NotNull(rotated);
    }
}
