using System;
using DDD.Application.Interfaces;
using DDD.Application.ViewModels;
using DDD.Domain.Core.Bus;
using DDD.Domain.Core.Notifications;
using DDD.Infra.CrossCutting.Identity.Authorization;
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
    [Authorize(Policy = "CanWriteCustomerData", Roles = Roles.Admin)]
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

    [HttpGet]
    [AllowAnonymous]
    [Route("event-management/pagination")]
    public IActionResult Pagination(int skip, int take)
    {
        return Response(_eventAppService.GetAll(skip, take));
    }
}