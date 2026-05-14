namespace Schnittstellenzentrale.Core.Interfaces;

public interface ISignalRNotificationService
{
    Task NotifyApplicationChangedAsync(int applicationId);
    Task NotifyGroupChangedAsync(int groupId);
}
