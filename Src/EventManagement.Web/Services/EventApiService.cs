using EventManagement.Web.Models;
using Newtonsoft.Json;
using System.Text;

namespace EventManagement.Web.Services;

public interface IEventApiService
{
    Task<ApiResponse<List<EventViewModel>>> GetEventsAsync();
    Task<ApiResponse<EventViewModel>> GetEventByIdAsync(Guid id);
    Task<ApiResponse<EventViewModel>> CreateEventAsync(CreateEventViewModel model);
    Task<ApiResponse<bool>> DeleteEventAsync(Guid id);
    Task<ApiResponse<List<VenueViewModel>>> GetVenuesAsync();
    
    // Capacity Management methods for US002
    Task<ApiResponse<bool>> SetEventCapacityAndPricingAsync(SetEventCapacityViewModel model);
    Task<ApiResponse<SetEventCapacityViewModel>> GetEventCapacityAndPricingAsync(Guid eventId);
    
    // Publish/Unpublish methods for US003
    Task<ApiResponse<bool>> PublishEventAsync(Guid eventId);
    Task<ApiResponse<bool>> UnpublishEventAsync(Guid eventId, string reason);
    
    // Update Event Details methods for US004
    Task<ApiResponse<bool>> UpdateEventDetailsAsync(UpdateEventViewModel model);
    
    // Cancel Event method for US005
    Task<ApiResponse<bool>> CancelEventAsync(Guid eventId, string reason, bool initiateRefunds = true);
}

