using EventManagement.Web.Services;
using System.Security.Claims;

namespace EventManagement.Web.Middleware;

public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtAuthenticationMiddleware> _logger;

    public JwtAuthenticationMiddleware(RequestDelegate next, ILogger<JwtAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
    {
        try
        {
            var accessToken = await tokenService.GetAccessTokenAsync();
            
            if (!string.IsNullOrEmpty(accessToken) && await tokenService.IsTokenValidAsync(accessToken))
            {
                var principal = tokenService.GetClaimsFromToken(accessToken);
                if (principal != null)
                {
                    context.User = principal;
                }
            }
            else if (!string.IsNullOrEmpty(accessToken))
            {
                // Token exists but is invalid/expired, try to refresh
                var validToken = await ((TokenService)tokenService).GetValidAccessTokenAsync();
                if (!string.IsNullOrEmpty(validToken))
                {
                    var principal = tokenService.GetClaimsFromToken(validToken);
                    if (principal != null)
                    {
                        context.User = principal;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in JWT authentication middleware");
        }

        await _next(context);
    }
}