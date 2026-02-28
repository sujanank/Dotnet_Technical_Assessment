using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using OpenSenseMapAPI.Services;
using Xunit;

namespace OpenSenseMapAPI.Tests.Services;

public class TokenCacheServiceTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<TokenCacheService>> _loggerMock;
    private readonly TokenCacheService _tokenCacheService;

    public TokenCacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<TokenCacheService>>();
        _tokenCacheService = new TokenCacheService(_memoryCache, _loggerMock.Object);
    }

    [Fact]
    public void SetToken_ValidEmailAndToken_StoresTokenInCache()
    {
        var email = "test@example.com";
        var token = "test-token-123";

        _tokenCacheService.SetToken(email, token);

        var retrievedToken = _tokenCacheService.GetToken(email);
        Assert.Equal(token, retrievedToken);
    }  

    [Fact]
    public void SetToken_EmptyEmail_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => _tokenCacheService.SetToken("", "token"));
        Assert.Contains("Email cannot be empty", exception.Message);
    }

    [Fact]
    public void SetToken_EmptyToken_ThrowsArgumentException()
    {
        var email = "test@example.com";
        var exception = Assert.Throws<ArgumentException>(() => _tokenCacheService.SetToken(email, ""));
        Assert.Contains("Token cannot be empty", exception.Message);
    }

    [Fact]
    public void GetToken_ExistingToken_ReturnsToken()
    {
        var email = "test@example.com";
        var token = "test-token-123";
        _tokenCacheService.SetToken(email, token);

        var result = _tokenCacheService.GetToken(email);

        Assert.Equal(token, result);
    }

    [Fact]
    public void GetToken_NonExistingToken_ReturnsNull()
    {
        var email = "nonexistent@example.com";

        var result = _tokenCacheService.GetToken(email);

        Assert.Null(result);
    }

    [Fact]
    public void GetToken_EmptyEmail_ReturnsNull()
    {
        var result = _tokenCacheService.GetToken("");

        Assert.Null(result);
    }

    [Fact]
    public void RemoveToken_ExistingToken_RemovesTokenFromCache()
    {
        var email = "test@example.com";
        var token = "test-token-123";
        _tokenCacheService.SetToken(email, token);

        _tokenCacheService.RemoveToken(email);

        var result = _tokenCacheService.GetToken(email);
        Assert.Null(result);
    }

    [Fact]
    public void RemoveToken_EmptyEmail_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => _tokenCacheService.RemoveToken(""));
        Assert.Contains("Email cannot be empty", exception.Message);
    }

    [Fact]
    public void SetToken_CaseInsensitiveEmail_StoresAndRetrievesCorrectly()
    {
        var email1 = "Test@Example.com";
        var email2 = "test@example.com";
        var token = "test-token-123";

        _tokenCacheService.SetToken(email1, token);
        var result = _tokenCacheService.GetToken(email2);

        Assert.Equal(token, result);
    }

    [Fact]
    public void MultipleUsers_IndependentTokens_StoresAndRetrievesCorrectly()
    {
        var email1 = "user1@example.com";
        var email2 = "user2@example.com";
        var token1 = "token-user1";
        var token2 = "token-user2";

        _tokenCacheService.SetToken(email1, token1);
        _tokenCacheService.SetToken(email2, token2);

        var result1 = _tokenCacheService.GetToken(email1);
        var result2 = _tokenCacheService.GetToken(email2);

        Assert.Equal(token1, result1);
        Assert.Equal(token2, result2);
    }

    [Fact]
    public void RemoveToken_OneUser_DoesNotAffectOtherUsers()
    {
        var email1 = "user1@example.com";
        var email2 = "user2@example.com";
        var token1 = "token-user1";
        var token2 = "token-user2";

        _tokenCacheService.SetToken(email1, token1);
        _tokenCacheService.SetToken(email2, token2);

        _tokenCacheService.RemoveToken(email1);

        var result1 = _tokenCacheService.GetToken(email1);
        var result2 = _tokenCacheService.GetToken(email2);

        Assert.Null(result1);
        Assert.Equal(token2, result2);
    }

    [Fact]
    public void GetEmailByToken_ExistingToken_ReturnsEmail()
    {
        var email = "test@example.com";
        var token = "test-token-123";

        _tokenCacheService.SetToken(email, token);

        var result = _tokenCacheService.GetEmailByToken(token);

        Assert.Equal(email.ToLowerInvariant(), result);
    }

    [Fact]
    public void GetEmailByToken_NonExistingToken_ReturnsNull()
    {
        var result = _tokenCacheService.GetEmailByToken("non-existing-token");

        Assert.Null(result);
    }

    [Fact]
    public void GetEmailByToken_EmptyToken_ReturnsNull()
    {
        var result = _tokenCacheService.GetEmailByToken("");

        Assert.Null(result);
    }

    [Fact]
    public void GetEmailByToken_MultipleTokens_ReturnsCorrectEmail()
    {
        var email1 = "user1@example.com";
        var email2 = "user2@example.com";
        var token1 = "token-user1";
        var token2 = "token-user2";

        _tokenCacheService.SetToken(email1, token1);
        _tokenCacheService.SetToken(email2, token2);

        var result1 = _tokenCacheService.GetEmailByToken(token1);
        var result2 = _tokenCacheService.GetEmailByToken(token2);

        Assert.Equal(email1.ToLowerInvariant(), result1);
        Assert.Equal(email2.ToLowerInvariant(), result2);
    }

    [Fact]
    public void RemoveToken_MultipleUsers_OnlyRemovesSpecifiedUserMappings()
    {
        var email1 = "user1@example.com";
        var email2 = "user2@example.com";
        var token1 = "token-user1";
        var token2 = "token-user2";

        _tokenCacheService.SetToken(email1, token1);
        _tokenCacheService.SetToken(email2, token2);

        _tokenCacheService.RemoveToken(email1);

        var result1Email = _tokenCacheService.GetEmailByToken(token1);
        var result1Token = _tokenCacheService.GetToken(email1);
        var result2Email = _tokenCacheService.GetEmailByToken(token2);
        var result2Token = _tokenCacheService.GetToken(email2);

        Assert.Null(result1Email);
        Assert.Null(result1Token);
        Assert.Equal(email2.ToLowerInvariant(), result2Email);
        Assert.Equal(token2, result2Token);
    }
   
}
