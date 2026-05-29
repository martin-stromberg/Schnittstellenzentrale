using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>NavigationStateServiceTests</summary>
public class NavigationStateServiceTests
{
    /// <summary>NavigationStateService_SetArea_FeuertOnAreaChanged</summary>
    [Fact]
    public async Task NavigationStateService_SetArea_FeuertOnAreaChanged()
    {
        var service = new NavigationStateService();
        var fired = false;
        service.OnAreaChanged += () => fired = true;

        await service.SetAreaAsync(NavigationArea.Environments);

        Assert.True(fired);
        Assert.Equal(NavigationArea.Environments, service.CurrentArea);
    }

    /// <summary>NavigationStateService_SetSelection_FeuertOnSelectionChanged</summary>
    [Fact]
    public async Task NavigationStateService_SetSelection_FeuertOnSelectionChanged()
    {
        var service = new NavigationStateService();
        var fired = false;
        service.OnSelectionChanged += () => fired = true;

        var group = new ApplicationGroup { Id = 1, Name = "Test" };
        var selection = new WorkspaceSelection(group, [group]);
        await service.SetWorkspaceSelectionAsync(selection);

        Assert.True(fired);
        Assert.Equal(selection, service.CurrentSelection);
    }
}
