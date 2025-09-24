using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EventManagement.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Web.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly AuthenticationService _authService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(AuthenticationService authService, 
                         ITokenService tokenService,
                         ILogger<LoginModel> logger)
        {
            _authService = authService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [BindProperty]
        public LoginInput Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear existing tokens
            await _tokenService.ClearTokensAsync();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var loginRequest = new LoginRequest
                {
                    Email = Input.Email,
                    Password = Input.Password
                };

                var result = await _authService.LoginAsync(loginRequest);
                
                if (result.Success && !string.IsNullOrEmpty(result.AccessToken) && !string.IsNullOrEmpty(result.RefreshToken))
                {
                    await _tokenService.SetTokensAsync(result.AccessToken, result.RefreshToken);
                    _logger.LogInformation("User logged in successfully.");
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }

    public class LoginInput
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}