public class EventApiService : IEventApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EventApiService> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITokenService _tokenService;

    public EventApiService(IHttpClientFactory httpClientFactory, 
                          ILogger<EventApiService> logger,
                          ICurrentUserService currentUserService,
                          ITokenService tokenService)
    {
        _httpClient = httpClientFactory.CreateClient("EventAPI");
        _logger = logger;
        _currentUserService = currentUserService;
        _tokenService = tokenService;
    }

    private async Task SetAuthenticationHeadersAsync()
    {
        // Clear any existing authorization headers
        _httpClient.DefaultRequestHeaders.Authorization = null;
        _httpClient.DefaultRequestHeaders.Remove("X-User-Id");
        _httpClient.DefaultRequestHeaders.Remove("X-User-Email");

        if (_currentUserService.IsAuthenticated)
        {
            // Set JWT Bearer token
            var token = await _tokenService.GetValidAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Add user context headers for the backend API
            if (!string.IsNullOrEmpty(_currentUserService.UserId))
                _httpClient.DefaultRequestHeaders.Add("X-User-Id", _currentUserService.UserId);
            
            if (!string.IsNullOrEmpty(_currentUserService.UserEmail))
                _httpClient.DefaultRequestHeaders.Add("X-User-Email", _currentUserService.UserEmail);
        }
    }

    public async Task<ApiResponse<List<EventViewModel>>> GetEventsAsync()
    {
        try
        {
            await SetAuthenticationHeadersAsync();
            var response = await _httpClient.GetAsync("api/v1/events/event-management");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var dddApiResponse = JsonConvert.DeserializeObject<DddApiResponse<List<EventViewModel>>>(content);
                
                if (dddApiResponse?.Success == true && dddApiResponse.Data != null)
                {
                    return new ApiResponse<List<EventViewModel>>
                    {
                        Success = true,
                        Data = dddApiResponse.Data
                    };
                }
                else
                {
                    var errorMessage = dddApiResponse?.Errors?.Any() == true 
                        ? string.Join(", ", dddApiResponse.Errors)
                        : "Unknown error from API";
                    
                    return new ApiResponse<List<EventViewModel>>
                    {
                        Success = false,
                        Message = errorMessage,
                        Data = new List<EventViewModel>()
                    };
                }
            }

            return new ApiResponse<List<EventViewModel>>
            {
                Success = false,
                Message = $"Failed to fetch events: {response.StatusCode}",
                Data = new List<EventViewModel>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching events from API");
            return new ApiResponse<List<EventViewModel>>
            {
                Success = false,
                Message = "Error connecting to the API service",
                Data = new List<EventViewModel>()
            };
        }
    }

    public async Task<ApiResponse<EventViewModel>> GetEventByIdAsync(Guid id)
    {
        try
        {
            await SetAuthenticationHeadersAsync();
            var response = await _httpClient.GetAsync($"api/v1/events/event-management/{id}");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var dddApiResponse = JsonConvert.DeserializeObject<DddApiResponse<EventViewModel>>(content);
                
                if (dddApiResponse?.Success == true && dddApiResponse.Data != null)
                {
                    return new ApiResponse<EventViewModel>
                    {
                        Success = true,
                        Data = dddApiResponse.Data
                    };
                }
                else
                {
                    var errorMessage = dddApiResponse?.Errors?.Any() == true
                        ? string.Join(", ", dddApiResponse.Errors)
                        : "Unknown error from API";
                    
                    return new ApiResponse<EventViewModel>
                    {
                        Success = false,
                        Message = errorMessage
                    };
                }
            }

            return new ApiResponse<EventViewModel>
            {
                Success = false,
                Message = $"Event not found: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching event {EventId} from API", id);
            return new ApiResponse<EventViewModel>
            {
                Success = false,
                Message = "Error connecting to the API service"
            };
        }
    }

    public async Task<ApiResponse<EventViewModel>> CreateEventAsync(CreateEventViewModel model)
    {
        try
        {
            await SetAuthenticationHeadersAsync();
            var json = JsonConvert.SerializeObject(new
            {
                title = model.Title,
                description = model.Description,
                startDate = model.StartDate,
                endDate = model.EndDate.Year > 1900 ? model.EndDate : model.StartDate.AddHours(2), // Use EndDate if provided, otherwise default 2-hour duration
                status = "Draft", // The API expects string values
                visibility = model.Visibility.ToString(),
                organizerId = Guid.NewGuid(), // TODO: Get from authenticated user
                venueId = model.VenueId ?? Guid.NewGuid(),
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/v1/events/event-management", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var dddApiResponse = JsonConvert.DeserializeObject<DddApiResponse<EventViewModel>>(responseContent);
                
                if (dddApiResponse?.Success == true && dddApiResponse.Data != null)
                {
                    return new ApiResponse<EventViewModel>
                    {
                        Success = true,
                        Data = dddApiResponse.Data,
                        Message = "Event created successfully"
                    };
                }
                else
                {
                    var errorMessage = dddApiResponse?.Errors?.Any() == true
                        ? string.Join(", ", dddApiResponse.Errors)
                        : "Unknown error from API";
                    
                    return new ApiResponse<EventViewModel>
                    {
                        Success = false,
                        Message = errorMessage,
                        Errors = dddApiResponse?.Errors?.ToList() ?? new List<string>()
                    };
                }
            }

            return new ApiResponse<EventViewModel>
            {
                Success = false,
                Message = $"Failed to create event: {response.StatusCode}",
                Errors = new List<string> { responseContent }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            return new ApiResponse<EventViewModel>
            {
                Success = false,
                Message = "Error connecting to the API service"
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteEventAsync(Guid id)
    {
        try
        {
            await SetAuthenticationHeadersAsync();
            var response = await _httpClient.DeleteAsync($"api/v1/events/event-management/{id}");

            return new ApiResponse<bool>
            {
                Success = response.IsSuccessStatusCode,
                Data = response.IsSuccessStatusCode,
                Message = response.IsSuccessStatusCode ? "Event deleted successfully" : $"Failed to delete event: {response.StatusCode}",
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event {EventId}", id);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Error connecting to the API service",
                Data = false,
            };
        }
    }

    public async Task<ApiResponse<List<VenueViewModel>>> GetVenuesAsync()
    {
        try
        {
            await SetAuthenticationHeadersAsync();
            var response = await _httpClient.GetAsync("api/v1/Venues/venues");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var dddApiResponse = JsonConvert.DeserializeObject<DddApiResponse<List<VenueViewModel>>>(content);
                
                if (dddApiResponse?.Success == true && dddApiResponse.Data != null)
                {
                    return new ApiResponse<List<VenueViewModel>>
                    {
                        Success = true,
                        Data = dddApiResponse.Data
                    };
                }
                else
                {
                    var errorMessage = dddApiResponse?.Errors?.Any() == true 
                        ? string.Join(", ", dddApiResponse.Errors)
                        : "Unknown error from API";
                    
                    return new ApiResponse<List<VenueViewModel>>
                    {
                        Success = false,
                        Message = errorMessage,
                        Data = new List<VenueViewModel>()
                    };
                }
            }

            return new ApiResponse<List<VenueViewModel>>
            {
                Success = false,
                Message = $"Failed to fetch venues: {response.StatusCode}",
                Data = new List<VenueViewModel>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching venues from API");
            return new ApiResponse<List<VenueViewModel>>
            {
                Success = false,
                Message = "Error connecting to the API service",
                Data = new List<VenueViewModel>()
            };
        }
    }

    // Capacity Management implementation for US002
    public async Task<ApiResponse<bool>> SetEventCapacityAndPricingAsync(SetEventCapacityViewModel model)
    {
        try
        {
            await SetAuthenticationHeadersAsync();
            
            var payload = new
            {
                eventId = model.EventId,
                totalCapacity = model.TotalCapacity,
                pricingTiers = model.PricingTiers.Select(pt => new
                {
                    name = pt.Name,
                    price = pt.Price,
                    currency = pt.Currency,
                    capacity = pt.Capacity,
                    saleStartDate = pt.SaleStartDate,
                    saleEndDate = pt.SaleEndDate
                }).ToList()
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"api/v1/events/event-management/{model.EventId}/capacity", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Event capacity and pricing configured successfully"
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                Message = $"Failed to set event capacity: {response.StatusCode}",
                Errors = new List<string> { responseContent }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting event capacity for event {EventId}", model.EventId);
            return new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                Message = "Error connecting to the API service"
            };
        }
    }

    public async Task<ApiResponse<SetEventCapacityViewModel>> GetEventCapacityAndPricingAsync(Guid eventId)
    {
        try
        {
            await SetAuthenticationHeadersAsync();
            var response = await _httpClient.GetAsync($"api/v1/events/event-management/{eventId}/capacity");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var dddApiResponse = JsonConvert.DeserializeObject<DddApiResponse<SetEventCapacityViewModel>>(content);
                
                if (dddApiResponse?.Success == true && dddApiResponse.Data != null)
                {
                    return new ApiResponse<SetEventCapacityViewModel>
                    {
                        Success = true,
                        Data = dddApiResponse.Data
                    };
                }
                else
                {
                    var errorMessage = dddApiResponse?.Errors?.Any() == true
                        ? string.Join(", ", dddApiResponse.Errors)
                        : "Unknown error from API";
                    
                    return new ApiResponse<SetEventCapacityViewModel>
                    {
                        Success = false,
                        Message = errorMessage
                    };
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // No capacity configuration exists yet
                return new ApiResponse<SetEventCapacityViewModel>
                {
                    Success = true,
                    Data = new SetEventCapacityViewModel
                    {
                        EventId = eventId,
                        TotalCapacity = 0,
                        PricingTiers = new List<PricingTierViewModel>()
                    }
                };
            }

            return new ApiResponse<SetEventCapacityViewModel>
            {
                Success = false,
                Message = $"Failed to get event capacity: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching event capacity for event {EventId}", eventId);
            return new ApiResponse<SetEventCapacityViewModel>
            {
                Success = false,
                Message = "Error connecting to the API service"
            };
        }
    }

    // Publish/Unpublish implementation for US003
    public async Task<ApiResponse<bool>> PublishEventAsync(Guid eventId)
    {
        try
        {
            await SetAuthenticationHeadersAsync();
            var response = await _httpClient.PutAsync($"api/v1/events/event-management/{eventId}/publish", null);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Event published successfully",
                };
            }

            // Parse error response from API (domain notifications)
            try
            {
                var dddApiResponse = JsonConvert.DeserializeObject<DddApiResponse<bool>>(responseContent);
                if (dddApiResponse?.Errors?.Any() == true)
                {
                    var errorMessage = string.Join(", ", dddApiResponse.Errors);
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = errorMessage,
                        Errors = dddApiResponse.Errors.ToList(),
                    };
                }
            }
            catch (JsonException)
            {
                // If JSON parsing fails, use raw response content
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                Message = $"Failed to publish event: {response.StatusCode}",
                Errors = new List<string> { responseContent },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {EventId}", eventId);
            return new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                Message = "Error connecting to the API service",
            };
        }
    }

    public async Task<ApiResponse<bool>> UnpublishEventAsync(Guid eventId, string reason)
    {
        try
        {
            await SetAuthenticationHeadersAsync();
            
            var payload = new { reason = reason };
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"api/v1/events/event-management/{eventId}/unpublish", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Event unpublished successfully",
                };
            }

            // Parse error response from API (domain notifications)
            try
            {
                var dddApiResponse = JsonConvert.DeserializeObject<DddApiResponse<bool>>(responseContent);
                if (dddApiResponse?.Errors?.Any() == true)
                {
                    var errorMessage = string.Join(", ", dddApiResponse.Errors);
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = errorMessage,
                        Errors = dddApiResponse.Errors.ToList(),
                    };
                }
            }
            catch (JsonException)
            {
                // If JSON parsing fails, use raw response content
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                Message = $"Failed to unpublish event: {response.StatusCode}",
                Errors = new List<string> { responseContent },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpublishing event {EventId}", eventId);
            return new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                Message = "Error connecting to the API service",
            };
        }
    }

    // Update Event Details implementation for US004
    public async Task<ApiResponse<bool>> UpdateEventDetailsAsync(UpdateEventViewModel model)
    {
        try
        {
            await SetAuthenticationHeadersAsync();
            
            var payload = new
            {
                id = model.Id,
                title = model.Title,
                description = model.Description,
                venueId = model.VenueId,
                startDate = model.StartDate,
                endDate = model.EndDate,
                forceUpdate = model.ForceUpdate
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"api/v1/events/event-management/{model.Id}/details", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Event details updated successfully"
                };
            }

            // Parse error response from API (domain notifications)
            try
            {
                var dddApiResponse = JsonConvert.DeserializeObject<DddApiResponse<bool>>(responseContent);
                if (dddApiResponse?.Errors?.Any() == true)
                {
                    var errorMessage = string.Join(", ", dddApiResponse.Errors);
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = errorMessage,
                        Errors = dddApiResponse.Errors.ToList()
                    };
                }
            }
            catch (JsonException)
            {
                // If JSON parsing fails, use raw response content
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                Message = $"Failed to update event details: {response.StatusCode}",
                Errors = new List<string> { responseContent }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event details for event {EventId}", model.Id);
            return new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                Message = "Error connecting to the API service"
            };
        }
    }

    // Cancel Event implementation for US005
    public async Task<ApiResponse<bool>> CancelEventAsync(Guid eventId, string reason, bool initiateRefunds = true)
    {
        try
        {
            await SetAuthenticationHeadersAsync();
            
            var payload = new { reason = reason, initiateRefunds = initiateRefunds };
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"api/v1/events/event-management/{eventId}/cancel", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Event cancelled successfully",
                };
            }

            // Parse error response from API (domain notifications)
            try
            {
                var dddApiResponse = JsonConvert.DeserializeObject<DddApiResponse<bool>>(responseContent);
                if (dddApiResponse?.Errors?.Any() == true)
                {
                    var errorMessage = string.Join(", ", dddApiResponse.Errors);
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = errorMessage,
                        Errors = dddApiResponse.Errors.ToList(),
                    };
                }
            }
            catch (JsonException)
            {
                // If JSON parsing fails, use raw response content
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                Message = $"Failed to cancel event: {response.StatusCode}",
                Errors = new List<string> { responseContent },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling event {EventId}", eventId);
            return new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                Message = "Error connecting to the API service",
            };
        }
    }
}