using System;
using System.Linq;

using DDD.Domain.Commands;

using FluentValidation;

namespace DDD.Domain.Validations;

public class SetEventCapacityCommandValidation : AbstractValidator<SetEventCapacityCommand>
{
    public SetEventCapacityCommandValidation()
    {
        ValidateId();
        ValidateTotalCapacity();
        ValidatePricingTiers();
    }

    protected void ValidateId()
    {
        RuleFor(c => c.Id)
            .NotEqual(Guid.Empty);
    }

    protected void ValidateTotalCapacity()
    {
        RuleFor(c => c.TotalCapacity)
            .GreaterThan(0)
            .WithMessage("Total capacity must be greater than 0")
            .LessThanOrEqualTo(50000)
            .WithMessage("Total capacity cannot exceed 50,000");
    }

    protected void ValidatePricingTiers()
    {
        RuleFor(c => c.PricingTiers)
            .NotNull()
            .WithMessage("At least one pricing tier must be provided")
            .Must(tiers => tiers.Count > 0)
            .WithMessage("At least one pricing tier must be provided");

        RuleForEach(c => c.PricingTiers).ChildRules(tier =>
        {
            tier.RuleFor(t => t.Name)
                .NotEmpty()
                .WithMessage("Pricing tier name is required")
                .MaximumLength(100)
                .WithMessage("Pricing tier name cannot exceed 100 characters");

            tier.RuleFor(t => t.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Pricing tier price cannot be negative");

            tier.RuleFor(t => t.Currency)
                .NotEmpty()
                .WithMessage("Currency is required")
                .Length(3)
                .WithMessage("Currency must be a 3-letter code (e.g., USD, EUR)");

            tier.RuleFor(t => t.Capacity)
                .GreaterThan(0)
                .WithMessage("Pricing tier capacity must be greater than 0");

            tier.RuleFor(t => t.SaleEndDate)
                .GreaterThan(t => t.SaleStartDate)
                .WithMessage("Sale end date must be after sale start date");
        });

        // Validate that pricing tier capacities sum to total capacity
        RuleFor(c => c)
            .Must(c => c.PricingTiers.Sum(t => t.Capacity) == c.TotalCapacity)
            .WithMessage("The sum of all pricing tier capacities must equal the total event capacity");

        // Validate unique tier names
        RuleFor(c => c.PricingTiers)
            .Must(tiers => tiers.Select(t => t.Name).Distinct().Count() == tiers.Count)
            .WithMessage("Pricing tier names must be unique");
    }
}