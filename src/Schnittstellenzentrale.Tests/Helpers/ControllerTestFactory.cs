using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Infrastructure.Data;
using Schnittstellenzentrale.Infrastructure.Repositories;
using Schnittstellenzentrale.Services;

namespace Schnittstellenzentrale.Tests.Helpers;

/// <summary>WebApplicationFactory für Controller-Integrationstests mit Auth-Bypass und SQLite In-Memory.</summary>
public class ControllerTestFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    /// <summary>Überschreibt die Standard-Token-Lebenszeit im <see cref="ITokenStore"/>.</summary>
    public TimeSpan? TokenLifetime { get; set; }

    /// <summary>Öffnet eine SQLite In-Memory-Verbindung für den Testlauf.</summary>
    public ControllerTestFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    /// <inheritdoc/>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
            services.RemoveAll<Microsoft.AspNetCore.Authentication.IAuthenticationHandlerProvider>();
            services.RemoveAll<Microsoft.Extensions.Options.IConfigureOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions>>();

            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextFactory<AppDbContext>>();

            services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(_connection));

            services.RemoveAll<IApplicationRepository>();
            services.AddScoped<IApplicationRepository, ApplicationRepository>();

            services.RemoveAll<IHostedService>();

            var signalRMock = new Mock<ISignalRNotificationService>();
            services.RemoveAll<ISignalRNotificationService>();
            services.AddScoped(_ => signalRMock.Object);

            if (TokenLifetime.HasValue)
            {
                services.RemoveAll<ITokenStore>();
                services.AddSingleton<ITokenStore>(_ => new TokenStore(TokenLifetime.Value));
            }
        });
    }

    /// <inheritdoc/>
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var db = factory.CreateDbContext();
        db.Database.EnsureCreated();

        return host;
    }

    /// <summary>Ruft <c>POST /authenticate</c> auf und gibt den zurückgelieferten Bearer-Token zurück.</summary>
    public async Task<string> ObtainTokenAsync(HttpClient client)
    {
        var response = await client.PostAsync("/authenticate", null);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthenticateResponse>();
        return body!.Token;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}
