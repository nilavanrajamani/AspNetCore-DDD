using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using DDD.Domain.Core.Bus;
using DDD.Domain.Core.Notifications;
using DDD.Domain.Interfaces;
using DDD.Infra.CrossCutting.Identity.Data;
using DDD.Infra.CrossCutting.Identity.Models;
using DDD.Infra.CrossCutting.Identity.Models.AccountViewModels;
using DDD.Infra.CrossCutting.Identity.Services;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DDD.Services.Api.Controllers.V1;

[Authorize]
[ApiVersion("1.0")]
public class AccountController : ApiController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AuthDbContext _dbContext;
    private readonly IUser _user;
    private readonly IJwtFactory _jwtFactory;
    private readonly ILogger _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        AuthDbContext dbContext,
        IUser user,
        IJwtFactory jwtFactory,
        ILoggerFactory loggerFactory,
        INotificationHandler<DomainNotification> notifications,
        IMediatorHandler mediator)
        : base(notifications, mediator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        _user = user;
        _jwtFactory = jwtFactory;
        _logger = loggerFactory.CreateLogger<AccountController>();
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            NotifyModelStateErrors();
            return Response();
        }

        // Sign In
        var signInResult = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, true);
        if (!signInResult.Succeeded)
        {
            NotifyError(signInResult.ToString(), "Login failure");
            return Response();
        }

        // Get User
        var appUser = await _userManager.FindByEmailAsync(model.Email);
        if (appUser is null)
        {
            return Response();
        }

        // var appUser = _userManager.Users.SingleOrDefault(r => r.Email == model.Email);

        _logger.LogInformation(1, "User logged in.");
        return Response(await GenerateToken(appUser));
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("register")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            NotifyModelStateErrors();
            return Response();
        }

        // Add User
        var appUser = new ApplicationUser { UserName = model.Email, Email = model.Email };
        var identityResult = await _userManager.CreateAsync(appUser, model.Password);
        if (!identityResult.Succeeded)
        {
            AddIdentityErrors(identityResult);
            return Response();
        }

        // Add UserRoles
        identityResult = await _userManager.AddToRoleAsync(appUser, "Admin");
        if (!identityResult.Succeeded)
        {
            AddIdentityErrors(identityResult);
            return Response();
        }

        // Add UserClaims
        var userClaims = new List<Claim>
        {
            new Claim("Customers_Write", "Write"),
            new Claim("Customers_Remove", "Remove"),
            new Claim("Events_Modify", "Modify"),
        };
        await _userManager.AddClaimsAsync(appUser, userClaims);

        // SignIn
        // await _signInManager.SignInAsync(user, false);

        _logger.LogInformation(3, "User created a new account with password.");
        return Response();
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("refresh")]
    public async Task<IActionResult> Refresh(TokenViewModel model)
    {
        if (!ModelState.IsValid)
        {
            NotifyModelStateErrors();
            return Response();
        }

        // Get current RefreshToken
        var refreshTokenCurrent = _dbContext.RefreshTokens.SingleOrDefault(
            x => x.Token == model.RefreshToken && !x.Used && !x.Invalidated);
        if (refreshTokenCurrent is null)
        {
            NotifyError("RefreshToken", "Refresh token does not exist");
            return Response();
        }

        if (refreshTokenCurrent.ExpiryDate < DateTime.UtcNow)
        {
            // Update current RefreshToken
            refreshTokenCurrent.Invalidated = true;
            await _dbContext.SaveChangesAsync();
            NotifyError("RefreshToken", "Refresh token invalid");
            return Response();
        }

        // Get User
        var appUser = await _userManager.FindByIdAsync(refreshTokenCurrent.UserId);
        if (appUser is null)
        {
            NotifyError("User", "User does not exist");
            return Response();
        }

        // Remove current RefreshToken
        // _dbContext.Remove(refreshTokenCurrent);
        // await _dbContext.SaveChangesAsync();

        // Update current RefreshToken
        refreshTokenCurrent.Used = true;
        await _dbContext.SaveChangesAsync();

        return Response(await GenerateToken(appUser));
    }

    [HttpGet]
    [Route("current")]
    public IActionResult GetCurrent()
    {
        return Response(new
        {
            IsAuthenticated = _user.IsAuthenticated(),
            ClaimsIdentity = _user.GetClaimsIdentity().Select(x => new { x.Type, x.Value }),
        });
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("debug-current")]
    public async Task<IActionResult> GetCurrentDebug()
    {
        if (!_user.IsAuthenticated())
        {
            return Response(new { IsAuthenticated = false, Message = "User not authenticated" });
        }

        var claims = _user.GetClaimsIdentity().ToList();
        var roles = claims.Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" || c.Type == "role").Select(c => c.Value).ToList();
        var isAdmin = roles.Contains("Admin");
        var email = claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/soap/envelope/")?.Value ??
                   claims.FirstOrDefault(c => c.Type == "email")?.Value;

        // Enhanced permission checking
        bool hasEventsModifyClaim = false;
        bool canModifyEventsData = false;
        string permissionSummary = "Not checked";

        try
        {
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var userClaims = await _userManager.GetClaimsAsync(user);
                    hasEventsModifyClaim = userClaims.Any(c => c.Type == "Events_Modify" && c.Value == "Modify");
                    canModifyEventsData = isAdmin && hasEventsModifyClaim;

                    permissionSummary = canModifyEventsData
                        ? "✅ YES - Has CanModifyEventsData permission"
                        : $"❌ NO - Missing: {(isAdmin ? "" : "Admin role")}{(isAdmin && !hasEventsModifyClaim ? "" : isAdmin ? "" : ", ")}{(hasEventsModifyClaim ? "" : "Events_Modify claim")}";
                }
                else
                {
                    permissionSummary = "❌ User not found in database";
                }
            }
            else
            {
                permissionSummary = "❌ No email found in claims";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permissions for current user");
            permissionSummary = $"❌ Error checking permissions: {ex.Message}";
        }

        return Response(new
        {
            IsAuthenticated = true,
            Email = email,
            UserId = claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value,
            IsAdmin = isAdmin,
            Roles = roles,
            AllClaims = claims.Select(c => new { c.Type, c.Value }).ToList(),
            // Enhanced permission info
            HasEventsModifyClaim = hasEventsModifyClaim,
            CanModifyEventsData = canModifyEventsData,
            PermissionSummary = permissionSummary
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Route("assign-events-permission")]
    public async Task<IActionResult> AssignEventsPermission([FromBody] AssignPermissionRequest request)
    {
        if (!ModelState.IsValid)
        {
            NotifyModelStateErrors();
            return Response();
        }

        // Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            NotifyError("User", "User not found");
            return Response();
        }

        // Check if user already has the claim
        var existingClaims = await _userManager.GetClaimsAsync(user);
        var hasEventsClaim = existingClaims.Any(c => c.Type == "Events_Modify" && c.Value == "Modify");

        if (!hasEventsClaim)
        {
            // Add Events_Modify claim
            var eventsClaim = new Claim("Events_Modify", "Modify");
            var result = await _userManager.AddClaimAsync(user, eventsClaim);

            if (!result.Succeeded)
            {
                AddIdentityErrors(result);
                return Response();
            }
        }

        // Ensure user has Admin role
        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
            if (!roleResult.Succeeded)
            {
                AddIdentityErrors(roleResult);
                return Response();
            }
        }

        return Response(new { Message = "Events modification permission assigned successfully" });
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = _userManager.Users.ToList();
        var userList = new List<object>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            userList.Add(new
            {
                user.Id,
                user.Email,
                user.UserName,
                Roles = roles,
                Claims = claims.Select(c => new { c.Type, c.Value }),
                HasEventsPermission = claims.Any(c => c.Type == "Events_Modify" && c.Value == "Modify"),
            });
        }

        return Response(userList);
    }

    private async Task<TokenViewModel> GenerateToken(ApplicationUser appUser)
    {
        if (appUser is null || string.IsNullOrEmpty(appUser.Email))
        {
            return new TokenViewModel();
        }

        // Init ClaimsIdentity
        var claimsIdentity = new ClaimsIdentity();
        claimsIdentity.AddClaim(new Claim(JwtRegisteredClaimNames.Email, appUser.Email));
        claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, appUser.Id));

        // Get UserClaims
        var userClaims = await _userManager.GetClaimsAsync(appUser);
        claimsIdentity.AddClaims(userClaims);

        // Get UserRoles
        var userRoles = await _userManager.GetRolesAsync(appUser);
        claimsIdentity.AddClaims(userRoles.Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));

        // ClaimsIdentity.DefaultRoleClaimType & ClaimTypes.Role is the same

        // Get RoleClaims
        foreach (var userRole in userRoles)
        {
            var role = await _roleManager.FindByNameAsync(userRole);
            if (role is not null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                claimsIdentity.AddClaims(roleClaims);
            }
        }

        // Generate access token
        var jwtToken = await _jwtFactory.GenerateJwtToken(claimsIdentity);

        // Add refresh token
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = appUser.Id,
            CreationDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddMinutes(90),
            JwtId = jwtToken.JwtId,
        };
        await _dbContext.RefreshTokens.AddAsync(refreshToken);
        await _dbContext.SaveChangesAsync();

        return new TokenViewModel
        {
            AccessToken = jwtToken.AccessToken,
            RefreshToken = refreshToken.Token,
        };
    }
}
