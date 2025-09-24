using System.Security.Claims;
using System.Text.Json;

namespace EventManagement.Web.Services;

public interface ITokenService
{
    Task<string?> GetAccessTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    Task SetTokensAsync(string accessToken, string refreshToken);
    Task ClearTokensAsync();
    Task<bool> IsTokenValidAsync(string token);
    ClaimsPrincipal? GetClaimsFromToken(string token);
    Task<string?> GetValidAccessTokenAsync();
}

public class TokenService : ITokenService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthenticationService _authService;
    private readonly ILogger<TokenService> _logger;
    
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";

    public TokenService(
        IHttpContextAccessor httpContextAccessor, 
        AuthenticationService authService,
        ILogger<TokenService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _authService = authService;
        _logger = logger;
    }

    public Task<string?> GetAccessTokenAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.Session != null)
        {
            return Task.FromResult(context.Session.GetString(AccessTokenKey));
        }
        return Task.FromResult<string?>(null);
    }

    public Task<string?> GetRefreshTokenAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.Session != null)
        {
            return Task.FromResult(context.Session.GetString(RefreshTokenKey));
        }
        return Task.FromResult<string?>(null);
    }

    public Task SetTokensAsync(string accessToken, string refreshToken)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.Session != null)
        {
            context.Session.SetString(AccessTokenKey, accessToken);
            context.Session.SetString(RefreshTokenKey, refreshToken);
        }
        return Task.CompletedTask;
    }

    public Task ClearTokensAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.Session != null)
        {
            context.Session.Remove(AccessTokenKey);
            context.Session.Remove(RefreshTokenKey);
        }
        return Task.CompletedTask;
    }

    public Task<bool> IsTokenValidAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
            return Task.FromResult(false);

        try
        {
            var claims = GetClaimsFromToken(token);
            if (claims == null)
                return Task.FromResult(false);

            // Check if token is expired
            var expClaim = claims.FindFirst("exp");
            if (expClaim != null && long.TryParse(expClaim.Value, out var exp))
            {
                var expDateTime = DateTimeOffset.FromUnixTimeSeconds(exp);
                return Task.FromResult(expDateTime > DateTimeOffset.UtcNow);
            }
            
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return Task.FromResult(false);
        }
    }

    public ClaimsPrincipal? GetClaimsFromToken(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
                return null;

            // Simple JWT parsing - in production, use proper JWT validation
            var parts = token.Split('.');
            if (parts.Length != 3)
                return null;

            var payload = parts[1];
            // Add padding if needed
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var jsonBytes = Convert.FromBase64String(payload);
            var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
            var claims = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (claims == null)
                return null;

            var claimsList = new List<Claim>();
            foreach (var claim in claims)
            {
                var value = claim.Value?.ToString() ?? string.Empty;
                claimsList.Add(new Claim(claim.Key, value));
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claimsList, "jwt"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing JWT token");
            return null;
        }
    }

    public async Task<string?> GetValidAccessTokenAsync()
    {
        var accessToken = await GetAccessTokenAsync();
        
        if (!string.IsNullOrEmpty(accessToken) && await IsTokenValidAsync(accessToken))
        {
            return accessToken;
        }

        // Try to refresh the token
        var refreshToken = await GetRefreshTokenAsync();
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var refreshResult = await _authService.RefreshTokenAsync(refreshToken);
            if (refreshResult.Success && !string.IsNullOrEmpty(refreshResult.AccessToken) && !string.IsNullOrEmpty(refreshResult.RefreshToken))
            {
                await SetTokensAsync(refreshResult.AccessToken, refreshResult.RefreshToken);
                return refreshResult.AccessToken;
            }
        }

        // Clear invalid tokens
        await ClearTokensAsync();
        return null;
    }
}