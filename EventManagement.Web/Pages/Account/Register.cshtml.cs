using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EventManagement.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Web.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly AuthenticationService _authService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            AuthenticationService authService,
            ITokenService tokenService,
            ILogger<RegisterModel> logger)
        {
            _authService = authService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [BindProperty]
        public RegisterInput Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            
            if (ModelState.IsValid)
            {
                var registerRequest = new RegisterRequest
                {
                    Email = Input.Email,
                    Password = Input.Password,
                    ConfirmPassword = Input.ConfirmPassword
                };

                var result = await _authService.RegisterAsync(registerRequest);
                
                if (result.Success && !string.IsNullOrEmpty(result.AccessToken) && !string.IsNullOrEmpty(result.RefreshToken))
                {
                    await _tokenService.SetTokensAsync(result.AccessToken, result.RefreshToken);
                    _logger.LogInformation("User created a new account and logged in successfully.");
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Registration failed. Please try again.");
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }

    public class RegisterInput
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}