using System;
using System.Collections.Generic;
using System.Linq;

using DDD.Domain.Core.Models;

namespace DDD.Domain.Models;

public class Event : EntityAudit
{
    private readonly List<PricingTier> _pricingTiers = new();

    public Event(Guid id, string title, string description, Guid organizerId, Guid venueId, DateTime startDate, DateTime endDate)
    {
        Id = id;
        Title = title;
        Description = description;
        OrganizerId = organizerId;
        VenueId = venueId;
        StartDate = startDate;
        EndDate = endDate;
        Status = EventStatus.Draft;
        Visibility = EventVisibility.Private;
        CreatedAt = DateTime.UtcNow;
    }

    // Empty constructor for EF
    protected Event()
    {
    }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public Guid OrganizerId { get; private set; }

    public Guid VenueId { get; private set; }

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public EventStatus Status { get; private set; }

    public EventVisibility Visibility { get; private set; }

    public int? TotalCapacity { get; private set; }

    public IReadOnlyList<PricingTier> PricingTiers => _pricingTiers.AsReadOnly();

    public void UpdateDetails(string title, string description, DateTime startDate, DateTime endDate)
    {
        Title = title;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCapacityAndPricing(int totalCapacity, IEnumerable<PricingTierDefinition> pricingTiers)
    {
        if (Status != EventStatus.Draft)
            throw new InvalidOperationException("Cannot modify capacity after event is published");

        var tiersList = pricingTiers.ToList();
        var totalTierCapacity = tiersList.Sum(t => t.Capacity);

        if (totalTierCapacity != totalCapacity)
            throw new InvalidOperationException("Pricing tier capacities must sum to total capacity");

        TotalCapacity = totalCapacity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReplacePricingTiers(IEnumerable<PricingTierDefinition> pricingTiers)
    {
        // Don't clear and re-add - instead, work with EF more explicitly
        // This method will be called by the repository to handle the collection properly
        _pricingTiers.Clear();

        foreach (var tierDef in pricingTiers)
        {
            _pricingTiers.Add(new PricingTier(
                Guid.NewGuid(),
                Id,
                tierDef.Name,
                tierDef.Price,
                tierDef.Currency,
                tierDef.Capacity,
                tierDef.SaleStartDate,
                tierDef.SaleEndDate));
        }
    }

    public bool HasAvailableCapacity(int requestedQuantity = 1)
    {
        if (!TotalCapacity.HasValue)
            return false;

        var reservedCapacity = _pricingTiers.Sum(t => t.Capacity - t.AvailableCapacity);
        return reservedCapacity + requestedQuantity <= TotalCapacity.Value;
    }

    public PricingTier GetAvailablePricingTier(string tierName, int quantity)
    {
        var tier = _pricingTiers.FirstOrDefault(t => t.Name == tierName);
        if (tier == null)
            throw new InvalidOperationException($"Pricing tier '{tierName}' not found");

        if (!tier.CanBookTickets(quantity))
            throw new InvalidOperationException($"Cannot book {quantity} tickets for tier '{tierName}'");

        return tier;
    }

    public void Publish()
    {
        if (Status != EventStatus.Draft)
            throw new InvalidOperationException("Only draft events can be published");

        ValidateEventCompleteness();

        Status = EventStatus.Published;
        Visibility = EventVisibility.Public;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unpublish(string reason)
    {
        if (Status != EventStatus.Published)
            throw new InvalidOperationException("Only published events can be unpublished");

        Status = EventStatus.Draft;
        Visibility = EventVisibility.Private;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status == EventStatus.Cancelled)
            throw new InvalidOperationException("Event is already cancelled");

        if (Status == EventStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed event");

        Status = EventStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    private void ValidateEventCompleteness()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Title))
            errors.Add("Event must have a title");

        if (string.IsNullOrWhiteSpace(Description))
            errors.Add("Event must have a description");

        if (VenueId == Guid.Empty)
            errors.Add("Event must have a venue");

        if (!TotalCapacity.HasValue || TotalCapacity <= 0)
            errors.Add("Event must have valid capacity");

        if (!_pricingTiers.Any())
            errors.Add("Event must have pricing tiers");

        if (StartDate <= DateTime.UtcNow)
            errors.Add("Event must have future start date");

        if (EndDate <= StartDate)
            errors.Add("Event end date must be after start date");

        if (errors.Any())
            throw new InvalidOperationException($"Cannot publish incomplete event: {string.Join(", ", errors)}");
    }
}

public enum EventStatus
{
    Draft,
    Published,
    Cancelled,
    Completed,
}

public enum EventVisibility
{
    Private,
    Public,
    InviteOnly,
}