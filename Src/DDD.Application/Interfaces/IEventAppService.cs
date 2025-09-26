using System;
using System.Collections.Generic;

using DDD.Application.ViewModels;

namespace DDD.Application.Interfaces;

public interface IEventAppService : IDisposable
{
    void Register(EventViewModel eventViewModel);

    IEnumerable<EventViewModel> GetAll();

    IEnumerable<EventViewModel> GetAll(int skip, int take);

    EventViewModel GetById(Guid id);

    void Update(EventViewModel eventViewModel);

    void Remove(Guid id);

    void SetCapacityAndPricing(SetEventCapacityViewModel capacityViewModel);

    SetEventCapacityViewModel GetCapacityAndPricing(Guid eventId);

    void PublishEvent(Guid eventId);

    void UnpublishEvent(Guid eventId, string reason);
}