using Microsoft.Extensions.Caching.Memory;

namespace OpenSenseMapAPI.Services;

public class TokenCacheService : ITokenCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<TokenCacheService> _logger;
    private readonly TimeSpan _tokenExpiration = TimeSpan.FromHours(24);

    public TokenCacheService(IMemoryCache cache, ILogger<TokenCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

 
    /// <summary>
    /// Sets the token in the cache
    /// </summary>
    /// <param name="email"></param>
    /// <param name="token"></param>
    /// <exception cref="ArgumentException"></exception>
    public void SetToken(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be empty", nameof(token));
        }
       
        var emailKey = GetCacheKey(email);
        var tokenKey = GetTokenKey(token);
        
        _cache.Set(emailKey, token, _tokenExpiration);
        _cache.Set(tokenKey, email.ToLowerInvariant(), _tokenExpiration);
        
        _logger.LogInformation("Token cached for user {Email}", email);
    }

    /// <summary>
    /// Retrieves the token from the cache
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public string? GetToken(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var cacheKey = GetCacheKey(email);
        return _cache.Get<string>(cacheKey);
    }

    /// <summary>
    /// Removes the token from the cache
    /// </summary>
    /// <param name="email"></param>
    /// <exception cref="ArgumentException"></exception>
    public void RemoveToken(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty", nameof(email));
        }
       
        var emailKey = GetCacheKey(email);
        var token = _cache.Get<string>(emailKey);
        
        _cache.Remove(emailKey);
        
        if (!string.IsNullOrWhiteSpace(token))
        {
            var tokenKey = GetTokenKey(token);
            _cache.Remove(tokenKey);
        }
        
        _logger.LogInformation("Token removed from cache for user {Email}", email);
    }   

    /// <summary>
    /// Gets the email associated with a token (reverse lookup)
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public string? GetEmailByToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var tokenKey = GetTokenKey(token);
        return _cache.Get<string>(tokenKey);
    }

    /// <summary>
    /// Generates the cache key
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    private static string GetCacheKey(string email)
    {
        return $"token_{email.ToLowerInvariant()}";
    }

    private static string GetTokenKey(string token)
    {
        return $"email_{token}";
    }
}
