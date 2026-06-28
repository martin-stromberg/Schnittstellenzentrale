namespace Schnittstellenzentrale.Core.Contracts;

public class SystemEnvironmentResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Mode { get; set; }
    public string? Owner { get; set; }
    public string? Description { get; set; }
    public IList<EnvironmentVariableResponse> Variables { get; set; } = [];
}
