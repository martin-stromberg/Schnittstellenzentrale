using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Models;

public class ActivityLogEntry
{
    public DateTime Timestamp { get; init; }
    public ActivityLogCategory Category { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Details { get; init; }
}
