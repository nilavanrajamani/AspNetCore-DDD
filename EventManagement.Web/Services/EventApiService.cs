using EventManagement.Web.Models;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;

namespace EventManagement.Web.Services;

public interface IEventApiService
{
    Task<ApiResponse<List<EventViewModel>>> GetEventsAsync();
    Task<ApiResponse<EventViewModel>> GetEventByIdAsync(Guid id);
    Task<ApiResponse<EventViewModel>> CreateEventAsync(CreateEventViewModel model);
    Task<ApiResponse<bool>> DeleteEventAsync(Guid id);
    Task<ApiResponse<List<VenueViewModel>>> GetVenuesAsync();
}

public class EventApiService : IEventApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EventApiService> _logger;
    private readonly ICurrentUserService _currentUserService;

    public EventApiService(IHttpClientFactory httpClientFactory, 
                          ILogger<EventApiService> logger,
                          ICurrentUserService currentUserService)
    {
        _httpClient = httpClientFactory.CreateClient("EventAPI");
        _logger = logger;
        _currentUserService = currentUserService;
    }

    private void SetAuthenticationHeaders()
    {
        if (_currentUserService.IsAuthenticated)
        {
            // Add user context headers for the backend API
            _httpClient.DefaultRequestHeaders.Remove("X-User-Id");
            _httpClient.DefaultRequestHeaders.Remove("X-User-Email");
            
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
            var response = await _httpClient.GetAsync("api/v1/events");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var events = JsonConvert.DeserializeObject<List<EventViewModel>>(content) ?? new List<EventViewModel>();
                return new ApiResponse<List<EventViewModel>>
                {
                    Success = true,
                    Data = events
                };
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
            var response = await _httpClient.GetAsync($"api/v1/events/{id}");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var eventData = JsonConvert.DeserializeObject<EventViewModel>(content);
                return new ApiResponse<EventViewModel>
                {
                    Success = true,
                    Data = eventData
                };
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
            var json = JsonConvert.SerializeObject(new
            {
                title = model.Title,
                description = model.Description,
                date = model.Date,
                status = 0, // Draft
                visibility = (int)model.Visibility,
                organizerId = Guid.NewGuid(), // TODO: Get from authenticated user
                venueId = model.VenueId
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/v1/events", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var createdEvent = JsonConvert.DeserializeObject<EventViewModel>(responseContent);
                return new ApiResponse<EventViewModel>
                {
                    Success = true,
                    Data = createdEvent,
                    Message = "Event created successfully"
                };
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
            var response = await _httpClient.DeleteAsync($"api/v1/events/{id}");

            return new ApiResponse<bool>
            {
                Success = response.IsSuccessStatusCode,
                Data = response.IsSuccessStatusCode,
                Message = response.IsSuccessStatusCode ? "Event deleted successfully" : $"Failed to delete event: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event {EventId}", id);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Error connecting to the API service"
            };
        }
    }

    public async Task<ApiResponse<List<VenueViewModel>>> GetVenuesAsync()
    {
        try
        {
            // Mock data for now - replace with actual API call when venues endpoint is available
            var venues = new List<VenueViewModel>
            {
                new() { Id = Guid.NewGuid(), Name = "Conference Center A", Address = "123 Main St", Capacity = 500 },
                new() { Id = Guid.NewGuid(), Name = "Grand Ballroom", Address = "456 Oak Ave", Capacity = 300 },
                new() { Id = Guid.NewGuid(), Name = "Tech Hub", Address = "789 Pine St", Capacity = 150 }
            };

            return new ApiResponse<List<VenueViewModel>>
            {
                Success = true,
                Data = venues
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching venues");
            return new ApiResponse<List<VenueViewModel>>
            {
                Success = false,
                Message = "Error fetching venues",
                Data = new List<VenueViewModel>()
            };
        }
    }
}