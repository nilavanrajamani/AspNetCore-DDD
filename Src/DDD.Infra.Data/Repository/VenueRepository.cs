using System.Linq;
using DDD.Domain.Interfaces;
using DDD.Domain.Models;
using DDD.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DDD.Infra.Data.Repository;

public class VenueRepository : Repository<Venue>, IVenueRepository
{
    public VenueRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public Venue GetByName(string name)
    {
        return _dbSet.AsNoTracking().FirstOrDefault(v => v.Name == name);
    }
}