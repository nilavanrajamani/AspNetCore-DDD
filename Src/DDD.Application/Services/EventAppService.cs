using System;
using System.Collections.Generic;

using AutoMapper;
using AutoMapper.QueryableExtensions;

using DDD.Application.Interfaces;
using DDD.Application.ViewModels;
using DDD.Domain.Commands;
using DDD.Domain.Core.Bus;
using DDD.Domain.Interfaces;

namespace DDD.Application.Services;

public class EventAppService : IEventAppService
{
    private readonly IMapper _mapper;
    private readonly IEventRepository _eventRepository;
    private readonly IMediatorHandler _bus;

    public EventAppService(
        IMapper mapper,
        IEventRepository eventRepository,
        IMediatorHandler bus)
    {
        _mapper = mapper;
        _eventRepository = eventRepository;
        _bus = bus;
    }

    public IEnumerable<EventViewModel> GetAll()
    {
        return _eventRepository.GetAll().ProjectTo<EventViewModel>(_mapper.ConfigurationProvider);
    }

    public IEnumerable<EventViewModel> GetAll(int skip, int take)
    {
        return _eventRepository.GetAll()
            .Skip(skip)
            .Take(take)
            .ProjectTo<EventViewModel>(_mapper.ConfigurationProvider);
    }

    public EventViewModel GetById(Guid id)
    {
        return _mapper.Map<EventViewModel>(_eventRepository.GetById(id));
    }

    public void Register(EventViewModel eventViewModel)
    {
        var registerCommand = _mapper.Map<CreateEventCommand>(eventViewModel);
        _bus.SendCommand(registerCommand);
    }

    public void Update(EventViewModel eventViewModel)
    {
        // TODO: Implement UpdateEventCommand when needed
        throw new NotImplementedException("Update event functionality not yet implemented");
    }

    public void Remove(Guid id)
    {
        // TODO: Implement RemoveEventCommand when needed
        throw new NotImplementedException("Remove event functionality not yet implemented");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}