using Microsoft.AspNetCore.SignalR;

namespace EventManagement.Web.Hubs;

public class EventNotificationHub : Hub
{
    public async Task JoinEventGroup(string eventId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Event_{eventId}");
    }

    public async Task LeaveEventGroup(string eventId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Event_{eventId}");
    }

    public async Task JoinOrganizerGroup(string organizerId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Organizer_{organizerId}");
    }

    public async Task LeaveOrganizerGroup(string organizerId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Organizer_{organizerId}");
    }

    public override async Task OnConnectedAsync()
    {
        // Optionally join user to their personal notification group
        if (Context.User?.Identity?.IsAuthenticated == true)
        {
            var userId = Context.User.FindFirst("sub")?.Value ?? Context.User.FindFirst("id")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}