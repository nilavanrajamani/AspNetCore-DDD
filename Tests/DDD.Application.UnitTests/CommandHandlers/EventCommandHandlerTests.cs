using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDD.Domain.CommandHandlers;
using DDD.Domain.Commands;
using DDD.Domain.Core.Bus;
using DDD.Domain.Core.Notifications;
using DDD.Domain.Interfaces;
using DDD.Domain.Models;
using MediatR;
using Moq;
using Xunit;

namespace DDD.Application.UnitTests.CommandHandlers;

public class EventCommandHandlerTests
{
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IVenueRepository> _venueRepositoryMock;
    private readonly Mock<IMediatorHandler> _busHandlerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<INotificationHandler<DomainNotification>> _notificationHandlerMock;
    private readonly EventCommandHandler _commandHandler;

    public EventCommandHandlerTests()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _venueRepositoryMock = new Mock<IVenueRepository>();
        _busHandlerMock = new Mock<IMediatorHandler>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationHandlerMock = new Mock<INotificationHandler<DomainNotification>>();

        _commandHandler = new EventCommandHandler(
            _eventRepositoryMock.Object,
            _venueRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _busHandlerMock.Object,
            _notificationHandlerMock.Object);
    }

    [Fact]
    public async Task Handle_SetEventCapacityCommand_WithValidCommand_UpdatesEventCapacity()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var existingEvent = new Event(eventId, "Test Event", "Description", Guid.NewGuid(), venueId, DateTime.Now.AddDays(30), DateTime.Now.AddDays(31));
        
        var command = new SetEventCapacityCommand(
            eventId,
            150,
            new List<PricingTierDefinition>
            {
                new("General Admission", 50.00m, "USD", 100, DateTime.Now, DateTime.Now.AddDays(25)),
                new("VIP", 150.00m, "USD", 50, DateTime.Now, DateTime.Now.AddDays(25))
            }
        );

        _eventRepositoryMock.Setup(x => x.GetById(eventId))
            .Returns(existingEvent);
        
        _unitOfWorkMock.Setup(x => x.Commit())
            .Returns(true);

        // Act
        var result = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(150, existingEvent.TotalCapacity);
        Assert.Equal(2, existingEvent.PricingTiers.Count);
        
        _eventRepositoryMock.Verify(x => x.Update(existingEvent), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task Handle_SetEventCapacityCommand_WithNonExistentEvent_ReturnsFalse()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var command = new SetEventCapacityCommand(
            eventId,
            100,
            new List<PricingTierDefinition>
            {
                new("General Admission", 50.00m, "USD", 100, DateTime.Now, DateTime.Now.AddDays(25))
            }
        );

        _eventRepositoryMock.Setup(x => x.GetById(eventId))
            .Returns((Event)null); // Event not found

        // Act
        var result = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _eventRepositoryMock.Verify(x => x.Update(It.IsAny<Event>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Never);
    }

    [Fact]
    public async Task Handle_SetEventCapacityCommand_WithInvalidCommand_ReturnsFalse()
    {
        // Arrange
        var command = new SetEventCapacityCommand(
            Guid.Empty, // Invalid event ID
            100,
            new List<PricingTierDefinition>
            {
                new("General Admission", 50.00m, "USD", 100, DateTime.Now, DateTime.Now.AddDays(25))
            }
        );

        // Act
        var result = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        
        // Verify that validation error notifications are sent
        _busHandlerMock.Verify(x => x.RaiseEvent(It.IsAny<DDD.Domain.Core.Notifications.DomainNotification>()), Times.AtLeastOnce);
        _eventRepositoryMock.Verify(x => x.Update(It.IsAny<Event>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Never);
    }

    [Fact]
    public async Task Handle_SetEventCapacityCommand_WithCapacityMismatch_ReturnsFalse()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var existingEvent = new Event(eventId, "Test Event", "Description", Guid.NewGuid(), venueId, DateTime.Now.AddDays(30), DateTime.Now.AddDays(31));
        
        var command = new SetEventCapacityCommand(
            eventId,
            100, // Total capacity = 100
            new List<PricingTierDefinition>
            {
                new("General Admission", 50.00m, "USD", 60, DateTime.Now, DateTime.Now.AddDays(25)),
                new("VIP", 150.00m, "USD", 50, DateTime.Now, DateTime.Now.AddDays(25)) // Total = 110
            }
        );

        _eventRepositoryMock.Setup(x => x.GetById(eventId))
            .Returns(existingEvent);

        // Act
        var result = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _busHandlerMock.Verify(x => x.RaiseEvent(It.IsAny<DDD.Domain.Core.Notifications.DomainNotification>()), Times.AtLeastOnce);
        _eventRepositoryMock.Verify(x => x.Update(It.IsAny<Event>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Never);
    }

    [Fact]
    public async Task Handle_SetEventCapacityCommand_WhenCommitFails_ReturnsFalse()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var existingEvent = new Event(eventId, "Test Event", "Description", Guid.NewGuid(), venueId, DateTime.Now.AddDays(30), DateTime.Now.AddDays(31));
        
        var command = new SetEventCapacityCommand(
            eventId,
            100,
            new List<PricingTierDefinition>
            {
                new("General Admission", 50.00m, "USD", 100, DateTime.Now, DateTime.Now.AddDays(25))
            }
        );

        _eventRepositoryMock.Setup(x => x.GetById(eventId))
            .Returns(existingEvent);
        
        _unitOfWorkMock.Setup(x => x.Commit())
            .Returns(false); // Commit fails

        // Act
        var result = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _eventRepositoryMock.Verify(x => x.Update(existingEvent), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
    }
}