using System;
using DDD.Domain.Models;
using Xunit;

namespace DDD.Application.UnitTests.Models;

public class PricingTierTests
{
    [Fact]
    public void PricingTier_CreatedWithValidData_HasCorrectProperties()
    {
        // Arrange
        var tierId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var name = "VIP";
        var price = 150.00m;
        var currency = "USD";
        var capacity = 50;
        var saleStartDate = DateTime.Now;
        var saleEndDate = DateTime.Now.AddDays(30);

        // Act
        var pricingTier = new PricingTier(tierId, eventId, name, price, currency, capacity, saleStartDate, saleEndDate);

        // Assert
        Assert.Equal(name, pricingTier.Name);
        Assert.Equal(price, pricingTier.Price);
        Assert.Equal(currency, pricingTier.Currency);
        Assert.Equal(capacity, pricingTier.Capacity);
        Assert.Equal(capacity, pricingTier.AvailableCapacity);
        Assert.Equal(saleStartDate, pricingTier.SaleStartDate);
        Assert.Equal(saleEndDate, pricingTier.SaleEndDate);
        Assert.Equal(eventId, pricingTier.EventId);
    }

    [Fact]
    public void CanBookTickets_WithAvailableCapacity_ReturnsTrue()
    {
        // Arrange
        var pricingTier = new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "General Admission",
            50.00m,
            "USD",
            100,
            DateTime.Now.AddDays(-1),
            DateTime.Now.AddDays(25));

        // Act & Assert
        Assert.True(pricingTier.CanBookTickets(1));
        Assert.True(pricingTier.CanBookTickets(50));
        Assert.True(pricingTier.CanBookTickets(100));
        Assert.False(pricingTier.CanBookTickets(101));
    }

    [Fact]
    public void ReserveCapacity_WithValidQuantity_ReducesAvailableCapacity()
    {
        // Arrange
        var pricingTier = new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "General Admission",
            50.00m,
            "USD",
            100,
            DateTime.Now.AddDays(-1),
            DateTime.Now.AddDays(25));

        // Act
        pricingTier.ReserveCapacity(30);

        // Assert
        Assert.Equal(70, pricingTier.AvailableCapacity);
    }

    [Fact]
    public void ReserveCapacity_WithExcessiveQuantity_ThrowsInvalidOperationException()
    {
        // Arrange
        var pricingTier = new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "General Admission",
            50.00m,
            "USD",
            100,
            DateTime.Now.AddDays(-1),
            DateTime.Now.AddDays(25));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => pricingTier.ReserveCapacity(101));
    }

    [Fact]
    public void ReleaseCapacity_WithValidQuantity_IncreasesAvailableCapacity()
    {
        // Arrange
        var pricingTier = new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "General Admission",
            50.00m,
            "USD",
            100,
            DateTime.Now.AddDays(-1),
            DateTime.Now.AddDays(25));

        pricingTier.ReserveCapacity(30);

        // Act
        pricingTier.ReleaseCapacity(10);

        // Assert
        Assert.Equal(80, pricingTier.AvailableCapacity);
    }

    [Fact]
    public void ReleaseCapacity_WithExcessiveQuantity_ThrowsInvalidOperationException()
    {
        // Arrange
        var pricingTier = new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "General Admission",
            50.00m,
            "USD",
            100,
            DateTime.Now.AddDays(-1),
            DateTime.Now.AddDays(25));

        pricingTier.ReserveCapacity(30);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => pricingTier.ReleaseCapacity(80));
    }
}