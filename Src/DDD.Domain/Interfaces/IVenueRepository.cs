using DDD.Domain.Models;

namespace DDD.Domain.Interfaces;

public interface IVenueRepository : IRepository<Venue>
{
    Venue GetByName(string name);
}