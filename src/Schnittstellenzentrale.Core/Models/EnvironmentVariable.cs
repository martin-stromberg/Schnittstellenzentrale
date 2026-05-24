namespace Schnittstellenzentrale.Core.Models;

public class EnvironmentVariable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsValueMasked { get; set; }
    public int SystemEnvironmentId { get; set; }
    public SystemEnvironment? SystemEnvironment { get; set; }
}
