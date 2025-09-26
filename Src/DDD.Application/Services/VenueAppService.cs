using AutoMapper;

using DDD.Application.Interfaces;
using DDD.Application.ViewModels;
using DDD.Domain.Interfaces;

namespace DDD.Application.Services;

public class VenueAppService : IVenueAppService
{
    private readonly IVenueRepository _venueRepository;
    private readonly IMapper _mapper;

    public VenueAppService(IVenueRepository venueRepository, IMapper mapper)
    {
        _venueRepository = venueRepository;
        _mapper = mapper;
    }

    public IEnumerable<VenueViewModel> GetAll()
    {
        return _mapper.Map<IEnumerable<VenueViewModel>>(_venueRepository.GetAll());
    }

    public VenueViewModel GetById(Guid id)
    {
        return _mapper.Map<VenueViewModel>(_venueRepository.GetById(id));
    }

    public void Dispose()
    {
        _venueRepository?.Dispose();
    }
}
