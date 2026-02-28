using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using OpenSenseMapAPI.Exceptions;
using OpenSenseMapAPI.Models;

namespace OpenSenseMapAPI.Services;

public class OpenSenseMapService : IOpenSenseMapService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenSenseMapService> _logger;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    public OpenSenseMapService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenSenseMapService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["OpenSenseMap:BaseUrl"] ?? "https://api.opensensemap.org";
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
    /// <summary>
    /// Registers a new user
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="OpenSenseMapException"></exception>
    public async Task<object> RegisterUserAsync(RegisterRequest request)
    {
        _logger.LogInformation("Attempting to register user with email: {Email}", request.Email);

        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/users/register", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Registration failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
                throw new OpenSenseMapException($"Registration failed: {errorContent}", (int)response.StatusCode);
            }

            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            var registerResponse = JsonSerializer.Deserialize<RegisterResponse>(result, _jsonOptions);
            return registerResponse ?? new RegisterResponse { Code = "created", Message = "User registered successfully" };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request error during registration");
            throw new OpenSenseMapException("Failed to connect to OpenSenseMap API", ex);
        }
    }

    /// <summary>
    /// Logs in a user
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="OpenSenseMapException"></exception>
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Attempting to login user with email: {Email}", request.Email);

        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/users/sign-in", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Login failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
                throw new OpenSenseMapException($"Login failed: {errorContent}", (int)response.StatusCode);
            }

            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("User logged in successfully: {Email}", request.Email);

            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(result, _jsonOptions);

            if (loginResponse == null)
            {
                throw new OpenSenseMapException("Invalid login response from server");
            }

            return loginResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request error during login");
            throw new OpenSenseMapException("Failed to connect to OpenSenseMap API", ex);
        }
    }

    /// <summary>
    /// Creates a new sense box
    /// </summary>
    /// <param name="request"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="OpenSenseMapException"></exception>
    public async Task<CreateBoxResponse> CreateNewSenseBoxAsync(NewSenseBoxRequest request, string token)
    {
        _logger.LogInformation("Attempting to create new sense box: {Name}", request.Name);

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException("Authentication token is required");
        }

        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/boxes")
            {
                Content = content
            };
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Create sense box failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
                throw new OpenSenseMapException($"Failed to create sense box: {errorContent}", (int)response.StatusCode);
            }

            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Sense box created successfully: {Name}", request.Name);
            _logger.LogDebug("CreateBox API Response: {Response}", result);

            var deserializeOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = null
            };

            var apiResponse = JsonSerializer.Deserialize<CreateBoxApiResponse>(result, deserializeOptions);

            if (apiResponse?.Data == null)
            {
                _logger.LogError("Failed to deserialize CreateBox response or data is null");
                return new CreateBoxResponse { Name = request.Name };
            }

            _logger.LogInformation("Deserialized box ID: {BoxId}, Name: {BoxName}", apiResponse.Data.Id, apiResponse.Data.Name);
            return apiResponse.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request error during sense box creation");
            throw new OpenSenseMapException("Failed to connect to OpenSenseMap API", ex);
        }
    }

    /// <summary>
    /// Retrieves a sense box by its ID
    /// </summary>
    /// <param name="senseBoxId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="OpenSenseMapException"></exception>
    public async Task<GetSenseBoxResponse> GetSenseBoxByIdAsync(string senseBoxId)
    {
        _logger.LogInformation("Attempting to retrieve sense box with ID: {SenseBoxId}", senseBoxId);

        if (string.IsNullOrWhiteSpace(senseBoxId))
        {
            throw new ArgumentException("Sense box ID cannot be empty", nameof(senseBoxId));
        }

        try
        {
            var response = await _httpClient.GetAsync($"/boxes/{senseBoxId}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Get sense box failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
                throw new OpenSenseMapException($"Failed to retrieve sense box: {errorContent}", (int)response.StatusCode);
            }

            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Sense box retrieved successfully: {SenseBoxId}", senseBoxId);

            var deserializeOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = null
            };

            var boxResponse = JsonSerializer.Deserialize<GetSenseBoxResponse>(result, deserializeOptions);
            return boxResponse ?? new GetSenseBoxResponse { Id = senseBoxId };
        
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request error during sense box retrieval");
            throw new OpenSenseMapException("Failed to connect to OpenSenseMap API", ex);
        }
    }

    /// <summary>
    /// Logs out a user
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="OpenSenseMapException"></exception>
    public async Task<object> LogoutAsync(string token)
    {
        _logger.LogInformation("Attempting to logout user");


        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException("Authentication token is required");
        }

        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/users/sign-out");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Logout failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
                throw new OpenSenseMapException($"Logout failed: {errorContent}", (int)response.StatusCode);
            }

            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("User logged out successfully");

            return JsonSerializer.Deserialize<object>(result) ?? new { message = "Logged out successfully" };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request error during logout");
            throw new OpenSenseMapException("Failed to connect to OpenSenseMap API", ex);
        }
    }
}
