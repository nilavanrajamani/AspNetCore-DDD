using System;
using System.Threading;
using System.Threading.Tasks;

using DDD.Domain.Commands;
using DDD.Domain.Core.Bus;
using DDD.Domain.Core.Notifications;
using DDD.Domain.Events;
using DDD.Domain.Interfaces;
using DDD.Domain.Models;

using MediatR;

namespace DDD.Domain.CommandHandlers;

public class EventCommandHandler : CommandHandler,
    IRequestHandler<CreateEventCommand, bool>,
    IRequestHandler<SetEventCapacityCommand, bool>,
    IRequestHandler<PublishEventCommand, bool>,
    IRequestHandler<UnpublishEventCommand, bool>,
    IRequestHandler<UpdateEventCommand, bool>,
    IRequestHandler<CancelEventCommand, bool>,
    IRequestHandler<SetEventVisibilityCommand, bool>,
    IRequestHandler<InviteUserToEventCommand, bool>,
    IRequestHandler<RemoveUserInvitationCommand, bool>
{
    private readonly IEventRepository _eventRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly IMediatorHandler _bus;

    public EventCommandHandler(
        IEventRepository eventRepository,
        IVenueRepository venueRepository,
        IUnitOfWork uow,
        IMediatorHandler bus,
        INotificationHandler<DomainNotification> notifications)
        : base(uow, bus, notifications)
    {
        _eventRepository = eventRepository;
        _venueRepository = venueRepository;
        _bus = bus;
    }

    public Task<bool> Handle(CreateEventCommand message, CancellationToken cancellationToken)
    {
        if (!message.IsValid())
        {
            NotifyValidationErrors(message);
            return Task.FromResult(false);
        }

        // Check if venue exists
        var venue = _venueRepository.GetById(message.VenueId);
        if (venue == null)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, "The venue does not exist."));
            return Task.FromResult(false);
        }

        // Check if organizer already has an event with the same title
        var existingEvent = _eventRepository.GetByOrganizerAndTitle(message.OrganizerId, message.Title);
        if (existingEvent != null)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, "The organizer already has an event with this title."));
            return Task.FromResult(false);
        }

        var eventEntity = new Event(
            Guid.NewGuid(),
            message.Title,
            message.Description,
            message.OrganizerId,
            message.VenueId,
            message.StartDate,
            message.EndDate);

        _eventRepository.Add(eventEntity);

        if (Commit())
        {
            _bus.RaiseEvent(new EventCreatedEvent(
                eventEntity.Id,
                eventEntity.Title,
                eventEntity.Description,
                eventEntity.OrganizerId,
                eventEntity.VenueId,
                eventEntity.StartDate,
                eventEntity.EndDate));
        }

        return Task.FromResult(true);
    }

    public Task<bool> Handle(SetEventCapacityCommand message, CancellationToken cancellationToken)
    {
        if (!message.IsValid())
        {
            NotifyValidationErrors(message);
            return Task.FromResult(false);
        }

        // Get the event to update
        var eventEntity = _eventRepository.GetById(message.Id);
        if (eventEntity == null)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, "The event does not exist."));
            return Task.FromResult(false);
        }

        // Check if event is in draft status
        if (eventEntity.Status != EventStatus.Draft)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, "Cannot modify capacity after event is published."));
            return Task.FromResult(false);
        }

        try
        {
            // Set capacity (scalar properties only)
            eventEntity.SetCapacityAndPricing(message.TotalCapacity, message.PricingTiers);
            
            // Handle pricing tiers collection replacement through repository
            _eventRepository.UpdateEventPricingTiers(eventEntity, message.PricingTiers);
            
            if (Commit())
            {
                _bus.RaiseEvent(new EventCapacitySetEvent(
                    eventEntity.Id,
                    message.TotalCapacity,
                    message.PricingTiers.Count));
            }

            return Task.FromResult(true);
        }
        catch (InvalidOperationException ex)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, ex.Message));
            return Task.FromResult(false);
        }
    }

    public Task<bool> Handle(PublishEventCommand message, CancellationToken cancellationToken)
    {
        if (!message.IsValid())
        {
            NotifyValidationErrors(message);
            return Task.FromResult(false);
        }

        var eventEntity = _eventRepository.GetById(message.Id);
        if (eventEntity == null)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, "The event does not exist."));
            return Task.FromResult(false);
        }

        try
        {
            eventEntity.Publish();
            _eventRepository.Update(eventEntity);

            if (Commit())
            {
                _bus.RaiseEvent(new EventPublishedEvent(
                    eventEntity.Id,
                    eventEntity.OrganizerId,
                    eventEntity.Title,
                    eventEntity.StartDate,
                    eventEntity.TotalCapacity ?? 0));
            }

            return Task.FromResult(true);
        }
        catch (InvalidOperationException ex)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, ex.Message));
            return Task.FromResult(false);
        }
    }

    public Task<bool> Handle(UnpublishEventCommand message, CancellationToken cancellationToken)
    {
        if (!message.IsValid())
        {
            NotifyValidationErrors(message);
            return Task.FromResult(false);
        }

        var eventEntity = _eventRepository.GetById(message.Id);
        if (eventEntity == null)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, "The event does not exist."));
            return Task.FromResult(false);
        }

        try
        {
            eventEntity.Unpublish(message.Reason);
            _eventRepository.Update(eventEntity);

            if (Commit())
            {
                _bus.RaiseEvent(new EventUnpublishedEvent(
                    eventEntity.Id,
                    eventEntity.OrganizerId,
                    message.Reason));
            }

            return Task.FromResult(true);
        }
        catch (InvalidOperationException ex)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, ex.Message));
            return Task.FromResult(false);
        }
    }

    public Task<bool> Handle(UpdateEventCommand message, CancellationToken cancellationToken)
    {
        if (!message.IsValid())
        {
            NotifyValidationErrors(message);
            return Task.FromResult(false);
        }

        var eventEntity = _eventRepository.GetById(message.Id);
        if (eventEntity == null)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, "The event does not exist."));
            return Task.FromResult(false);
        }

        // Check if venue exists if it's being changed
        if (eventEntity.VenueId != message.VenueId)
        {
            var venue = _venueRepository.GetById(message.VenueId);
            if (venue == null)
            {
                _bus.RaiseEvent(new DomainNotification(message.MessageType, "The venue does not exist."));
                return Task.FromResult(false);
            }
        }

        try
        {
            var oldTitle = eventEntity.Title;
            var oldDescription = eventEntity.Description;
            var oldVenueId = eventEntity.VenueId;
            var oldStartDate = eventEntity.StartDate;
            var oldEndDate = eventEntity.EndDate;

            eventEntity.UpdateDetails(
                message.Title,
                message.Description,
                message.VenueId,
                message.StartDate,
                message.EndDate,
                message.ForceUpdate);

            _eventRepository.Update(eventEntity);

            if (Commit())
            {
                // Create change list for the domain event
                var changes = new List<EventChange>();

                if (!string.Equals(oldTitle, message.Title, StringComparison.OrdinalIgnoreCase))
                    changes.Add(new EventChange("Title", oldTitle, message.Title));

                if (!string.Equals(oldDescription, message.Description, StringComparison.OrdinalIgnoreCase))
                    changes.Add(new EventChange("Description", oldDescription, message.Description));

                if (oldVenueId != message.VenueId)
                    changes.Add(new EventChange("VenueId", oldVenueId.ToString(), message.VenueId.ToString()));

                if (oldStartDate != message.StartDate)
                    changes.Add(new EventChange("StartDate", oldStartDate.ToString("yyyy-MM-dd HH:mm:ss"), message.StartDate.ToString("yyyy-MM-dd HH:mm:ss")));

                if (oldEndDate != message.EndDate)
                    changes.Add(new EventChange("EndDate", oldEndDate.ToString("yyyy-MM-dd HH:mm:ss"), message.EndDate.ToString("yyyy-MM-dd HH:mm:ss")));

                if (changes.Count > 0 && eventEntity.Status == EventStatus.Published)
                {
                    _bus.RaiseEvent(new EventUpdatedEvent(
                        eventEntity.Id,
                        eventEntity.OrganizerId,
                        changes));
                }
            }

            return Task.FromResult(true);
        }
        catch (InvalidOperationException ex)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, ex.Message));
            return Task.FromResult(false);
        }
    }

    public Task<bool> Handle(CancelEventCommand message, CancellationToken cancellationToken)
    {
        if (!message.IsValid())
        {
            NotifyValidationErrors(message);
            return Task.FromResult(false);
        }

        var eventEntity = _eventRepository.GetById(message.Id);
        if (eventEntity == null)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, "The event does not exist."));
            return Task.FromResult(false);
        }

        try
        {
            var previousStatus = eventEntity.Status;
            eventEntity.Cancel(message.Reason, message.InitiateRefunds);
            _eventRepository.Update(eventEntity);

            if (Commit())
            {
                _bus.RaiseEvent(new EventCancelledEvent(
                    eventEntity.Id,
                    eventEntity.OrganizerId,
                    message.Reason,
                    previousStatus,
                    message.InitiateRefunds));
            }

            return Task.FromResult(true);
        }
        catch (InvalidOperationException ex)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, ex.Message));
            return Task.FromResult(false);
        }
        catch (ArgumentException ex)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, ex.Message));
            return Task.FromResult(false);
        }
    }

    public Task<bool> Handle(SetEventVisibilityCommand message, CancellationToken cancellationToken)
    {
        if (!message.IsValid())
        {
            NotifyValidationErrors(message);
            return Task.FromResult(false);
        }

        var eventEntity = _eventRepository.GetById(message.EventId);
        if (eventEntity == null)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, "The event does not exist."));
            return Task.FromResult(false);
        }

        try
        {
            var previousVisibility = eventEntity.Visibility;
            eventEntity.SetVisibility(message.Visibility);
            _eventRepository.Update(eventEntity);

            if (Commit())
            {
                _bus.RaiseEvent(new EventVisibilityChangedEvent(
                    eventEntity.Id,
                    eventEntity.OrganizerId,
                    previousVisibility.ToString(),
                    message.Visibility.ToString()));
            }

            return Task.FromResult(true);
        }
        catch (InvalidOperationException ex)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, ex.Message));
            return Task.FromResult(false);
        }
    }

    public Task<bool> Handle(InviteUserToEventCommand message, CancellationToken cancellationToken)
    {
        if (!message.IsValid())
        {
            NotifyValidationErrors(message);
            return Task.FromResult(false);
        }

        var eventEntity = _eventRepository.GetById(message.EventId);
        if (eventEntity == null)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, "The event does not exist."));
            return Task.FromResult(false);
        }

        try
        {
            eventEntity.InviteUser(message.UserId, message.Role);
            _eventRepository.Update(eventEntity);

            if (Commit())
            {
                _bus.RaiseEvent(new UserInvitedToEventEvent(
                    eventEntity.Id,
                    eventEntity.OrganizerId,
                    message.UserId,
                    message.Role));
            }

            return Task.FromResult(true);
        }
        catch (InvalidOperationException ex)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, ex.Message));
            return Task.FromResult(false);
        }
    }

    public Task<bool> Handle(RemoveUserInvitationCommand message, CancellationToken cancellationToken)
    {
        if (!message.IsValid())
        {
            NotifyValidationErrors(message);
            return Task.FromResult(false);
        }

        var eventEntity = _eventRepository.GetById(message.EventId);
        if (eventEntity == null)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, "The event does not exist."));
            return Task.FromResult(false);
        }

        try
        {
            eventEntity.RemoveInvitation(message.UserId);
            _eventRepository.Update(eventEntity);

            if (Commit())
            {
                _bus.RaiseEvent(new UserInvitationRemovedEvent(
                    eventEntity.Id,
                    eventEntity.OrganizerId,
                    message.UserId));
            }

            return Task.FromResult(true);
        }
        catch (InvalidOperationException ex)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, ex.Message));
            return Task.FromResult(false);
        }
    }

    public void Dispose()
    {
        _eventRepository.Dispose();
        _venueRepository.Dispose();
    }
}