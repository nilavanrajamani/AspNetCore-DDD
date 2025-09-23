using System;
using System.Linq;
using DDD.Domain.Interfaces;
using DDD.Domain.Models;
using DDD.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DDD.Infra.Data.Repository;

public class EventRepository : Repository<Event>, IEventRepository
{
    public EventRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public Event GetByTitle(string title)
    {
        return _dbSet.AsNoTracking().FirstOrDefault(e => e.Title == title);
    }

    public Event GetByOrganizerAndTitle(Guid organizerId, string title)
    {
        return _dbSet.AsNoTracking()
            .FirstOrDefault(e => e.OrganizerId == organizerId && e.Title == title);
    }
}