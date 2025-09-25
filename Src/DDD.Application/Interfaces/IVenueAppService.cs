using DDD.Application.ViewModels;

namespace DDD.Application.Interfaces;

public interface IVenueAppService : IDisposable
{
    IEnumerable<VenueViewModel> GetAll();

    VenueViewModel GetById(Guid id);
}
