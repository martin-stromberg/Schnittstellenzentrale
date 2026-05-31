namespace Schnittstellenzentrale.Core.Interfaces;

public interface IApplicationGroupService
{
    Task UpdateNameAsync(int groupId, string name);
    Task UpdateDescriptionAsync(int groupId, string? description);
    Task UpdateSubtitleAsync(int groupId, string? subtitle);
    Task UpdateIconAsync(int groupId, byte[] iconData);
}
