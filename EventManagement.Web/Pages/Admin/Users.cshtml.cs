using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EventManagement.Web.Services;
using System.Text.Json;
using System.Text;

namespace EventManagement.Web.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly AuthenticationService _authService;
        private readonly ILogger<UsersModel> _logger;
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;

        public UsersModel(AuthenticationService authService, ILogger<UsersModel> logger, IHttpClientFactory httpClientFactory, ITokenService tokenService)
        {
            _authService = authService;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("EventAPI");
            _tokenService = tokenService;
        }

        public List<UserInfo>? Users { get; set; }
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }

        public async Task OnGetAsync()
        {
            await LoadUsersAsync();
        }

        public async Task<IActionResult> OnPostAssignPermissionAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                Message = "Email is required";
                IsSuccess = false;
                await LoadUsersAsync();
                return Page();
            }

            try
            {
                // Get current user's token for authorization
                var token = await _tokenService.GetValidAccessTokenAsync();
                
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                
                _logger.LogInformation("Attempting to assign permission with token present: {HasToken}", !string.IsNullOrEmpty(token));

                var request = new { Email = email };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/v1/account/assign-events-permission", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Message = $"Events permission successfully assigned to {email}";
                    IsSuccess = true;
                }
                else
                {
                    Message = $"Failed to assign permission: {response.StatusCode} - {responseContent}";
                    IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning permission to user {Email}", email);
                Message = "Error connecting to the API service";
                IsSuccess = false;
            }

            await LoadUsersAsync();
            return Page();
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                // Get current user's token for authorization
                var token = await _tokenService.GetValidAccessTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync("api/v1/account/users");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<UserInfo>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Success == true)
                    {
                        Users = apiResponse.Data ?? new List<UserInfo>();
                    }
                    else
                    {
                        Users = new List<UserInfo>();
                        _logger.LogWarning("API returned unsuccessful response: {Message}", apiResponse?.Message);
                    }
                }
                else
                {
                    Users = new List<UserInfo>();
                    _logger.LogWarning("Failed to load users: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                Users = new List<UserInfo>();
            }
        }
    }

    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<ClaimInfo> Claims { get; set; } = new();
        public bool HasEventsPermission { get; set; }
    }

    public class ClaimInfo
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}