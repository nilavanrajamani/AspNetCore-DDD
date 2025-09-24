using System;

using DDD.Domain.Commands;

using FluentValidation;

namespace DDD.Domain.Validations;

public abstract class EventValidation<T> : AbstractValidator<T>
    where T : EventCommand
{
    protected static bool HaveFutureDate(DateTime eventDate)
    {
        return eventDate > DateTime.Now;
    }

    protected static bool HaveValidDateRange(DateTime startDate, DateTime endDate)
    {
        return endDate > startDate;
    }

    protected void ValidateTitle()
    {
        RuleFor(e => e.Title)
            .NotEmpty().WithMessage("Please ensure you have entered the Title")
            .Length(2, 200).WithMessage("The Title must have between 2 and 200 characters");
    }

    protected void ValidateDescription()
    {
        RuleFor(e => e.Description)
            .NotEmpty().WithMessage("Please ensure you have entered the Description")
            .Length(10, 2000).WithMessage("The Description must have between 10 and 2000 characters");
    }

    protected void ValidateOrganizerId()
    {
        RuleFor(e => e.OrganizerId)
            .NotEqual(Guid.Empty).WithMessage("Please ensure you have entered a valid Organizer ID");
    }

    protected void ValidateVenueId()
    {
        RuleFor(e => e.VenueId)
            .NotEqual(Guid.Empty).WithMessage("Please ensure you have entered a valid Venue ID");
    }

    protected void ValidateStartDate()
    {
        RuleFor(e => e.StartDate)
            .NotEmpty()
            .Must(HaveFutureDate)
            .WithMessage("The event must be scheduled for a future date");
    }

    protected void ValidateEndDate()
    {
        RuleFor(e => e.EndDate)
            .NotEmpty();

        RuleFor(e => e)
            .Must(e => HaveValidDateRange(e.StartDate, e.EndDate))
            .WithMessage("The end date must be after the start date");
    }

    protected void ValidateId()
    {
        RuleFor(e => e.Id)
            .NotEqual(Guid.Empty);
    }
}