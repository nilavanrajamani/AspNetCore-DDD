using System;

using DDD.Domain.Commands;

using FluentValidation;

namespace DDD.Domain.Validations;

public class PublishEventCommandValidation : EventValidation<PublishEventCommand>
{
    public PublishEventCommandValidation()
    {
        ValidateId();
    }
}