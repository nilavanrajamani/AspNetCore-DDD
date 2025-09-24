using System.Text;
using System.Text.Json;
using EventManagement.Web.Models;

namespace EventManagement.Web.Services;

public class AuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthenticationService> _logger;
    
    public AuthenticationService(HttpClient httpClient, ILogger<AuthenticationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AuthenticationResult> LoginAsync(LoginRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/v1/account/login", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var dddApiResponse = JsonSerializer.Deserialize<DddApiResponse<TokenResponse>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (dddApiResponse?.Success == true && dddApiResponse.Data != null)
                {
                    return new AuthenticationResult 
                    { 
                        Success = true, 
                        AccessToken = dddApiResponse.Data.AccessToken,
                        RefreshToken = dddApiResponse.Data.RefreshToken
                    };
                }
                else
                {
                    var errorMessage = dddApiResponse?.Errors?.Any() == true
                        ? string.Join(", ", dddApiResponse.Errors)
                        : "Unknown error from authentication API";
                    
                    return new AuthenticationResult 
                    { 
                        Success = false, 
                        ErrorMessage = errorMessage
                    };
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Login failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
                
                return new AuthenticationResult 
                { 
                    Success = false, 
                    ErrorMessage = "Invalid login credentials" 
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return new AuthenticationResult 
            { 
                Success = false, 
                ErrorMessage = "An error occurred during login" 
            };
        }
    }

    public async Task<AuthenticationResult> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/v1/account/register", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var dddApiResponse = JsonSerializer.Deserialize<DddApiResponse<TokenResponse>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (dddApiResponse?.Success == true && dddApiResponse.Data != null)
                {
                    return new AuthenticationResult 
                    { 
                        Success = true, 
                        AccessToken = dddApiResponse.Data.AccessToken,
                        RefreshToken = dddApiResponse.Data.RefreshToken
                    };
                }
                else
                {
                    var errorMessage = dddApiResponse?.Errors?.Any() == true
                        ? string.Join(", ", dddApiResponse.Errors)
                        : "Registration failed. Please try again.";
                    
                    return new AuthenticationResult 
                    { 
                        Success = false, 
                        ErrorMessage = errorMessage
                    };
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Registration failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
                
                return new AuthenticationResult 
                { 
                    Success = false, 
                    ErrorMessage = "Registration failed. Please try again." 
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return new AuthenticationResult 
            { 
                Success = false, 
                ErrorMessage = "An error occurred during registration" 
            };
        }
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var request = new { RefreshToken = refreshToken };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/v1/account/refresh", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var dddApiResponse = JsonSerializer.Deserialize<DddApiResponse<TokenResponse>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (dddApiResponse?.Success == true && dddApiResponse.Data != null)
                {
                    return new AuthenticationResult 
                    { 
                        Success = true, 
                        AccessToken = dddApiResponse.Data.AccessToken,
                        RefreshToken = dddApiResponse.Data.RefreshToken
                    };
                }
                else
                {
                    var errorMessage = dddApiResponse?.Errors?.Any() == true
                        ? string.Join(", ", dddApiResponse.Errors)
                        : "Session expired. Please login again.";
                    
                    return new AuthenticationResult 
                    { 
                        Success = false, 
                        ErrorMessage = errorMessage
                    };
                }
            }
            else
            {
                _logger.LogWarning("Token refresh failed with status {StatusCode}", response.StatusCode);
                return new AuthenticationResult 
                { 
                    Success = false, 
                    ErrorMessage = "Session expired. Please login again." 
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return new AuthenticationResult 
            { 
                Success = false, 
                ErrorMessage = "An error occurred refreshing session" 
            };
        }
    }

    public async Task<CurrentUserResult> GetCurrentUserAsync(string accessToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = await _httpClient.GetAsync("/api/v1/account/current");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var dddApiResponse = JsonSerializer.Deserialize<DddApiResponse<UserInfo>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (dddApiResponse?.Success == true && dddApiResponse.Data != null)
                {
                    return new CurrentUserResult 
                    { 
                        Success = true, 
                        User = dddApiResponse.Data
                    };
                }
                else
                {
                    var errorMessage = dddApiResponse?.Errors?.Any() == true
                        ? string.Join(", ", dddApiResponse.Errors)
                        : "Failed to get user information";
                    
                    return new CurrentUserResult 
                    { 
                        Success = false, 
                        ErrorMessage = errorMessage
                    };
                }
            }
            else
            {
                _logger.LogWarning("Get current user failed with status {StatusCode}", response.StatusCode);
                return new CurrentUserResult 
                { 
                    Success = false, 
                    ErrorMessage = "Failed to get user information" 
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return new CurrentUserResult 
            { 
                Success = false, 
                ErrorMessage = "An error occurred getting user information" 
            };
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}

// Request/Response models
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class AuthenticationResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? ErrorMessage { get; set; }
}

public class CurrentUserResult
{
    public bool Success { get; set; }
    public UserInfo? User { get; set; }
    public string? ErrorMessage { get; set; }
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    // Add other user properties as needed
}