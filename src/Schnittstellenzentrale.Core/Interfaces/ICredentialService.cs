namespace Schnittstellenzentrale.Core.Interfaces;

public interface ICredentialService
{
    string? GetPassword(string target);
    void SavePassword(string target, string username, string password);
    void DeletePassword(string target);
}
