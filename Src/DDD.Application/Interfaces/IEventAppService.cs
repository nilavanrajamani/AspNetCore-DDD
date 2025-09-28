using System;
using System.Collections.Generic;

using DDD.Application.ViewModels;

namespace DDD.Application.Interfaces;

public interface IEventAppService : IDisposable
{
    void Register(EventViewModel eventViewModel);

    IEnumerable<EventViewModel> GetAll();

    IEnumerable<EventViewModel> GetAll(int skip, int take);

    EventViewModel GetById(Guid id);

    void Update(EventViewModel eventViewModel);

    void UpdateEventDetails(UpdateEventViewModel updateEventViewModel);

    void Remove(Guid id);

    void SetCapacityAndPricing(SetEventCapacityViewModel capacityViewModel);

    SetEventCapacityViewModel GetCapacityAndPricing(Guid eventId);

    void PublishEvent(Guid eventId);

    void UnpublishEvent(Guid eventId, string reason);

    void CancelEvent(Guid eventId, string reason, bool initiateRefunds = true);

    void SetEventVisibility(Guid eventId, string visibility);

    void InviteUserToEvent(Guid eventId, Guid userId, string role = "Attendee");

    void RemoveUserInvitation(Guid eventId, Guid userId);

    bool CanUserAccessEvent(Guid eventId, Guid userId);
    
    /// <summary>
    /// Gets "my events only" filtered by organizer ID using specification pattern.
    /// </summary>
    /// <param name="organizerId">The ID of the organizer whose events to retrieve.</param>
    /// <returns>A collection of events organized by the specified user.</returns>
    IEnumerable<EventViewModel> GetMyEventsOnly(Guid organizerId);
    
    /// <summary>
    /// Gets "my events only" with status filtering using specification pattern.
    /// </summary>
    /// <param name="organizerId">The ID of the organizer whose events to retrieve.</param>
    /// <param name="status">The event status to filter by.</param>
    /// <returns>A collection of events organized by the specified user with the given status.</returns>
    IEnumerable<EventViewModel> GetMyEventsOnly(Guid organizerId, string status);
    
    /// <summary>
    /// Gets "my events only" with pagination using specification pattern.
    /// </summary>
    /// <param name="organizerId">The ID of the organizer whose events to retrieve.</param>
    /// <param name="skip">The number of events to skip for pagination.</param>
    /// <param name="take">The number of events to take for pagination.</param>
    /// <returns>A collection of events organized by the specified user with pagination applied.</returns>
    IEnumerable<EventViewModel> GetMyEventsOnly(Guid organizerId, int skip, int take);
}