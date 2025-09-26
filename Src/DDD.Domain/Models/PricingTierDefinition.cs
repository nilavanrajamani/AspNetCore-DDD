using System;

namespace DDD.Domain.Models;

public record PricingTierDefinition(
    string Name,
    decimal Price,
    string Currency,
    int Capacity,
    DateTime SaleStartDate,
    DateTime SaleEndDate);