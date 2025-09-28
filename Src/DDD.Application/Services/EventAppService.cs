using System;
using System.Collections.Generic;
using System.Linq;

using AutoMapper;
using AutoMapper.QueryableExtensions;

using DDD.Application.Interfaces;
using DDD.Application.ViewModels;
using DDD.Domain.Commands;
using DDD.Domain.Core.Bus;
using DDD.Domain.Interfaces;

namespace DDD.Application.Services;

public class EventAppService : IEventAppService
{
    private readonly IMapper _mapper;
    private readonly IEventRepository _eventRepository;
    private readonly IMediatorHandler _bus;

    public EventAppService(
        IMapper mapper,
        IEventRepository eventRepository,
        IMediatorHandler bus)
    {
        _mapper = mapper;
        _eventRepository = eventRepository;
        _bus = bus;
    }

    public IEnumerable<EventViewModel> GetAll()
    {
        return _eventRepository.GetAll().ProjectTo<EventViewModel>(_mapper.ConfigurationProvider);
    }

    public IEnumerable<EventViewModel> GetAll(int skip, int take)
    {
        return _eventRepository.GetAll()
            .Skip(skip)
            .Take(take)
            .ProjectTo<EventViewModel>(_mapper.ConfigurationProvider);
    }

    public EventViewModel GetById(Guid id)
    {
        return _mapper.Map<EventViewModel>(_eventRepository.GetById(id));
    }

    public void Register(EventViewModel eventViewModel)
    {
        var registerCommand = _mapper.Map<CreateEventCommand>(eventViewModel);
        _bus.SendCommand(registerCommand);
    }

    public void Update(EventViewModel eventViewModel)
    {
        // TODO: Implement UpdateEventCommand when needed
        throw new NotImplementedException("Update event functionality not yet implemented");
    }

    public void UpdateEventDetails(UpdateEventViewModel updateEventViewModel)
    {
        var updateCommand = _mapper.Map<UpdateEventCommand>(updateEventViewModel);
        _bus.SendCommand(updateCommand);
    }

    public void Remove(Guid id)
    {
        // TODO: Implement RemoveEventCommand when needed
        throw new NotImplementedException("Remove event functionality not yet implemented");
    }

    public void SetCapacityAndPricing(SetEventCapacityViewModel capacityViewModel)
    {
        var command = _mapper.Map<SetEventCapacityCommand>(capacityViewModel);
        _bus.SendCommand(command);
    }

    public SetEventCapacityViewModel GetCapacityAndPricing(Guid eventId)
    {
        var eventEntity = _eventRepository.GetById(eventId);
        if (eventEntity == null)
        {
            return null;
        }

        return _mapper.Map<SetEventCapacityViewModel>(eventEntity);
    }

