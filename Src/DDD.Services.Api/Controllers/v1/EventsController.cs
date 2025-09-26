using System;

using DDD.Application.Interfaces;
using DDD.Application.ViewModels;
using DDD.Domain.Core.Bus;
using DDD.Domain.Core.Notifications;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDD.Services.Api.Controllers.V1;

[Authorize]
[ApiVersion("1.0")]
public class EventsController : ApiController
{
    private readonly IEventAppService _eventAppService;

    public EventsController(
        IEventAppService eventAppService,
        INotificationHandler<DomainNotification> notifications,
        IMediatorHandler mediator)
        : base(notifications, mediator)
    {
        _eventAppService = eventAppService;
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("event-management")]
    public IActionResult Get()
    {
        return Response(_eventAppService.GetAll());
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("event-management/{id:guid}")]
    public IActionResult Get(Guid id)
    {
        var eventViewModel = _eventAppService.GetById(id);
        return Response(eventViewModel);
    }

    [HttpPost]
    [Authorize(Policy = "CanModifyEventsData")]
    [Route("event-management")]
    public IActionResult Post([FromBody] EventViewModel eventViewModel)
    {
        if (!ModelState.IsValid)
        {
            NotifyModelStateErrors();
            return Response(eventViewModel);
        }

        _eventAppService.Register(eventViewModel);

        return Response(eventViewModel);
    }

    [HttpPut]
    [Authorize(Policy = "CanModifyEventsData")]
    [Route("event-management/{id:guid}")]
    public IActionResult Put(Guid id, [FromBody] EventViewModel eventViewModel)
    {
        if (!ModelState.IsValid)
        {
            NotifyModelStateErrors();
            return Response(eventViewModel);
        }

        eventViewModel.Id = id;
        _eventAppService.Update(eventViewModel);

        return Response(eventViewModel);
    }

    [HttpDelete]
    [Authorize(Policy = "CanModifyEventsData")]
    [Route("event-management/{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        _eventAppService.Remove(id);
        return Response();
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("event-management/pagination")]
    public IActionResult Pagination(int skip, int take)
    {
        return Response(_eventAppService.GetAll(skip, take));
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("event-management/{id:guid}/capacity")]
    public IActionResult GetCapacityAndPricing(Guid id)
    {
        var capacityViewModel = _eventAppService.GetCapacityAndPricing(id);
        if (capacityViewModel == null)
        {
            return NotFound();
        }
        return Response(capacityViewModel);
    }

    [HttpPut]
    [Authorize(Policy = "CanModifyEventsData")]
    [Route("event-management/{id:guid}/capacity")]
    public IActionResult SetCapacityAndPricing(Guid id, [FromBody] SetEventCapacityViewModel capacityViewModel)
    {
        if (!ModelState.IsValid)
        {
            NotifyModelStateErrors();
            return Response(capacityViewModel);
        }

        capacityViewModel.EventId = id;
        _eventAppService.SetCapacityAndPricing(capacityViewModel);

        return Response(capacityViewModel);
    }

    [HttpPut]
    [Authorize(Policy = "CanModifyEventsData")]
    [Route("event-management/{id:guid}/publish")]
    public IActionResult PublishEvent(Guid id)
    {
        _eventAppService.PublishEvent(id);
        return Response();
    }

    [HttpPut]
    [Authorize(Policy = "CanModifyEventsData")]
    [Route("event-management/{id:guid}/unpublish")]
    public IActionResult UnpublishEvent(Guid id, [FromBody] UnpublishEventRequest request)
    {
        if (!ModelState.IsValid)
        {
            NotifyModelStateErrors();
            return Response(request);
        }

        _eventAppService.UnpublishEvent(id, request.Reason);
        return Response();
    }
}