namespace OpenSenseMapAPI.Services;

public interface ITokenCacheService
{
    void SetToken(string email, string token);
    string? GetToken(string email);
    void RemoveToken(string email);
    string? GetEmailByToken(string token);
}
