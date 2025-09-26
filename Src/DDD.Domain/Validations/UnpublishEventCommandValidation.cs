using System;

using DDD.Domain.Commands;

using FluentValidation;

namespace DDD.Domain.Validations;

public class UnpublishEventCommandValidation : EventValidation<UnpublishEventCommand>
{
    public UnpublishEventCommandValidation()
    {
        ValidateId();
        ValidateReason();
    }

    protected void ValidateReason()
    {
        RuleFor(c => c.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .Length(3, 500).WithMessage("Reason must be between 3 and 500 characters");
    }
}