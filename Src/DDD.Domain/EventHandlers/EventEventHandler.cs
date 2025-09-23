using System.Threading;
using System.Threading.Tasks;
using DDD.Domain.Events;
using MediatR;

namespace DDD.Domain.EventHandlers;

public class EventEventHandler : INotificationHandler<EventCreatedEvent>
{
    public Task Handle(EventCreatedEvent message, CancellationToken cancellationToken)
    {
        // Send notification about event creation
        // Could send email to organizer, log to audit, etc.

        return Task.CompletedTask;
    }
}