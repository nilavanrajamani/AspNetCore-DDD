using System;

using DDD.Domain.Core.Models;

namespace DDD.Domain.Models;

public class PricingTier : EntityAudit
{
    public PricingTier(Guid id, Guid eventId, string name, decimal price, string currency, int capacity, DateTime saleStartDate, DateTime saleEndDate)
    {
        Id = id;
        EventId = eventId;
        Name = name;
        Price = price;
        Currency = currency;
        Capacity = capacity;
        AvailableCapacity = capacity;
        SaleStartDate = saleStartDate;
        SaleEndDate = saleEndDate;
        CreatedAt = DateTime.UtcNow;
    }

    // Empty constructor for EF
    protected PricingTier()
    {
    }

    public Guid EventId { get; private set; }

    public string Name { get; private set; }

    public decimal Price { get; private set; }

    public string Currency { get; private set; }

    public int Capacity { get; private set; }

    public int AvailableCapacity { get; private set; }

    public DateTime SaleStartDate { get; private set; }

    public DateTime SaleEndDate { get; private set; }

    public bool IsActive => DateTime.UtcNow >= SaleStartDate && DateTime.UtcNow <= SaleEndDate;

    public bool CanBookTickets(int quantity)
    {
        return IsActive && AvailableCapacity >= quantity;
    }

    public void ReserveCapacity(int quantity)
    {
        if (!CanBookTickets(quantity))
            throw new InvalidOperationException($"Cannot reserve {quantity} tickets for tier {Name}");

        AvailableCapacity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseCapacity(int quantity)
    {
        AvailableCapacity = Math.Min(AvailableCapacity + quantity, Capacity);
        UpdatedAt = DateTime.UtcNow;
    }
}