using DDD.Application.Interfaces;
using DDD.Domain.Core.Bus;
using DDD.Domain.Core.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDD.Services.Api.Controllers.V1;

[Authorize]
[ApiVersion("1.0")]
public class VenuesController : ApiController
{
    private readonly IVenueAppService _venueAppService;

    public VenuesController(
        IVenueAppService venueAppService,
        INotificationHandler<DomainNotification> notifications,
        IMediatorHandler mediator)
        : base(notifications, mediator)
    {
        _venueAppService = venueAppService;
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("venues")]
    public IActionResult Get()
    {
        return Response(_venueAppService.GetAll());
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("venues/{id:guid}")]
    public IActionResult Get(Guid id)
    {
        var venueViewModel = _venueAppService.GetById(id);
        return Response(venueViewModel);
    }
}