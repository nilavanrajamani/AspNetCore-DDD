using System.Threading;
using System.Threading.Tasks;
using DDD.Domain.Events;
using MediatR;

namespace DDD.Domain.EventHandlers;

public class EventEventHandler : INotificationHandler<EventCreatedEvent>, INotificationHandler<EventCapacitySetEvent>
{
    public Task Handle(EventCreatedEvent message, CancellationToken cancellationToken)
    {
        // Send notification about event creation
        // Could send email to organizer, log to audit, etc.

        return Task.CompletedTask;
    }

    public Task Handle(EventCapacitySetEvent message, CancellationToken cancellationToken)
    {
        // Send notification about event capacity being set
        // Could notify interested parties, update analytics, etc.

        return Task.CompletedTask;
    }
}