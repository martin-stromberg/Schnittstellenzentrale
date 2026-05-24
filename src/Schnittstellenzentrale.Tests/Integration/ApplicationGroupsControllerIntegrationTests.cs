using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Integration;

/// <summary>Integrationstests für den ApplicationGroupsController.</summary>
public class ApplicationGroupsControllerIntegrationTests : IClassFixture<ControllerTestFactory>
{
    private readonly ControllerTestFactory _factory;

    /// <summary>Initialisiert den Test mit der gemeinsamen Controller-Factory.</summary>
    public ApplicationGroupsControllerIntegrationTests(ControllerTestFactory factory)
    {
        _factory = factory;
    }

    /// <summary>POST mit gültigem Token und Request gibt 201 und Location-Header zurück.</summary>
    [Fact]
    public async Task PostApplicationGroup_WithValidTokenAndRequest_Returns201AndLocation()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/application-groups");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Storage-Mode", "Team");
        request.Content = JsonContent.Create(new CreateApplicationGroupRequest { Name = "TestGruppe" });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.True(response.Headers.Contains("X-New-Token"));

        var body = await response.Content.ReadFromJsonAsync<ApplicationGroupResponse>();
        Assert.NotNull(body);
        Assert.Equal("TestGruppe", body.Name);
        Assert.True(body.Id > 0);
    }

    /// <summary>PostApplicationGroup_WithoutToken_Returns401</summary>
    [Fact]
    public async Task PostApplicationGroup_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/application-groups");
        request.Content = JsonContent.Create(new CreateApplicationGroupRequest { Name = "TestGruppe" });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>PostApplicationGroup_WithExpiredToken_Returns401</summary>
    [Fact]
    public async Task PostApplicationGroup_WithExpiredToken_Returns401()
    {
        var factory = new ControllerTestFactory { TokenLifetime = TimeSpan.FromMilliseconds(1) };
        var client = factory.CreateClient();
        var tokenStore = factory.Services.GetRequiredService<ITokenStore>();

        var token = await tokenStore.CreateTokenAsync("TEST\\testuser");

        await Task.Delay(10);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/application-groups");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.TokenValue);
        request.Headers.Add("X-Storage-Mode", "Team");
        request.Content = JsonContent.Create(new CreateApplicationGroupRequest { Name = "TestGruppe" });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>PostApplicationGroup_WithMissingName_Returns400</summary>
    [Fact]
    public async Task PostApplicationGroup_WithMissingName_Returns400()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/application-groups");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Storage-Mode", "Team");
        request.Content = JsonContent.Create(new CreateApplicationGroupRequest { Name = "" });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>PostApplicationGroup_Returns_NewTokenHeader</summary>
    [Fact]
    public async Task PostApplicationGroup_Returns_NewTokenHeader()
    {
        var client = _factory.CreateClient();
        var originalToken = await _factory.ObtainTokenAsync(client);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/application-groups");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", originalToken);
        request.Headers.Add("X-Storage-Mode", "Team");
        request.Content = JsonContent.Create(new CreateApplicationGroupRequest { Name = "RotationsTest" });

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        Assert.True(response.Headers.TryGetValues("X-New-Token", out var tokenValues));
        var newToken = tokenValues.First();
        Assert.NotEqual(originalToken, newToken);
    }

    /// <summary>PostApplicationGroup_AfterRotation_OldTokenIsUnauthorized</summary>
    [Fact]
    public async Task PostApplicationGroup_AfterRotation_OldTokenIsUnauthorized()
    {
        var client = _factory.CreateClient();
        var originalToken = await _factory.ObtainTokenAsync(client);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/application-groups");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", originalToken);
        request.Headers.Add("X-Storage-Mode", "Team");
        request.Content = JsonContent.Create(new CreateApplicationGroupRequest { Name = "RotationsTest" });

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var request2 = new HttpRequestMessage(HttpMethod.Post, "/api/application-groups");
        request2.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", originalToken);
        request2.Headers.Add("X-Storage-Mode", "Team");
        request2.Content = JsonContent.Create(new CreateApplicationGroupRequest { Name = "SollteFehlschlagen" });

        var response2 = await client.SendAsync(request2);
        Assert.Equal(HttpStatusCode.Unauthorized, response2.StatusCode);
    }

    /// <summary>PostApplicationGroup_AfterRotation_NewTokenIsValid</summary>
    [Fact]
    public async Task PostApplicationGroup_AfterRotation_NewTokenIsValid()
    {
        var client = _factory.CreateClient();
        var originalToken = await _factory.ObtainTokenAsync(client);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/application-groups");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", originalToken);
        request.Headers.Add("X-Storage-Mode", "Team");
        request.Content = JsonContent.Create(new CreateApplicationGroupRequest { Name = "RotationsTest" });

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        response.Headers.TryGetValues("X-New-Token", out var tokenValues);
        var newToken = tokenValues!.First();

        var request3 = new HttpRequestMessage(HttpMethod.Post, "/api/application-groups");
        request3.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newToken);
        request3.Headers.Add("X-Storage-Mode", "Team");
        request3.Content = JsonContent.Create(new CreateApplicationGroupRequest { Name = "MitNeuemToken" });

        var response3 = await client.SendAsync(request3);
        Assert.Equal(HttpStatusCode.Created, response3.StatusCode);
    }

    /// <summary>GetApplicationGroups_WithValidToken_Returns200WithList</summary>
    [Fact]
    public async Task GetApplicationGroups_WithValidToken_Returns200WithList()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/application-groups");
        postRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        postRequest.Headers.Add("X-Storage-Mode", "Team");
        postRequest.Content = JsonContent.Create(new CreateApplicationGroupRequest { Name = "GetTestGruppe" });
        var postResponse = await client.SendAsync(postRequest);
        postResponse.EnsureSuccessStatusCode();
        postResponse.Headers.TryGetValues("X-New-Token", out var newTokenValues);
        token = newTokenValues!.First();

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/application-groups");
        getRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        getRequest.Headers.Add("X-Storage-Mode", "Team");
        getRequest.Headers.Add("X-Owner", "");

        var getResponse = await client.SendAsync(getRequest);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.True(getResponse.Headers.Contains("X-New-Token"));

        var body = await getResponse.Content.ReadFromJsonAsync<IList<ApplicationGroupResponse>>();
        Assert.NotNull(body);
        Assert.Contains(body, g => g.Name == "GetTestGruppe");
        Assert.All(body, g => Assert.NotNull(g.Applications));
    }

    /// <summary>GetApplicationGroups_WithoutToken_Returns401</summary>
    [Fact]
    public async Task GetApplicationGroups_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/application-groups");
        request.Headers.Add("X-Storage-Mode", "Team");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>GetApplicationGroupById_WithValidId_Returns200</summary>
    [Fact]
    public async Task GetApplicationGroupById_WithValidId_Returns200()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/application-groups");
        postRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        postRequest.Headers.Add("X-Storage-Mode", "Team");
        postRequest.Content = JsonContent.Create(new CreateApplicationGroupRequest { Name = "GetByIdGruppe" });
        var postResponse = await client.SendAsync(postRequest);
        postResponse.EnsureSuccessStatusCode();
        var created = await postResponse.Content.ReadFromJsonAsync<ApplicationGroupResponse>();
        postResponse.Headers.TryGetValues("X-New-Token", out var newTokenValues);
        token = newTokenValues!.First();

        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/application-groups/{created!.Id}");
        getRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var getResponse = await client.SendAsync(getRequest);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.True(getResponse.Headers.Contains("X-New-Token"));

        var body = await getResponse.Content.ReadFromJsonAsync<ApplicationGroupResponse>();
        Assert.NotNull(body);
        Assert.Equal(created.Id, body.Id);
        Assert.Equal("GetByIdGruppe", body.Name);
    }

    /// <summary>GetApplicationGroupById_WithInvalidId_Returns404</summary>
    [Fact]
    public async Task GetApplicationGroupById_WithInvalidId_Returns404()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/application-groups/999999");
        getRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var getResponse = await client.SendAsync(getRequest);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    /// <summary>PutApplicationGroup_WithValidRequest_Returns200AndRotatesToken</summary>
    [Fact]
    public async Task PutApplicationGroup_WithValidRequest_Returns200AndRotatesToken()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/application-groups");
        postRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        postRequest.Headers.Add("X-Storage-Mode", "Team");
        postRequest.Content = JsonContent.Create(new CreateApplicationGroupRequest { Name = "UrsprünglicheGruppe" });
        var postResponse = await client.SendAsync(postRequest);
        postResponse.EnsureSuccessStatusCode();
        var created = await postResponse.Content.ReadFromJsonAsync<ApplicationGroupResponse>();
        postResponse.Headers.TryGetValues("X-New-Token", out var newTokenValues);
        token = newTokenValues!.First();

        var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/application-groups/{created!.Id}");
        putRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        putRequest.Headers.Add("X-Storage-Mode", "Team");
        putRequest.Content = JsonContent.Create(new UpdateApplicationGroupRequest { Name = "UmbenenntGruppe" });

        var putResponse = await client.SendAsync(putRequest);

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        Assert.True(putResponse.Headers.Contains("X-New-Token"));

        var body = await putResponse.Content.ReadFromJsonAsync<ApplicationGroupResponse>();
        Assert.NotNull(body);
        Assert.Equal("UmbenenntGruppe", body.Name);
    }

    /// <summary>PutApplicationGroup_WithInvalidId_Returns404</summary>
    [Fact]
    public async Task PutApplicationGroup_WithInvalidId_Returns404()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var putRequest = new HttpRequestMessage(HttpMethod.Put, "/api/application-groups/999999");
        putRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        putRequest.Headers.Add("X-Storage-Mode", "Team");
        putRequest.Content = JsonContent.Create(new UpdateApplicationGroupRequest { Name = "NichtVorhanden" });

        var putResponse = await client.SendAsync(putRequest);

        Assert.Equal(HttpStatusCode.NotFound, putResponse.StatusCode);
    }

    /// <summary>PutApplicationGroup_WithMissingName_Returns400</summary>
    [Fact]
    public async Task PutApplicationGroup_WithMissingName_Returns400()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/application-groups");
        postRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        postRequest.Headers.Add("X-Storage-Mode", "Team");
        postRequest.Content = JsonContent.Create(new CreateApplicationGroupRequest { Name = "PutValidierungsTest" });
        var postResponse = await client.SendAsync(postRequest);
        postResponse.EnsureSuccessStatusCode();
        var created = await postResponse.Content.ReadFromJsonAsync<ApplicationGroupResponse>();
        postResponse.Headers.TryGetValues("X-New-Token", out var newTokenValues);
        token = newTokenValues!.First();

        var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/application-groups/{created!.Id}");
        putRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        putRequest.Headers.Add("X-Storage-Mode", "Team");
        putRequest.Content = JsonContent.Create(new UpdateApplicationGroupRequest { Name = "" });

        var putResponse = await client.SendAsync(putRequest);

        Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);
    }

    /// <summary>DeleteApplicationGroup_WithValidId_Returns204AndRotatesToken</summary>
    [Fact]
    public async Task DeleteApplicationGroup_WithValidId_Returns204AndRotatesToken()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/application-groups");
        postRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        postRequest.Headers.Add("X-Storage-Mode", "Team");
        postRequest.Content = JsonContent.Create(new CreateApplicationGroupRequest { Name = "ZuLöschendeGruppe" });
        var postResponse = await client.SendAsync(postRequest);
        postResponse.EnsureSuccessStatusCode();
        var created = await postResponse.Content.ReadFromJsonAsync<ApplicationGroupResponse>();
        postResponse.Headers.TryGetValues("X-New-Token", out var newTokenValues);
        token = newTokenValues!.First();

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/application-groups/{created!.Id}");
        deleteRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        deleteRequest.Headers.Add("X-Storage-Mode", "Team");

        var deleteResponse = await client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.True(deleteResponse.Headers.Contains("X-New-Token"));
    }

    /// <summary>DeleteApplicationGroup_WithInvalidId_Returns404</summary>
    [Fact]
    public async Task DeleteApplicationGroup_WithInvalidId_Returns404()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/application-groups/999999");
        deleteRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        deleteRequest.Headers.Add("X-Storage-Mode", "Team");

        var deleteResponse = await client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    /// <summary>DeleteApplicationGroup_WithSystemGroup_Returns403</summary>
    [Fact]
    public async Task DeleteApplicationGroup_WithSystemGroup_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var systemGroup = await repo.AddGroupAsync(new ApplicationGroup
        {
            Name = "Schnittstellenzentrale",
            IsSystem = true
        });

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/application-groups/{systemGroup.Id}");
        deleteRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        deleteRequest.Headers.Add("X-Storage-Mode", "Team");

        var deleteResponse = await client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
    }

    /// <summary>PutApplicationGroup_WithSystemGroup_Returns403</summary>
    [Fact]
    public async Task PutApplicationGroup_WithSystemGroup_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var systemGroup = await repo.AddGroupAsync(new ApplicationGroup
        {
            Name = "Schnittstellenzentrale",
            IsSystem = true
        });

        var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/application-groups/{systemGroup.Id}");
        putRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        putRequest.Headers.Add("X-Storage-Mode", "Team");
        putRequest.Content = JsonContent.Create(new UpdateApplicationGroupRequest { Name = "NeuerName" });

        var putResponse = await client.SendAsync(putRequest);

        Assert.Equal(HttpStatusCode.Forbidden, putResponse.StatusCode);
    }
}
