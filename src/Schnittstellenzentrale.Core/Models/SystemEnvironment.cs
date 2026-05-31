using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Models;

public class SystemEnvironment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public StorageMode Mode { get; set; }
    public string? Owner { get; set; }
    public string? Description { get; set; }
    public ICollection<EnvironmentVariable> Variables { get; set; } = [];
}
