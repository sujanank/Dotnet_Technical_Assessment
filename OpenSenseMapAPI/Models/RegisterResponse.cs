namespace OpenSenseMapAPI.Models
{

    public class RegisterResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}
