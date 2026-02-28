using OpenSenseMapAPI.Models;

namespace OpenSenseMapAPI.Services;

public interface IOpenSenseMapService
{
    Task<object> RegisterUserAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<CreateBoxResponse> CreateNewSenseBoxAsync(NewSenseBoxRequest request, string token);
    Task<GetSenseBoxResponse> GetSenseBoxByIdAsync(string senseBoxId);
    Task<object> LogoutAsync(string token);
}
