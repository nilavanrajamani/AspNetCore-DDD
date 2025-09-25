using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EventManagement.Web.Services;
using System.Text.Json;

namespace EventManagement.Web.Pages.Admin
{
    public class DebugAuthModel : PageModel
    {
        private readonly ITokenService _tokenService;
        private readonly HttpClient _httpClient;
        private readonly ILogger<DebugAuthModel> _logger;

        public DebugAuthModel(ITokenService tokenService, IHttpClientFactory httpClientFactory, ILogger<DebugAuthModel> logger)
        {
            _tokenService = tokenService;
            _httpClient = httpClientFactory.CreateClient("EventAPI");
            _logger = logger;
        }

        public ApiDebugInfo? ApiDebugInfo { get; set; }
        public string? ApiError { get; set; }
        public bool HasToken { get; set; }
        public int TokenLength { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Get token info
                var token = await _tokenService.GetValidAccessTokenAsync();
                HasToken = !string.IsNullOrEmpty(token);
                TokenLength = token?.Length ?? 0;

                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    // Call API debug endpoint
                    var response = await _httpClient.GetAsync("api/v1/account/current-debug");
                    var content = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var apiResponse = JsonSerializer.Deserialize<DddApiResponse<ApiDebugInfo>>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (apiResponse?.Success == true)
                        {
                            ApiDebugInfo = apiResponse.Data;
                        }
                        else
                        {
                            ApiError = $"API returned unsuccessful response: {apiResponse?.Errors?.FirstOrDefault()}";
                        }
                    }
                    else
                    {
                        ApiError = $"API call failed: {response.StatusCode} - {content}";
                    }
                }
                else
                {
                    ApiError = "No valid token available";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting debug info");
                ApiError = $"Exception: {ex.Message}";
            }
        }
    }

    public class ApiDebugInfo
    {
        public bool IsAuthenticated { get; set; }

        public string? Email { get; set; }

        public string? UserId { get; set; }

        public bool IsAdmin { get; set; }

        public List<string>? Roles { get; set; }

        public List<ClaimDebugInfo>? AllClaims { get; set; }

        public bool CanModifyEventsData { get; set; }

        public PermissionDetails? PermissionDetails { get; set; }
    }

    public class PermissionDetails
    {
        public bool HasAdminRole { get; set; }

        public bool HasEventsModifyClaim { get; set; }

        public string PolicyResult { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;
    }

    public class ClaimDebugInfo
    {
        public string Type { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }

    public class DddApiResponse<T>
    {
        public bool Success { get; set; }

        public T? Data { get; set; }

        public List<string>? Errors { get; set; }
    }
}
