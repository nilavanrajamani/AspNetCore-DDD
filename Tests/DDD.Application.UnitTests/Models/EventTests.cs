using System;
using System.Collections.Generic;
using System.Linq;
using DDD.Domain.Models;
using Xunit;

namespace DDD.Application.UnitTests.Models;

public class EventTests
{
    [Fact]
    public void SetCapacityAndPricing_WithValidData_SetsCapacityAndPricingTiers()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var @event = new Event(eventId, "Test Event", "Test Description", organizerId, venueId, DateTime.Now.AddDays(30), DateTime.Now.AddDays(31));

        var pricingTierDefinitions = new List<PricingTierDefinition>
        {
            new("General Admission", 50.00m, "USD", 100, DateTime.Now, DateTime.Now.AddDays(25)),
            new("VIP", 150.00m, "USD", 50, DateTime.Now, DateTime.Now.AddDays(25)),
        };

        // Act
        @event.SetCapacityAndPricing(150, pricingTierDefinitions);

        // Assert
        Assert.Equal(150, @event.TotalCapacity);
        Assert.Equal(2, @event.PricingTiers.Count);
        
        var generalAdmission = @event.PricingTiers.First(pt => pt.Name == "General Admission");
        Assert.Equal(50.00m, generalAdmission.Price);
        Assert.Equal("USD", generalAdmission.Currency);
        Assert.Equal(100, generalAdmission.Capacity);
        Assert.Equal(100, generalAdmission.AvailableCapacity);
        
        var vip = @event.PricingTiers.First(pt => pt.Name == "VIP");
        Assert.Equal(150.00m, vip.Price);
        Assert.Equal("USD", vip.Currency);
        Assert.Equal(50, vip.Capacity);
        Assert.Equal(50, vip.AvailableCapacity);
    }

    [Fact]
    public void SetCapacityAndPricing_WithInvalidTotalCapacity_ThrowsArgumentException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var @event = new Event(eventId, "Test Event", "Test Description", organizerId, venueId, DateTime.Now.AddDays(30), DateTime.Now.AddDays(31));

        var pricingTierDefinitions = new List<PricingTierDefinition>
        {
            new("General Admission", 50.00m, "USD", 100, DateTime.Now, DateTime.Now.AddDays(25)),
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => @event.SetCapacityAndPricing(0, pricingTierDefinitions));
        Assert.Throws<ArgumentException>(() => @event.SetCapacityAndPricing(-1, pricingTierDefinitions));
    }

    [Fact]
    public void SetCapacityAndPricing_WithCapacityMismatch_ThrowsArgumentException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var @event = new Event(eventId, "Test Event", "Test Description", organizerId, venueId, DateTime.Now.AddDays(30), DateTime.Now.AddDays(31));

        var pricingTierDefinitions = new List<PricingTierDefinition>
        {
            new("General Admission", 50.00m, "USD", 100, DateTime.Now, DateTime.Now.AddDays(25)),
            new("VIP", 150.00m, "USD", 60, DateTime.Now, DateTime.Now.AddDays(25)), // Total = 160, but totalCapacity = 150
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => @event.SetCapacityAndPricing(150, pricingTierDefinitions));
        Assert.Contains("sum of pricing tier capacities", exception.Message);
    }

    [Fact]
    public void SetCapacityAndPricing_WithNullPricingTiers_ThrowsArgumentNullException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var @event = new Event(eventId, "Test Event", "Test Description", organizerId, venueId, DateTime.Now.AddDays(30), DateTime.Now.AddDays(31));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => @event.SetCapacityAndPricing(100, null));
    }

    [Fact]
    public void SetCapacityAndPricing_WithEmptyPricingTiers_ThrowsArgumentException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var @event = new Event(eventId, "Test Event", "Test Description", organizerId, venueId, DateTime.Now.AddDays(30), DateTime.Now.AddDays(31));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => @event.SetCapacityAndPricing(100, new List<PricingTierDefinition>()));
    }

    [Fact]
    public void HasAvailableCapacity_WithAvailableCapacity_ReturnsTrue()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var @event = new Event(eventId, "Test Event", "Test Description", organizerId, venueId, DateTime.Now.AddDays(30), DateTime.Now.AddDays(31));

        var pricingTierDefinitions = new List<PricingTierDefinition>
        {
            new("General Admission", 50.00m, "USD", 100, DateTime.Now, DateTime.Now.AddDays(25)),
        };
        
        @event.SetCapacityAndPricing(100, pricingTierDefinitions);

        // Act & Assert
        Assert.True(@event.HasAvailableCapacity());
        Assert.True(@event.HasAvailableCapacity(50));
        Assert.True(@event.HasAvailableCapacity(100));
        Assert.False(@event.HasAvailableCapacity(101));
    }

    [Fact]
    public void GetAvailablePricingTier_WithAvailableTier_ReturnsTier()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var @event = new Event(eventId, "Test Event", "Test Description", organizerId, venueId, DateTime.Now.AddDays(30), DateTime.Now.AddDays(31));

        var pricingTierDefinitions = new List<PricingTierDefinition>
        {
            new("General Admission", 50.00m, "USD", 100, DateTime.Now, DateTime.Now.AddDays(25)),
            new("VIP", 150.00m, "USD", 50, DateTime.Now, DateTime.Now.AddDays(25)),
        };

        @event.SetCapacityAndPricing(150, pricingTierDefinitions);

        // Act
        var generalAdmissionTier = @event.GetAvailablePricingTier("General Admission", 5);
        var vipTier = @event.GetAvailablePricingTier("VIP", 5);

        // Assert
        Assert.NotNull(generalAdmissionTier);
        Assert.Equal("General Admission", generalAdmissionTier.Name);
        Assert.NotNull(vipTier);
        Assert.Equal("VIP", vipTier.Name);

        // Test for non-existent tier
        Assert.Throws<InvalidOperationException>(() => @event.GetAvailablePricingTier("Non-existent", 5));
    }

    [Fact]
    public void Event_CreatedWithValidData_HasCorrectProperties()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var title = "Test Event";
        var description = "Test Description";
        var startDate = DateTime.Now.AddDays(30);
        var endDate = DateTime.Now.AddDays(31);

        // Act
        var @event = new Event(eventId, title, description, organizerId, venueId, startDate, endDate);

        // Assert
        Assert.Equal(eventId, @event.Id);
        Assert.Equal(title, @event.Title);
        Assert.Equal(description, @event.Description);
        Assert.Equal(startDate, @event.StartDate);
        Assert.Equal(endDate, @event.EndDate);
        Assert.Equal(organizerId, @event.OrganizerId);
        Assert.Equal(venueId, @event.VenueId);
        Assert.Null(@event.TotalCapacity);
        Assert.Empty(@event.PricingTiers);
    }
}

