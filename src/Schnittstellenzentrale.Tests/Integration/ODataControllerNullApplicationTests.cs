using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.OData;
using Endpoint = Schnittstellenzentrale.Core.Models.Endpoint;

namespace Schnittstellenzentrale.Tests.Integration;

/// <summary>Unit-Tests für den IsSystem-Guard bei verwaisten Endpoints und EndpointGroups (Application == null).</summary>
public class ODataControllerNullApplicationTests
{
    private static ODataEndpointsController CreateEndpointsController(
        Mock<IEndpointRepository> endpointRepoMock,
        Mock<IApplicationRepository> appRepoMock)
    {
        var tokenStoreMock = new Mock<ITokenStore>();
        var controller = new ODataEndpointsController(tokenStoreMock.Object, endpointRepoMock.Object, appRepoMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    private static ODataEndpointGroupsController CreateEndpointGroupsController(
        Mock<IEndpointRepository> endpointRepoMock,
        Mock<IApplicationRepository> appRepoMock)
    {
        var tokenStoreMock = new Mock<ITokenStore>();
        var controller = new ODataEndpointGroupsController(tokenStoreMock.Object, endpointRepoMock.Object, appRepoMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    /// <summary>Put_EndpointWithNullApplication_Returns404</summary>
    [Fact]
    public async Task Put_EndpointWithNullApplication_Returns404()
    {
        var endpointRepoMock = new Mock<IEndpointRepository>();
        var appRepoMock = new Mock<IApplicationRepository>();

        var orphanedEndpoint = new Endpoint
        {
            Id = 1,
            Name = "Orphaned",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/test",
            ApplicationId = 99,
            Application = null!
        };
        endpointRepoMock.Setup(r => r.GetEndpointByIdAsync(1)).ReturnsAsync(orphanedEndpoint);

        var controller = CreateEndpointsController(endpointRepoMock, appRepoMock);

        var result = await controller.Put(1, new Endpoint { Name = "Changed", Method = Core.Enums.HttpMethod.GET, RelativePath = "/test" });

        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>Patch_EndpointWithNullApplication_Returns404</summary>
    [Fact]
    public async Task Patch_EndpointWithNullApplication_Returns404()
    {
        var endpointRepoMock = new Mock<IEndpointRepository>();
        var appRepoMock = new Mock<IApplicationRepository>();

        var orphanedEndpoint = new Endpoint
        {
            Id = 1,
            Name = "Orphaned",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/test",
            ApplicationId = 99,
            Application = null!
        };
        endpointRepoMock.Setup(r => r.GetEndpointByIdAsync(1)).ReturnsAsync(orphanedEndpoint);

        var controller = CreateEndpointsController(endpointRepoMock, appRepoMock);
        var patch = System.Text.Json.JsonDocument.Parse("""{"Name":"Changed"}""").RootElement;

        var result = await controller.Patch(1, patch);

        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>Delete_EndpointWithNullApplication_Returns404</summary>
    [Fact]
    public async Task Delete_EndpointWithNullApplication_Returns404()
    {
        var endpointRepoMock = new Mock<IEndpointRepository>();
        var appRepoMock = new Mock<IApplicationRepository>();

        var orphanedEndpoint = new Endpoint
        {
            Id = 1,
            Name = "Orphaned",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/test",
            ApplicationId = 99,
            Application = null!
        };
        endpointRepoMock.Setup(r => r.GetEndpointByIdAsync(1)).ReturnsAsync(orphanedEndpoint);

        var controller = CreateEndpointsController(endpointRepoMock, appRepoMock);

        var result = await controller.Delete(1);

        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>Put_EndpointGroupWithNullApplication_Returns404</summary>
    [Fact]
    public async Task Put_EndpointGroupWithNullApplication_Returns404()
    {
        var endpointRepoMock = new Mock<IEndpointRepository>();
        var appRepoMock = new Mock<IApplicationRepository>();

        var orphanedGroup = new EndpointGroup
        {
            Id = 1,
            Name = "Orphaned",
            ApplicationId = 99,
            Application = null!
        };
        endpointRepoMock.Setup(r => r.GetEndpointGroupByIdAsync(1)).ReturnsAsync(orphanedGroup);

        var controller = CreateEndpointGroupsController(endpointRepoMock, appRepoMock);

        var result = await controller.Put(1, new EndpointGroup { Name = "Changed", ApplicationId = 99 });

        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>Patch_EndpointGroupWithNullApplication_Returns404</summary>
    [Fact]
    public async Task Patch_EndpointGroupWithNullApplication_Returns404()
    {
        var endpointRepoMock = new Mock<IEndpointRepository>();
        var appRepoMock = new Mock<IApplicationRepository>();

        var orphanedGroup = new EndpointGroup
        {
            Id = 1,
            Name = "Orphaned",
            ApplicationId = 99,
            Application = null!
        };
        endpointRepoMock.Setup(r => r.GetEndpointGroupByIdAsync(1)).ReturnsAsync(orphanedGroup);

        var controller = CreateEndpointGroupsController(endpointRepoMock, appRepoMock);
        var patch = System.Text.Json.JsonDocument.Parse("""{"Name":"Changed"}""").RootElement;

        var result = await controller.Patch(1, patch);

        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>Delete_EndpointGroupWithNullApplication_Returns404</summary>
    [Fact]
    public async Task Delete_EndpointGroupWithNullApplication_Returns404()
    {
        var endpointRepoMock = new Mock<IEndpointRepository>();
        var appRepoMock = new Mock<IApplicationRepository>();

        var orphanedGroup = new EndpointGroup
        {
            Id = 1,
            Name = "Orphaned",
            ApplicationId = 99,
            Application = null!
        };
        endpointRepoMock.Setup(r => r.GetEndpointGroupByIdAsync(1)).ReturnsAsync(orphanedGroup);

        var controller = CreateEndpointGroupsController(endpointRepoMock, appRepoMock);

        var result = await controller.Delete(1);

        Assert.IsType<NotFoundResult>(result);
    }
}
