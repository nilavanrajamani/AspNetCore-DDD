using System;
using DDD.Domain.Models;

namespace DDD.Domain.Interfaces;

public interface IEventRepository : IRepository<Event>
{
    Event GetByTitle(string title);

    Event GetByOrganizerAndTitle(Guid organizerId, string title);
}