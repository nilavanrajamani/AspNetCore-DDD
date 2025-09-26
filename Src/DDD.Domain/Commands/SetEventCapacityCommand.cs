using System;
using System.Collections.Generic;

using DDD.Domain.Models;
using DDD.Domain.Validations;

namespace DDD.Domain.Commands;

public class SetEventCapacityCommand : EventCommand
{
    public SetEventCapacityCommand(Guid eventId, int totalCapacity, List<PricingTierDefinition> pricingTiers)
    {
        Id = eventId;
        TotalCapacity = totalCapacity;
        PricingTiers = pricingTiers;
    }

    public int TotalCapacity { get; set; }

    public List<PricingTierDefinition> PricingTiers { get; set; }

    public override bool IsValid()
    {
        ValidationResult = new SetEventCapacityCommandValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}