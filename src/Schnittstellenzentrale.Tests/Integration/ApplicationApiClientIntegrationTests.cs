using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Tests.Helpers;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Schnittstellenzentrale.Tests.Integration;

/// <summary>Integrationstests für EndpointGroupsController und EndpointsController.</summary>
public class ApplicationApiClientIntegrationTests : IClassFixture<ControllerTestFactory>
{
    private readonly ControllerTestFactory _factory;

    /// <summary>Initialisiert den Test mit der gemeinsamen Controller-Factory.</summary>
    public ApplicationApiClientIntegrationTests(ControllerTestFactory factory)
    {
        _factory = factory;
    }

    private static HttpRequestMessage BuildRequest(HttpMethod method, string url, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Storage-Mode", "Team");
        if (body != null)
            request.Content = JsonContent.Create(body);
        return request;
    }

    private static string ExtractNewToken(HttpResponseMessage response)
    {
        response.Headers.TryGetValues("X-New-Token", out var values);
        return values!.First();
    }

    // ─── EndpointGroups ────────────────────────────────────────────────────────

    /// <summary>PostEndpointGroup_WithValidTokenAndRequest_Returns201AndBody</summary>
    [Fact]
    public async Task PostEndpointGroup_WithValidTokenAndRequest_Returns201AndBody()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postApp = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/applications", token,
            new CreateApplicationRequest { Name = "AppForEG", BaseUrl = "https://eg.example.com" }));
        postApp.EnsureSuccessStatusCode();
        var createdApp = await postApp.Content.ReadFromJsonAsync<ApplicationResponse>();
        token = ExtractNewToken(postApp);

        var response = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoint-groups", token,
            new CreateEndpointGroupRequest { Name = "TestGruppe", ApplicationId = createdApp!.Id }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(response.Headers.Contains("X-New-Token"));

        var body = await response.Content.ReadFromJsonAsync<EndpointGroupResponse>();
        Assert.NotNull(body);
        Assert.Equal("TestGruppe", body.Name);
        Assert.Equal(createdApp.Id, body.ApplicationId);
        Assert.True(body.Id > 0);
    }

    /// <summary>PostEndpointGroup_WithoutToken_Returns401</summary>
    [Fact]
    public async Task PostEndpointGroup_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/endpoint-groups");
        request.Content = JsonContent.Create(new CreateEndpointGroupRequest { Name = "TestGruppe", ApplicationId = 1 });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>GetEndpointGroups_WithValidToken_Returns200WithList</summary>
    [Fact]
    public async Task GetEndpointGroups_WithValidToken_Returns200WithList()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postApp = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/applications", token,
            new CreateApplicationRequest { Name = "AppForGetGroups", BaseUrl = "https://getgroups.example.com" }));
        postApp.EnsureSuccessStatusCode();
        var createdApp = await postApp.Content.ReadFromJsonAsync<ApplicationResponse>();
        token = ExtractNewToken(postApp);

        var postGroup = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoint-groups", token,
            new CreateEndpointGroupRequest { Name = "GetListGruppe", ApplicationId = createdApp!.Id }));
        postGroup.EnsureSuccessStatusCode();
        token = ExtractNewToken(postGroup);

        var response = await client.SendAsync(BuildRequest(HttpMethod.Get, $"/api/endpoint-groups?applicationId={createdApp.Id}", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<IList<EndpointGroupResponse>>();
        Assert.NotNull(body);
        Assert.Contains(body, g => g.Name == "GetListGruppe");
    }

    /// <summary>GetEndpointGroups_WithoutToken_Returns401</summary>
    [Fact]
    public async Task GetEndpointGroups_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/endpoint-groups?applicationId=1"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>GetEndpointGroupById_WithValidId_Returns200</summary>
    [Fact]
    public async Task GetEndpointGroupById_WithValidId_Returns200()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postApp = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/applications", token,
            new CreateApplicationRequest { Name = "AppForGetGroupById", BaseUrl = "https://getgroupbyid.example.com" }));
        postApp.EnsureSuccessStatusCode();
        var createdApp = await postApp.Content.ReadFromJsonAsync<ApplicationResponse>();
        token = ExtractNewToken(postApp);

        var postGroup = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoint-groups", token,
            new CreateEndpointGroupRequest { Name = "GetByIdGruppe", ApplicationId = createdApp!.Id }));
        postGroup.EnsureSuccessStatusCode();
        var createdGroup = await postGroup.Content.ReadFromJsonAsync<EndpointGroupResponse>();
        token = ExtractNewToken(postGroup);

        var response = await client.SendAsync(BuildRequest(HttpMethod.Get, $"/api/endpoint-groups/{createdGroup!.Id}", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<EndpointGroupResponse>();
        Assert.NotNull(body);
        Assert.Equal(createdGroup.Id, body.Id);
        Assert.Equal("GetByIdGruppe", body.Name);
    }

    /// <summary>GetEndpointGroupById_WithInvalidId_Returns404</summary>
    [Fact]
    public async Task GetEndpointGroupById_WithInvalidId_Returns404()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var response = await client.SendAsync(BuildRequest(HttpMethod.Get, "/api/endpoint-groups/999999", token));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>PutEndpointGroup_WithValidRequest_Returns200</summary>
    [Fact]
    public async Task PutEndpointGroup_WithValidRequest_Returns200()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postApp = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/applications", token,
            new CreateApplicationRequest { Name = "AppForPutGroup", BaseUrl = "https://putgroup.example.com" }));
        postApp.EnsureSuccessStatusCode();
        var createdApp = await postApp.Content.ReadFromJsonAsync<ApplicationResponse>();
        token = ExtractNewToken(postApp);

        var postGroup = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoint-groups", token,
            new CreateEndpointGroupRequest { Name = "OriginalGruppe", ApplicationId = createdApp!.Id }));
        postGroup.EnsureSuccessStatusCode();
        var createdGroup = await postGroup.Content.ReadFromJsonAsync<EndpointGroupResponse>();
        token = ExtractNewToken(postGroup);

        var putResponse = await client.SendAsync(BuildRequest(HttpMethod.Put, $"/api/endpoint-groups/{createdGroup!.Id}", token,
            new UpdateEndpointGroupRequest { Name = "UmbenenntGruppe", RowVersion = createdGroup.RowVersion }));

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        var body = await putResponse.Content.ReadFromJsonAsync<EndpointGroupResponse>();
        Assert.NotNull(body);
        Assert.Equal("UmbenenntGruppe", body.Name);
    }

    /// <summary>DeleteEndpointGroup_WithValidId_Returns204</summary>
    [Fact]
    public async Task DeleteEndpointGroup_WithValidId_Returns204()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postApp = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/applications", token,
            new CreateApplicationRequest { Name = "AppForDeleteGroup", BaseUrl = "https://deletegroup.example.com" }));
        postApp.EnsureSuccessStatusCode();
        var createdApp = await postApp.Content.ReadFromJsonAsync<ApplicationResponse>();
        token = ExtractNewToken(postApp);

        var postGroup = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoint-groups", token,
            new CreateEndpointGroupRequest { Name = "ZuLoeschendeGruppe", ApplicationId = createdApp!.Id }));
        postGroup.EnsureSuccessStatusCode();
        var createdGroup = await postGroup.Content.ReadFromJsonAsync<EndpointGroupResponse>();
        token = ExtractNewToken(postGroup);

        var deleteResponse = await client.SendAsync(BuildRequest(HttpMethod.Delete, $"/api/endpoint-groups/{createdGroup!.Id}", token));

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    /// <summary>DeleteEndpointGroup_WithInvalidId_Returns404</summary>
    [Fact]
    public async Task DeleteEndpointGroup_WithInvalidId_Returns404()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var response = await client.SendAsync(BuildRequest(HttpMethod.Delete, "/api/endpoint-groups/999999", token));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── Endpoints ─────────────────────────────────────────────────────────────

    /// <summary>PostEndpoint_WithValidTokenAndRequest_Returns201AndBody</summary>
    [Fact]
    public async Task PostEndpoint_WithValidTokenAndRequest_Returns201AndBody()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postApp = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/applications", token,
            new CreateApplicationRequest { Name = "AppForEndpoint", BaseUrl = "https://endpoint.example.com" }));
        postApp.EnsureSuccessStatusCode();
        var createdApp = await postApp.Content.ReadFromJsonAsync<ApplicationResponse>();
        token = ExtractNewToken(postApp);

        var response = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoints", token,
            new CreateEndpointRequest { Name = "TestEndpoint", RelativePath = "/api/items", ApplicationId = createdApp!.Id }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(response.Headers.Contains("X-New-Token"));

        var body = await response.Content.ReadFromJsonAsync<EndpointResponse>();
        Assert.NotNull(body);
        Assert.Equal("TestEndpoint", body.Name);
        Assert.Equal("/api/items", body.RelativePath);
        Assert.Equal(createdApp.Id, body.ApplicationId);
        Assert.True(body.Id > 0);
    }

    /// <summary>PostEndpoint_WithoutToken_Returns401</summary>
    [Fact]
    public async Task PostEndpoint_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/endpoints");
        request.Content = JsonContent.Create(new CreateEndpointRequest { Name = "Test", RelativePath = "/test", ApplicationId = 1 });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>GetEndpoints_WithValidToken_Returns200WithList</summary>
    [Fact]
    public async Task GetEndpoints_WithValidToken_Returns200WithList()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postApp = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/applications", token,
            new CreateApplicationRequest { Name = "AppForGetEndpoints", BaseUrl = "https://getendpoints.example.com" }));
        postApp.EnsureSuccessStatusCode();
        var createdApp = await postApp.Content.ReadFromJsonAsync<ApplicationResponse>();
        token = ExtractNewToken(postApp);

        var postEndpoint = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoints", token,
            new CreateEndpointRequest { Name = "ListEndpoint", RelativePath = "/api/list", ApplicationId = createdApp!.Id }));
        postEndpoint.EnsureSuccessStatusCode();
        token = ExtractNewToken(postEndpoint);

        var response = await client.SendAsync(BuildRequest(HttpMethod.Get, $"/api/endpoints?applicationId={createdApp.Id}", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<IList<EndpointResponse>>();
        Assert.NotNull(body);
        Assert.Contains(body, e => e.Name == "ListEndpoint");
    }

    /// <summary>GetEndpoints_WithGroupId_ReturnsFilteredList</summary>
    [Fact]
    public async Task GetEndpoints_WithGroupId_ReturnsFilteredList()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postApp = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/applications", token,
            new CreateApplicationRequest { Name = "AppForGroupFilter", BaseUrl = "https://groupfilter.example.com" }));
        postApp.EnsureSuccessStatusCode();
        var createdApp = await postApp.Content.ReadFromJsonAsync<ApplicationResponse>();
        token = ExtractNewToken(postApp);

        var postGroup = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoint-groups", token,
            new CreateEndpointGroupRequest { Name = "FilterGruppe", ApplicationId = createdApp!.Id }));
        postGroup.EnsureSuccessStatusCode();
        var createdGroup = await postGroup.Content.ReadFromJsonAsync<EndpointGroupResponse>();
        token = ExtractNewToken(postGroup);

        var postEndpointInGroup = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoints", token,
            new CreateEndpointRequest { Name = "InGroup", RelativePath = "/api/ingroup", ApplicationId = createdApp.Id, EndpointGroupId = createdGroup!.Id }));
        postEndpointInGroup.EnsureSuccessStatusCode();
        token = ExtractNewToken(postEndpointInGroup);

        var postEndpointNoGroup = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoints", token,
            new CreateEndpointRequest { Name = "NoGroup", RelativePath = "/api/nogroup", ApplicationId = createdApp.Id }));
        postEndpointNoGroup.EnsureSuccessStatusCode();
        token = ExtractNewToken(postEndpointNoGroup);

        var response = await client.SendAsync(BuildRequest(HttpMethod.Get,
            $"/api/endpoints?applicationId={createdApp.Id}&groupId={createdGroup.Id}", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<IList<EndpointResponse>>();
        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal("InGroup", body[0].Name);
    }

    /// <summary>GetEndpoints_WithoutToken_Returns401</summary>
    [Fact]
    public async Task GetEndpoints_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/endpoints?applicationId=1"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>GetEndpointById_WithValidId_Returns200</summary>
    [Fact]
    public async Task GetEndpointById_WithValidId_Returns200()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postApp = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/applications", token,
            new CreateApplicationRequest { Name = "AppForGetEndpointById", BaseUrl = "https://getendpointbyid.example.com" }));
        postApp.EnsureSuccessStatusCode();
        var createdApp = await postApp.Content.ReadFromJsonAsync<ApplicationResponse>();
        token = ExtractNewToken(postApp);

        var postEndpoint = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoints", token,
            new CreateEndpointRequest { Name = "GetByIdEndpoint", RelativePath = "/api/byid", ApplicationId = createdApp!.Id }));
        postEndpoint.EnsureSuccessStatusCode();
        var createdEndpoint = await postEndpoint.Content.ReadFromJsonAsync<EndpointResponse>();
        token = ExtractNewToken(postEndpoint);

        var response = await client.SendAsync(BuildRequest(HttpMethod.Get, $"/api/endpoints/{createdEndpoint!.Id}", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<EndpointResponse>();
        Assert.NotNull(body);
        Assert.Equal(createdEndpoint.Id, body.Id);
        Assert.Equal("GetByIdEndpoint", body.Name);
    }

    /// <summary>GetEndpointById_WithInvalidId_Returns404</summary>
    [Fact]
    public async Task GetEndpointById_WithInvalidId_Returns404()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var response = await client.SendAsync(BuildRequest(HttpMethod.Get, "/api/endpoints/999999", token));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>PutEndpoint_WithValidRequest_Returns200</summary>
    [Fact]
    public async Task PutEndpoint_WithValidRequest_Returns200()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postApp = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/applications", token,
            new CreateApplicationRequest { Name = "AppForPutEndpoint", BaseUrl = "https://putendpoint.example.com" }));
        postApp.EnsureSuccessStatusCode();
        var createdApp = await postApp.Content.ReadFromJsonAsync<ApplicationResponse>();
        token = ExtractNewToken(postApp);

        var postEndpoint = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoints", token,
            new CreateEndpointRequest { Name = "OriginalEndpoint", RelativePath = "/api/original", ApplicationId = createdApp!.Id }));
        postEndpoint.EnsureSuccessStatusCode();
        var createdEndpoint = await postEndpoint.Content.ReadFromJsonAsync<EndpointResponse>();
        token = ExtractNewToken(postEndpoint);

        var putResponse = await client.SendAsync(BuildRequest(HttpMethod.Put, $"/api/endpoints/{createdEndpoint!.Id}", token,
            new UpdateEndpointRequest { Name = "AktualisierterEndpoint", RelativePath = "/api/updated", RowVersion = createdEndpoint.RowVersion }));

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        var body = await putResponse.Content.ReadFromJsonAsync<EndpointResponse>();
        Assert.NotNull(body);
        Assert.Equal("AktualisierterEndpoint", body.Name);
        Assert.Equal("/api/updated", body.RelativePath);
    }

    /// <summary>PutEndpoint_WithInvalidId_Returns404</summary>
    [Fact]
    public async Task PutEndpoint_WithInvalidId_Returns404()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var putResponse = await client.SendAsync(BuildRequest(HttpMethod.Put, "/api/endpoints/999999", token,
            new UpdateEndpointRequest { Name = "NichtVorhanden", RelativePath = "/api/notfound" }));

        Assert.Equal(HttpStatusCode.NotFound, putResponse.StatusCode);
    }

    /// <summary>DeleteEndpoint_WithValidId_Returns204</summary>
    [Fact]
    public async Task DeleteEndpoint_WithValidId_Returns204()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postApp = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/applications", token,
            new CreateApplicationRequest { Name = "AppForDeleteEndpoint", BaseUrl = "https://deleteendpoint.example.com" }));
        postApp.EnsureSuccessStatusCode();
        var createdApp = await postApp.Content.ReadFromJsonAsync<ApplicationResponse>();
        token = ExtractNewToken(postApp);

        var postEndpoint = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoints", token,
            new CreateEndpointRequest { Name = "ZuLoeschenderEndpoint", RelativePath = "/api/delete", ApplicationId = createdApp!.Id }));
        postEndpoint.EnsureSuccessStatusCode();
        var createdEndpoint = await postEndpoint.Content.ReadFromJsonAsync<EndpointResponse>();
        token = ExtractNewToken(postEndpoint);

        var deleteResponse = await client.SendAsync(BuildRequest(HttpMethod.Delete, $"/api/endpoints/{createdEndpoint!.Id}", token));

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    /// <summary>DeleteEndpoint_WithInvalidId_Returns404</summary>
    [Fact]
    public async Task DeleteEndpoint_WithInvalidId_Returns404()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var response = await client.SendAsync(BuildRequest(HttpMethod.Delete, "/api/endpoints/999999", token));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── Headers ────────────────────────────────────────────────────────────────

    /// <summary>PostHeader_WithValidRequest_Returns201AndBody</summary>
    [Fact]
    public async Task PostHeader_WithValidRequest_Returns201AndBody()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postApp = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/applications", token,
            new CreateApplicationRequest { Name = "AppForHeader", BaseUrl = "https://header.example.com" }));
        postApp.EnsureSuccessStatusCode();
        var createdApp = await postApp.Content.ReadFromJsonAsync<ApplicationResponse>();
        token = ExtractNewToken(postApp);

        var postEndpoint = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoints", token,
            new CreateEndpointRequest { Name = "EndpointForHeader", RelativePath = "/api/hdr", ApplicationId = createdApp!.Id }));
        postEndpoint.EnsureSuccessStatusCode();
        var createdEndpoint = await postEndpoint.Content.ReadFromJsonAsync<EndpointResponse>();
        token = ExtractNewToken(postEndpoint);

        var response = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoints/headers", token,
            new AddEndpointKeyValueRequest { Key = "Accept", Value = "application/json", EndpointId = createdEndpoint!.Id }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<EndpointKeyValueResponse>();
        Assert.NotNull(body);
        Assert.Equal("Accept", body.Key);
        Assert.Equal("application/json", body.Value);
        Assert.Equal(createdEndpoint.Id, body.EndpointId);
        Assert.True(body.Id > 0);
    }

    /// <summary>PostHeader_WithoutToken_Returns401</summary>
    [Fact]
    public async Task PostHeader_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/endpoints/headers");
        request.Content = JsonContent.Create(new AddEndpointKeyValueRequest { Key = "Accept", Value = "application/json", EndpointId = 1 });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>DeleteHeader_WithValidId_Returns204</summary>
    [Fact]
    public async Task DeleteHeader_WithValidId_Returns204()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postApp = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/applications", token,
            new CreateApplicationRequest { Name = "AppForDeleteHeader", BaseUrl = "https://deletehdr.example.com" }));
        postApp.EnsureSuccessStatusCode();
        var createdApp = await postApp.Content.ReadFromJsonAsync<ApplicationResponse>();
        token = ExtractNewToken(postApp);

        var postEndpoint = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoints", token,
            new CreateEndpointRequest { Name = "EndpointForDeleteHeader", RelativePath = "/api/delhdr", ApplicationId = createdApp!.Id }));
        postEndpoint.EnsureSuccessStatusCode();
        var createdEndpoint = await postEndpoint.Content.ReadFromJsonAsync<EndpointResponse>();
        token = ExtractNewToken(postEndpoint);

        var postHeader = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoints/headers", token,
            new AddEndpointKeyValueRequest { Key = "X-Custom", Value = "val", EndpointId = createdEndpoint!.Id }));
        postHeader.EnsureSuccessStatusCode();
        var createdHeader = await postHeader.Content.ReadFromJsonAsync<EndpointKeyValueResponse>();
        token = ExtractNewToken(postHeader);

        var deleteResponse = await client.SendAsync(BuildRequest(HttpMethod.Delete, $"/api/endpoints/headers/{createdHeader!.Id}", token));

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    /// <summary>DeleteHeader_WithInvalidId_Returns404</summary>
    [Fact]
    public async Task DeleteHeader_WithInvalidId_Returns404()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var response = await client.SendAsync(BuildRequest(HttpMethod.Delete, "/api/endpoints/headers/999999", token));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── QueryParameters ────────────────────────────────────────────────────────

    /// <summary>PostQueryParameter_WithValidRequest_Returns201AndBody</summary>
    [Fact]
    public async Task PostQueryParameter_WithValidRequest_Returns201AndBody()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postApp = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/applications", token,
            new CreateApplicationRequest { Name = "AppForQP", BaseUrl = "https://qp.example.com" }));
        postApp.EnsureSuccessStatusCode();
        var createdApp = await postApp.Content.ReadFromJsonAsync<ApplicationResponse>();
        token = ExtractNewToken(postApp);

        var postEndpoint = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoints", token,
            new CreateEndpointRequest { Name = "EndpointForQP", RelativePath = "/api/qp", ApplicationId = createdApp!.Id }));
        postEndpoint.EnsureSuccessStatusCode();
        var createdEndpoint = await postEndpoint.Content.ReadFromJsonAsync<EndpointResponse>();
        token = ExtractNewToken(postEndpoint);

        var response = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoints/query-parameters", token,
            new AddEndpointKeyValueRequest { Key = "page", Value = "1", EndpointId = createdEndpoint!.Id }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<EndpointKeyValueResponse>();
        Assert.NotNull(body);
        Assert.Equal("page", body.Key);
        Assert.Equal("1", body.Value);
        Assert.Equal(createdEndpoint.Id, body.EndpointId);
        Assert.True(body.Id > 0);
    }

    /// <summary>PostQueryParameter_WithoutToken_Returns401</summary>
    [Fact]
    public async Task PostQueryParameter_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/endpoints/query-parameters");
        request.Content = JsonContent.Create(new AddEndpointKeyValueRequest { Key = "page", Value = "1", EndpointId = 1 });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>DeleteQueryParameter_WithInvalidId_Returns404</summary>
    [Fact]
    public async Task DeleteQueryParameter_WithInvalidId_Returns404()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var response = await client.SendAsync(BuildRequest(HttpMethod.Delete, "/api/endpoints/query-parameters/999999", token));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>DeleteQueryParameter_WithValidId_Returns204</summary>
    [Fact]
    public async Task DeleteQueryParameter_WithValidId_Returns204()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var postApp = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/applications", token,
            new CreateApplicationRequest { Name = "AppForDeleteQP", BaseUrl = "https://deleteqp.example.com" }));
        postApp.EnsureSuccessStatusCode();
        var createdApp = await postApp.Content.ReadFromJsonAsync<ApplicationResponse>();
        token = ExtractNewToken(postApp);

        var postEndpoint = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoints", token,
            new CreateEndpointRequest { Name = "EndpointForDeleteQP", RelativePath = "/api/delqp", ApplicationId = createdApp!.Id }));
        postEndpoint.EnsureSuccessStatusCode();
        var createdEndpoint = await postEndpoint.Content.ReadFromJsonAsync<EndpointResponse>();
        token = ExtractNewToken(postEndpoint);

        var postQP = await client.SendAsync(BuildRequest(HttpMethod.Post, "/api/endpoints/query-parameters", token,
            new AddEndpointKeyValueRequest { Key = "filter", Value = "active", EndpointId = createdEndpoint!.Id }));
        postQP.EnsureSuccessStatusCode();
        var createdQP = await postQP.Content.ReadFromJsonAsync<EndpointKeyValueResponse>();
        token = ExtractNewToken(postQP);

        var deleteResponse = await client.SendAsync(BuildRequest(HttpMethod.Delete, $"/api/endpoints/query-parameters/{createdQP!.Id}", token));

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    // ─── SystemEnvironments ─────────────────────────────────────────────────────

    /// <summary>GetSystemEnvironmentById_WithValidToken_Returns200WithBody</summary>
    [Fact]
    public async Task GetSystemEnvironmentById_WithValidToken_Returns200WithBody()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ISystemEnvironmentRepository>();
        var env = await repo.AddAsync(new Schnittstellenzentrale.Core.Models.SystemEnvironment
        {
            Name = "IntegrationEnv",
            Mode = Schnittstellenzentrale.Core.Enums.StorageMode.Team
        });

        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var response = await client.SendAsync(BuildRequest(HttpMethod.Get, $"/api/system-environments/{env.Id}", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SystemEnvironmentResponse>();
        Assert.NotNull(body);
        Assert.Equal(env.Id, body.Id);
        Assert.Equal("IntegrationEnv", body.Name);
    }

    /// <summary>GetSystemEnvironmentById_WithNonExistentId_Returns404</summary>
    [Fact]
    public async Task GetSystemEnvironmentById_WithNonExistentId_Returns404()
    {
        var client = _factory.CreateClient();
        var token = await _factory.ObtainTokenAsync(client);

        var response = await client.SendAsync(BuildRequest(HttpMethod.Get, "/api/system-environments/999999", token));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
