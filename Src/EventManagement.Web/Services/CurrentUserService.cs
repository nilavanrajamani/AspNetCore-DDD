using System.Security.Claims;

namespace EventManagement.Web.Services
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserEmail { get; }
        string? UserName { get; }
        bool IsAuthenticated { get; }
        IEnumerable<string> Roles { get; }
        IEnumerable<string> Permissions { get; }
        string? Issuer { get; }
        string? Audience { get; }
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Based on the claims screenshot, the user ID is in the 'nameid' claim
        public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue("nameid") 
                               ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        // Email might be in 'email' claim or standard ClaimTypes.Email
        public string? UserEmail => _httpContextAccessor.HttpContext?.User?.FindFirstValue("email")
                                  ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

        // Username might be in 'name' claim or standard ClaimTypes.Name
        public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirstValue("name")
                                 ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        // Roles are in 'role' claims
        public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User?.FindAll("role")?.Select(c => c.Value)
                                          ?? _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value)
                                          ?? Enumerable.Empty<string>();

        // Permissions are claims that don't match standard JWT claims (custom permission claims)
        public IEnumerable<string> Permissions 
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null) return Enumerable.Empty<string>();

                // Standard JWT claims to exclude from permissions
                var standardClaims = new HashSet<string>
                {
                    "nameid", "email", "name", "role", "jti", "iat", "nbf", "exp", "iss", "aud",
                    ClaimTypes.NameIdentifier, ClaimTypes.Email, ClaimTypes.Name, ClaimTypes.Role
                };

                return user.Claims
                    .Where(c => !standardClaims.Contains(c.Type))
                    .Select(c => c.Type)
                    .Distinct()
                    .ToList();
            }
        }

        // JWT issuer claim
        public string? Issuer => _httpContextAccessor.HttpContext?.User?.FindFirstValue("iss");

        // JWT audience claim  
        public string? Audience => _httpContextAccessor.HttpContext?.User?.FindFirstValue("aud");
    }
}