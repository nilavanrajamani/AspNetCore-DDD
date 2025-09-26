using System;
using System.Collections.Generic;
using DDD.Domain.Commands;
using DDD.Domain.Models;
using Xunit;

namespace DDD.Application.UnitTests.Commands;

public class SetEventCapacityCommandTests
{
    [Fact]
    public void SetEventCapacityCommand_WithValidData_IsValid()
    {
        // Arrange
        var command = new SetEventCapacityCommand(
            Guid.NewGuid(),
            100,
            new List<PricingTierDefinition>
            {
                new("General Admission", 50.00m, "USD", 60, DateTime.Now, DateTime.Now.AddDays(25)),
                new("VIP", 150.00m, "USD", 40, DateTime.Now, DateTime.Now.AddDays(25))
            }
        );

        // Act
        var isValid = command.IsValid();

        // Assert
        Assert.True(isValid);
        Assert.Empty(command.ValidationResult.Errors);
    }

    [Fact]
    public void SetEventCapacityCommand_WithInvalidEventId_IsInvalid()
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
        var isValid = command.IsValid();

        // Assert
        Assert.False(isValid);
        Assert.Contains(command.ValidationResult.Errors, error => error.PropertyName == "EventId");
    }

    [Fact]
    public void SetEventCapacityCommand_WithInvalidTotalCapacity_IsInvalid()
    {
        // Arrange
        var command = new SetEventCapacityCommand(
            Guid.NewGuid(),
            0, // Invalid capacity
            new List<PricingTierDefinition>
            {
                new("General Admission", 50.00m, "USD", 100, DateTime.Now, DateTime.Now.AddDays(25))
            }
        );

        // Act
        var isValid = command.IsValid();

        // Assert
        Assert.False(isValid);
        Assert.Contains(command.ValidationResult.Errors, error => error.PropertyName == "TotalCapacity");
    }

    [Fact]
    public void SetEventCapacityCommand_WithMismatchedCapacities_IsInvalid()
    {
        // Arrange
        var command = new SetEventCapacityCommand(
            Guid.NewGuid(),
            100, // Total capacity = 100
            new List<PricingTierDefinition>
            {
                new("General Admission", 50.00m, "USD", 60, DateTime.Now, DateTime.Now.AddDays(25)),
                new("VIP", 150.00m, "USD", 50, DateTime.Now, DateTime.Now.AddDays(25)) // Total tier capacity = 110
            }
        );

        // Act
        var isValid = command.IsValid();

        // Assert
        Assert.False(isValid);
        Assert.Contains(command.ValidationResult.Errors, error => error.ErrorMessage.Contains("sum of pricing tier capacities"));
    }

    [Fact]
    public void SetEventCapacityCommand_WithEmptyPricingTiers_IsInvalid()
    {
        // Arrange
        var command = new SetEventCapacityCommand(
            Guid.NewGuid(),
            100,
            new List<PricingTierDefinition>() // Empty list
        );

        // Act
        var isValid = command.IsValid();

        // Assert
        Assert.False(isValid);
        Assert.Contains(command.ValidationResult.Errors, error => error.PropertyName == "PricingTiers");
    }

    [Fact]
    public void SetEventCapacityCommand_WithNullPricingTiers_IsInvalid()
    {
        // Arrange
        var command = new SetEventCapacityCommand(
            Guid.NewGuid(),
            100,
            null // Null pricing tiers
        );

        // Act
        var isValid = command.IsValid();

        // Assert
        Assert.False(isValid);
        Assert.Contains(command.ValidationResult.Errors, error => error.PropertyName == "PricingTiers");
    }

    [Fact]
    public void SetEventCapacityCommand_WithInvalidPricingTierData_IsInvalid()
    {
        // Arrange
        var command = new SetEventCapacityCommand(
            Guid.NewGuid(),
            100,
            new List<PricingTierDefinition>
            {
                new("", -10.00m, "", 0, DateTime.Now, DateTime.Now.AddDays(-1)) // All invalid values
            }
        );

        // Act
        var isValid = command.IsValid();

        // Assert
        Assert.False(isValid);
        Assert.True(command.ValidationResult.Errors.Count > 0);
    }
}