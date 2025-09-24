using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EventManagement.Web.Services;

namespace EventManagement.Web.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(ITokenService tokenService, ILogger<LogoutModel> logger)
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<IActionResult> OnPost(string? returnUrl = null)
        {
            await _tokenService.ClearTokensAsync();
            _logger.LogInformation("User logged out.");
            
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return RedirectToPage("/Index");
            }
        }
    }
}