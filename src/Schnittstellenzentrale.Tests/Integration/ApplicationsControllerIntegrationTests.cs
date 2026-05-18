using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Schnittstellenzentrale.Tests.Integration;

public class ApplicationsControllerIntegrationTests : IClassFixture<ControllerTestFactory>
{
    private readonly ControllerTestFactory _factory;

    public ApplicationsControllerIntegrationTests(ControllerTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostApplication_WithValidTokenAndRequest_Returns201AndLocation()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/applications");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Storage-Mode", "Team");
        request.Content = JsonContent.Create(new CreateApplicationRequest
        {
            Name = "TestApp",
            BaseUrl = "https://example.com"
        });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.True(response.Headers.Contains("X-New-Token"));

        var body = await response.Content.ReadFromJsonAsync<ApplicationResponse>();
        Assert.NotNull(body);
        Assert.Equal("TestApp", body.Name);
        Assert.Equal("https://example.com", body.BaseUrl);
        Assert.True(body.Id > 0);
    }

    [Fact]
    public async Task PostApplication_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/applications");
        request.Content = JsonContent.Create(new CreateApplicationRequest
        {
            Name = "TestApp",
            BaseUrl = "https://example.com"
        });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostApplication_WithMissingName_Returns400()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/applications");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Storage-Mode", "Team");
        request.Content = JsonContent.Create(new CreateApplicationRequest
        {
            Name = "",
            BaseUrl = "https://example.com"
        });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostApplication_WithMissingBaseUrl_Returns400()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/applications");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Storage-Mode", "Team");
        request.Content = JsonContent.Create(new CreateApplicationRequest
        {
            Name = "TestApp",
            BaseUrl = ""
        });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetApplications_WithValidToken_Returns200WithList()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/applications");
        postRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        postRequest.Headers.Add("X-Storage-Mode", "Team");
        postRequest.Content = JsonContent.Create(new CreateApplicationRequest
        {
            Name = "GetAllApp",
            BaseUrl = "https://getall.example.com"
        });
        var postResponse = await client.SendAsync(postRequest);
        postResponse.EnsureSuccessStatusCode();
        postResponse.Headers.TryGetValues("X-New-Token", out var newTokenValues);
        token = newTokenValues!.First();

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/applications");
        getRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        getRequest.Headers.Add("X-Storage-Mode", "Team");
        getRequest.Headers.Add("X-Owner", "");

        var getResponse = await client.SendAsync(getRequest);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.True(getResponse.Headers.Contains("X-New-Token"));

        var body = await getResponse.Content.ReadFromJsonAsync<IList<ApplicationResponse>>();
        Assert.NotNull(body);
        Assert.Contains(body, a => a.Name == "GetAllApp");
    }

    [Fact]
    public async Task GetApplications_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/applications");
        request.Headers.Add("X-Storage-Mode", "Team");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUngroupedApplications_WithValidToken_Returns200WithList()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/applications");
        postRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        postRequest.Headers.Add("X-Storage-Mode", "Team");
        postRequest.Content = JsonContent.Create(new CreateApplicationRequest
        {
            Name = "UngroupedApp",
            BaseUrl = "https://ungrouped.example.com"
        });
        var postResponse = await client.SendAsync(postRequest);
        postResponse.EnsureSuccessStatusCode();
        postResponse.Headers.TryGetValues("X-New-Token", out var newTokenValues);
        token = newTokenValues!.First();

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/applications/ungrouped");
        getRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        getRequest.Headers.Add("X-Storage-Mode", "Team");
        getRequest.Headers.Add("X-Owner", "");

        var getResponse = await client.SendAsync(getRequest);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.True(getResponse.Headers.Contains("X-New-Token"));

        var body = await getResponse.Content.ReadFromJsonAsync<IList<ApplicationResponse>>();
        Assert.NotNull(body);
        Assert.Contains(body, a => a.Name == "UngroupedApp");
    }

    [Fact]
    public async Task GetApplicationById_WithValidId_Returns200WithAllFields()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/applications");
        postRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        postRequest.Headers.Add("X-Storage-Mode", "Team");
        postRequest.Content = JsonContent.Create(new CreateApplicationRequest
        {
            Name = "GetByIdApp",
            BaseUrl = "https://getbyid.example.com",
            Description = "Testbeschreibung",
            InterfaceUrl = "https://getbyid.example.com/swagger",
            Owner = "owner1"
        });
        var postResponse = await client.SendAsync(postRequest);
        postResponse.EnsureSuccessStatusCode();
        var created = await postResponse.Content.ReadFromJsonAsync<ApplicationResponse>();
        postResponse.Headers.TryGetValues("X-New-Token", out var newTokenValues);
        token = newTokenValues!.First();

        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/applications/{created!.Id}");
        getRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var getResponse = await client.SendAsync(getRequest);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.True(getResponse.Headers.Contains("X-New-Token"));

        var body = await getResponse.Content.ReadFromJsonAsync<ApplicationResponse>();
        Assert.NotNull(body);
        Assert.Equal(created.Id, body.Id);
        Assert.Equal("GetByIdApp", body.Name);
        Assert.Equal("https://getbyid.example.com", body.BaseUrl);
        Assert.Equal("Testbeschreibung", body.Description);
        Assert.Equal("https://getbyid.example.com/swagger", body.InterfaceUrl);
        Assert.Equal((int)InterfaceType.Rest, body.InterfaceType);
        Assert.Equal("owner1", body.Owner);
    }

    [Fact]
    public async Task GetApplicationById_WithInvalidId_Returns404()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/applications/999999");
        getRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var getResponse = await client.SendAsync(getRequest);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task PutApplication_WithValidRequest_Returns200AndRotatesToken()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/applications");
        postRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        postRequest.Headers.Add("X-Storage-Mode", "Team");
        postRequest.Content = JsonContent.Create(new CreateApplicationRequest
        {
            Name = "UrsprünglicheApp",
            BaseUrl = "https://original.example.com"
        });
        var postResponse = await client.SendAsync(postRequest);
        postResponse.EnsureSuccessStatusCode();
        var created = await postResponse.Content.ReadFromJsonAsync<ApplicationResponse>();
        postResponse.Headers.TryGetValues("X-New-Token", out var newTokenValues);
        token = newTokenValues!.First();

        var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/applications/{created!.Id}");
        putRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        putRequest.Headers.Add("X-Storage-Mode", "Team");
        putRequest.Content = JsonContent.Create(new UpdateApplicationRequest
        {
            Name = "AktualisierteApp",
            BaseUrl = "https://updated.example.com",
            Description = "Neue Beschreibung"
        });

        var putResponse = await client.SendAsync(putRequest);

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        Assert.True(putResponse.Headers.Contains("X-New-Token"));

        var body = await putResponse.Content.ReadFromJsonAsync<ApplicationResponse>();
        Assert.NotNull(body);
        Assert.Equal("AktualisierteApp", body.Name);
        Assert.Equal("https://updated.example.com", body.BaseUrl);
        Assert.Equal("Neue Beschreibung", body.Description);
    }

    [Fact]
    public async Task PutApplication_WithInvalidId_Returns404()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var putRequest = new HttpRequestMessage(HttpMethod.Put, "/api/applications/999999");
        putRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        putRequest.Headers.Add("X-Storage-Mode", "Team");
        putRequest.Content = JsonContent.Create(new UpdateApplicationRequest
        {
            Name = "NichtVorhanden",
            BaseUrl = "https://notfound.example.com"
        });

        var putResponse = await client.SendAsync(putRequest);

        Assert.Equal(HttpStatusCode.NotFound, putResponse.StatusCode);
    }

    [Fact]
    public async Task PutApplication_WithMissingBaseUrl_Returns400()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/applications");
        postRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        postRequest.Headers.Add("X-Storage-Mode", "Team");
        postRequest.Content = JsonContent.Create(new CreateApplicationRequest
        {
            Name = "PutValidierungsApp",
            BaseUrl = "https://validation.example.com"
        });
        var postResponse = await client.SendAsync(postRequest);
        postResponse.EnsureSuccessStatusCode();
        var created = await postResponse.Content.ReadFromJsonAsync<ApplicationResponse>();
        postResponse.Headers.TryGetValues("X-New-Token", out var newTokenValues);
        token = newTokenValues!.First();

        var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/applications/{created!.Id}");
        putRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        putRequest.Headers.Add("X-Storage-Mode", "Team");
        putRequest.Content = JsonContent.Create(new UpdateApplicationRequest
        {
            Name = "PutValidierungsApp",
            BaseUrl = ""
        });

        var putResponse = await client.SendAsync(putRequest);

        Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteApplication_WithValidId_Returns204AndRotatesToken()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/applications");
        postRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        postRequest.Headers.Add("X-Storage-Mode", "Team");
        postRequest.Content = JsonContent.Create(new CreateApplicationRequest
        {
            Name = "ZuLöschendeApp",
            BaseUrl = "https://delete.example.com"
        });
        var postResponse = await client.SendAsync(postRequest);
        postResponse.EnsureSuccessStatusCode();
        var created = await postResponse.Content.ReadFromJsonAsync<ApplicationResponse>();
        postResponse.Headers.TryGetValues("X-New-Token", out var newTokenValues);
        token = newTokenValues!.First();

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/applications/{created!.Id}");
        deleteRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        deleteRequest.Headers.Add("X-Storage-Mode", "Team");

        var deleteResponse = await client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.True(deleteResponse.Headers.Contains("X-New-Token"));
    }

    [Fact]
    public async Task DeleteApplication_WithInvalidId_Returns404()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/applications/999999");
        deleteRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        deleteRequest.Headers.Add("X-Storage-Mode", "Team");

        var deleteResponse = await client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteApplication_WithSystemApplication_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var systemApp = await repo.AddApplicationAsync(new Application
        {
            Name = "Schnittstellenzentrale",
            IsSystem = true,
            BaseUrl = "https://localhost:5001"
        });

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/applications/{systemApp.Id}");
        deleteRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        deleteRequest.Headers.Add("X-Storage-Mode", "Team");

        var deleteResponse = await client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task PutApplication_WithSystemApplication_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var repo = _factory.Services.GetRequiredService<IApplicationRepository>();
        var systemApp = await repo.AddApplicationAsync(new Application
        {
            Name = "Schnittstellenzentrale",
            IsSystem = true,
            BaseUrl = "https://localhost:5001"
        });

        var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/applications/{systemApp.Id}");
        putRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        putRequest.Headers.Add("X-Storage-Mode", "Team");
        putRequest.Content = JsonContent.Create(new UpdateApplicationRequest
        {
            Name = "NeuerName",
            BaseUrl = "https://localhost:5001"
        });

        var putResponse = await client.SendAsync(putRequest);

        Assert.Equal(HttpStatusCode.Forbidden, putResponse.StatusCode);
    }
}
