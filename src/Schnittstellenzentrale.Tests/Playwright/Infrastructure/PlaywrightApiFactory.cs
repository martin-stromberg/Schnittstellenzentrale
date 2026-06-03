using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Data;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>
/// In-Process-TestServer für API-Calls aus dem Blazor-Server.
/// Verhindert TCP-Deadlocks: der Kestrel-Prozess macht HTTP-Requests gegen sich selbst
/// über <see cref="TestServer.CreateHandler()"/>, nicht über echtes TCP.
/// Teilt die SQLite-Verbindung mit <see cref="PlaywrightServer"/> — kein Datei-Locking.
/// </summary>
internal sealed class PlaywrightApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbConnectionString;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISignalRNotificationService _signalRNotificationService;

    public PlaywrightApiFactory(
        string dbConnectionString,
        ICurrentUserService currentUserService,
        ISignalRNotificationService signalRNotificationService)
    {
        _dbConnectionString = dbConnectionString;
        _currentUserService = currentUserService;
        _signalRNotificationService = signalRNotificationService;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Playwright");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAuthenticationSchemeProvider>();
            services.RemoveAll<IAuthenticationHandlerProvider>();
            services.RemoveAll<IConfigureOptions<AuthenticationOptions>>();
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            services.RemoveAll<IHostedService>();

            services.RemoveAll<ISignalRNotificationService>();
            services.AddScoped(_ => _signalRNotificationService);

            services.RemoveAll<ICurrentUserService>();
            services.AddSingleton(_ => _currentUserService);

            // Eigene Verbindung zur benannten In-Memory-DB — kein geteiltes Verbindungsobjekt.
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextFactory<AppDbContext>>();
            services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(_dbConnectionString));

            // Akzeptiert jeden Bearer-Token, damit die vom Kestrel-Server ausgestellten
            // Tokens hier ohne eigenen TokenStore-Abgleich gültig sind.
            services.RemoveAll<ITokenStore>();
            services.AddSingleton<ITokenStore, PermissiveTokenStore>();
        });
    }

    private sealed class PermissiveTokenStore : ITokenStore
    {
        private static readonly AuthToken Token = new()
        {
            TokenValue = "playwright-inprocess-token",
            ExpiresAt = DateTime.MaxValue,
            WindowsUsername = @"TEST\testuser"
        };

        public Task<AuthToken> CreateTokenAsync(string username) => Task.FromResult(Token);
        public Task<AuthToken?> ValidateAndRotateAsync(string tokenString) => Task.FromResult<AuthToken?>(Token);
    }
}
