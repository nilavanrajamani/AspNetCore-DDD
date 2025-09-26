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
    IRequestHandler<SetEventCapacityCommand, bool>
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

    public void Dispose()
    {
        _eventRepository.Dispose();
        _venueRepository.Dispose();
    }
}