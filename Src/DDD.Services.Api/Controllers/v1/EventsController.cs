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

    [HttpPut]
    [Authorize(Policy = "CanModifyEventsData")]
    [Route("event-management/{id:guid}/details")]
    public IActionResult UpdateEventDetails(Guid id, [FromBody] UpdateEventViewModel updateEventViewModel)
    {
        if (!ModelState.IsValid)
        {
            NotifyModelStateErrors();
            return Response(updateEventViewModel);
        }

        updateEventViewModel.Id = id;
        _eventAppService.UpdateEventDetails(updateEventViewModel);

        return Response();
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

    [HttpPut]
    [Authorize(Policy = "CanModifyEventsData")]
    [Route("event-management/{id:guid}/cancel")]
    public IActionResult CancelEvent(Guid id, [FromBody] CancelEventRequest request)
    {
        if (!ModelState.IsValid)
        {
            NotifyModelStateErrors();
            return Response(request);
        }

        _eventAppService.CancelEvent(id, request.Reason, request.InitiateRefunds);
        return Response();
    }

    [HttpPut]
    [Authorize(Policy = "CanModifyEventsData")]
    [Route("event-management/{id:guid}/visibility")]
    public IActionResult SetEventVisibility(Guid id, [FromBody] SetEventVisibilityRequest request)
    {
        if (!ModelState.IsValid)
        {
            NotifyModelStateErrors();
            return Response(request);
        }

        _eventAppService.SetEventVisibility(id, request.Visibility);
        return Response();
    }

    [HttpPost]
    [Authorize(Policy = "CanModifyEventsData")]
    [Route("event-management/{id:guid}/invitations")]
    public IActionResult InviteUserToEvent(Guid id, [FromBody] InviteUserToEventRequest request)
    {
        if (!ModelState.IsValid)
        {
            NotifyModelStateErrors();
            return Response(request);
        }

        _eventAppService.InviteUserToEvent(id, request.UserId, request.Role);
        return Response();
    }

    [HttpDelete]
    [Authorize(Policy = "CanModifyEventsData")]
    [Route("event-management/{id:guid}/invitations/{userId:guid}")]
    public IActionResult RemoveUserInvitation(Guid id, Guid userId)
    {
        _eventAppService.RemoveUserInvitation(id, userId);
        return Response();
    }

    [HttpGet]
    [Authorize(Policy = "CanModifyEventsData")]
    [Route("event-management/{id:guid}/access/{userId:guid}")]
    public IActionResult CheckUserAccess(Guid id, Guid userId)
    {
        var hasAccess = _eventAppService.CanUserAccessEvent(id, userId);
        return Response(new { HasAccess = hasAccess });
    }

    [HttpGet]
    [Authorize]
    [Route("event-management/my-events/{organizerId:guid}")]
    public IActionResult GetMyEventsOnly(Guid organizerId)
    {
        var events = _eventAppService.GetMyEventsOnly(organizerId);
        return Response(events);
    }

    [HttpGet]
    [Authorize]
    [Route("event-management/my-events/{organizerId:guid}/status/{status}")]
    public IActionResult GetMyEventsOnlyByStatus(Guid organizerId, string status)
    {
        var events = _eventAppService.GetMyEventsOnly(organizerId, status);
        return Response(events);
    }

    [HttpGet]
    [Authorize]
    [Route("event-management/my-events/{organizerId:guid}/page")]
    public IActionResult GetMyEventsOnlyPaginated(Guid organizerId, [FromQuery] int skip = 0, [FromQuery] int take = 10)
    {
        if (take > 100) // Limit maximum page size
        {
            take = 100;
        }

        var events = _eventAppService.GetMyEventsOnly(organizerId, skip, take);
        return Response(events);
    }
}