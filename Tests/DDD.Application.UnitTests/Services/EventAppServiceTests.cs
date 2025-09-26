using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper;

using DDD.Application.Interfaces;
using DDD.Application.Services;
using DDD.Application.ViewModels;
using DDD.Domain.Commands;
using DDD.Domain.Core.Bus;
using DDD.Domain.Interfaces;
using DDD.Domain.Models;

using Moq;

using Xunit;

namespace DDD.Application.UnitTests.Services;

public class EventAppServiceTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IMediatorHandler> _mediatorHandlerMock;
    private readonly EventAppService _eventAppService;

    public EventAppServiceTests()
    {
        _mapperMock = new Mock<IMapper>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _mediatorHandlerMock = new Mock<IMediatorHandler>();
        _eventAppService = new EventAppService(_mapperMock.Object, _eventRepositoryMock.Object, _mediatorHandlerMock.Object);
    }

    [Fact]
    public void SetCapacityAndPricing_WithValidViewModel_SendsCommand()
    {
        // Arrange
        var viewModel = new SetEventCapacityViewModel
        {
            EventId = Guid.NewGuid(),
            TotalCapacity = 150,
            PricingTiers = new List<PricingTierViewModel>
            {
                new()
                {
                    Name = "General Admission",
                    Price = 50.00m,
                    Currency = "USD",
                    Capacity = 100,
                    SaleStartDate = DateTime.Now,
                    SaleEndDate = DateTime.Now.AddDays(25),
                },
                new()
                {
                    Name = "VIP",
                    Price = 150.00m,
                    Currency = "USD",
                    Capacity = 50,
                    SaleStartDate = DateTime.Now,
                    SaleEndDate = DateTime.Now.AddDays(25),
                },
            },
        };

        var expectedPricingTiers = new List<PricingTierDefinition>
        {
            new("General Admission", 50.00m, "USD", 100, DateTime.Now, DateTime.Now.AddDays(25)),
            new("VIP", 150.00m, "USD", 50, DateTime.Now, DateTime.Now.AddDays(25)),
        };

        _mapperMock.Setup(x => x.Map<List<PricingTierDefinition>>(viewModel.PricingTiers))
                   .Returns(expectedPricingTiers);

        // Act
        _eventAppService.SetCapacityAndPricing(viewModel);

        // Assert
        _mediatorHandlerMock.Verify(
            x => x.SendCommand(It.Is<SetEventCapacityCommand>(cmd =>
                cmd.Id == viewModel.EventId &&
                cmd.TotalCapacity == viewModel.TotalCapacity &&
                cmd.PricingTiers.Count == expectedPricingTiers.Count)),
            Times.Once);
    }

    [Fact]
    public void SetCapacityAndPricing_WithNullViewModel_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _eventAppService.SetCapacityAndPricing(null));
    }

    [Fact]
    public void SetCapacityAndPricing_WithValidViewModel_MapsCorrectly()
    {
        // Arrange
        var viewModel = new SetEventCapacityViewModel
        {
            EventId = Guid.NewGuid(),
            TotalCapacity = 200,
            PricingTiers = new List<PricingTierViewModel>
            {
                new()
                {
                    Name = "Early Bird",
                    Price = 40.00m,
                    Currency = "USD",
                    Capacity = 150,
                    SaleStartDate = DateTime.Now,
                    SaleEndDate = DateTime.Now.AddDays(10),
                },
                new()
                {
                    Name = "Regular",
                    Price = 60.00m,
                    Currency = "USD",
                    Capacity = 50,
                    SaleStartDate = DateTime.Now.AddDays(10),
                    SaleEndDate = DateTime.Now.AddDays(25),
                },
            },
        };

        var expectedPricingTiers = new List<PricingTierDefinition>
        {
            new("Early Bird", 40.00m, "USD", 150, DateTime.Now, DateTime.Now.AddDays(10)),
            new("Regular", 60.00m, "USD", 50, DateTime.Now.AddDays(10), DateTime.Now.AddDays(25)),
        };

        _mapperMock.Setup(x => x.Map<List<PricingTierDefinition>>(viewModel.PricingTiers))
                   .Returns(expectedPricingTiers);

        SetEventCapacityCommand capturedCommand = null;
        _mediatorHandlerMock.Setup(x => x.SendCommand(It.IsAny<SetEventCapacityCommand>()))
                           .Callback<SetEventCapacityCommand>(cmd => capturedCommand = cmd);

        // Act
        _eventAppService.SetCapacityAndPricing(viewModel);

        // Assert
        Assert.NotNull(capturedCommand);
        Assert.Equal(viewModel.EventId, capturedCommand.Id);
        Assert.Equal(viewModel.TotalCapacity, capturedCommand.TotalCapacity);
        Assert.Equal(expectedPricingTiers.Count, capturedCommand.PricingTiers.Count);
    }
}