    public void PublishEvent(Guid eventId)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("Event ID cannot be empty", nameof(eventId));
        }

        var publishCommand = new PublishEventCommand(eventId);
        _bus.SendCommand(publishCommand);
    }

    public void UnpublishEvent(Guid eventId, string reason)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("Event ID cannot be empty", nameof(eventId));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reason cannot be null, empty, or whitespace", nameof(reason));
        }

        var unpublishCommand = new UnpublishEventCommand(eventId, reason);
        _bus.SendCommand(unpublishCommand);
    }

    public void CancelEvent(Guid eventId, string reason, bool initiateRefunds = true)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("Event ID cannot be empty", nameof(eventId));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reason cannot be null, empty, or whitespace", nameof(reason));
        }

        var cancelCommand = new CancelEventCommand(eventId, reason, initiateRefunds);
        _bus.SendCommand(cancelCommand);
    }

    public void SetEventVisibility(Guid eventId, string visibility)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("Event ID cannot be empty", nameof(eventId));
        }

        if (string.IsNullOrWhiteSpace(visibility))
        {
            throw new ArgumentException("Visibility cannot be null, empty, or whitespace", nameof(visibility));
        }

        if (!Enum.TryParse<Domain.Models.EventVisibility>(visibility, true, out var parsedVisibility))
        {
            throw new ArgumentException("Invalid visibility value. Must be Private, Public, or InviteOnly", nameof(visibility));
        }

        var command = new SetEventVisibilityCommand(eventId, parsedVisibility);
        _bus.SendCommand(command);
    }

    public void InviteUserToEvent(Guid eventId, Guid userId, string role = "Attendee")
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("Event ID cannot be empty", nameof(eventId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role cannot be null, empty, or whitespace", nameof(role));
        }

        if (!Enum.TryParse<Domain.Models.InvitationRole>(role, true, out var parsedRole))
        {
            throw new ArgumentException("Invalid role value. Must be Attendee, Speaker, Sponsor, or VIP", nameof(role));
        }

        var command = new InviteUserToEventCommand(eventId, userId, parsedRole);
        _bus.SendCommand(command);
    }

    public void RemoveUserInvitation(Guid eventId, Guid userId)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("Event ID cannot be empty", nameof(eventId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        }

        var command = new RemoveUserInvitationCommand(eventId, userId);
        _bus.SendCommand(command);
    }

    public bool CanUserAccessEvent(Guid eventId, Guid userId)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("Event ID cannot be empty", nameof(eventId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        }

        var eventEntity = _eventRepository.GetById(eventId);
        if (eventEntity == null)
        {
            return false;
        }

        return eventEntity.CanUserAccess(userId);
    }
    
    /// <summary>
    /// Gets "my events only" filtered by organizer ID using specification pattern.
    /// </summary>
    /// <param name="organizerId">The ID of the organizer whose events to retrieve.</param>
    /// <returns>A collection of events organized by the specified user.</returns>
    public IEnumerable<EventViewModel> GetMyEventsOnly(Guid organizerId)
    {
        if (organizerId == Guid.Empty)
        {
            throw new ArgumentException("Organizer ID cannot be empty", nameof(organizerId));
        }

        return _eventRepository.GetMyEventsOnly(organizerId)
            .ProjectTo<EventViewModel>(_mapper.ConfigurationProvider);
    }
    
    /// <summary>
    /// Gets "my events only" with status filtering using specification pattern.
    /// </summary>
    /// <param name="organizerId">The ID of the organizer whose events to retrieve.</param>
    /// <param name="status">The event status to filter by.</param>
    /// <returns>A collection of events organized by the specified user with the given status.</returns>
    public IEnumerable<EventViewModel> GetMyEventsOnly(Guid organizerId, string status)
    {
        if (organizerId == Guid.Empty)
        {
            throw new ArgumentException("Organizer ID cannot be empty", nameof(organizerId));
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ArgumentException("Status cannot be null or empty", nameof(status));
        }

        if (!Enum.TryParse<Domain.Models.EventStatus>(status, true, out var eventStatus))
        {
            throw new ArgumentException($"Invalid event status: {status}", nameof(status));
        }

        return _eventRepository.GetMyEventsOnly(organizerId, eventStatus)
            .ProjectTo<EventViewModel>(_mapper.ConfigurationProvider);
    }
    
    /// <summary>
    /// Gets "my events only" with pagination using specification pattern.
    /// </summary>
    /// <param name="organizerId">The ID of the organizer whose events to retrieve.</param>
    /// <param name="skip">The number of events to skip for pagination.</param>
    /// <param name="take">The number of events to take for pagination.</param>
    /// <returns>A collection of events organized by the specified user with pagination applied.</returns>
    public IEnumerable<EventViewModel> GetMyEventsOnly(Guid organizerId, int skip, int take)
    {
        if (organizerId == Guid.Empty)
        {
            throw new ArgumentException("Organizer ID cannot be empty", nameof(organizerId));
        }

        if (skip < 0)
        {
            throw new ArgumentException("Skip value cannot be negative", nameof(skip));
        }

        if (take <= 0)
        {
            throw new ArgumentException("Take value must be positive", nameof(take));
        }

        return _eventRepository.GetMyEventsOnly(organizerId, skip, take)
            .ProjectTo<EventViewModel>(_mapper.ConfigurationProvider);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}