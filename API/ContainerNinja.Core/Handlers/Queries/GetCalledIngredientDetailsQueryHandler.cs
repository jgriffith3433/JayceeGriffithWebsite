using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetCalledIngredientDetailsQuery : IRequest<CalledIngredientDetailsDTO>
    {
        public int Id { get; init; }
    }

    public class GetCalledIngredientDetailsQueryHandler : IRequestHandler<GetCalledIngredientDetailsQuery, CalledIngredientDetailsDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetCalledIngredientDetailsQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<CalledIngredientDetailsDTO> Handle(GetCalledIngredientDetailsQuery request, CancellationToken cancellationToken)
        {
            var calledIngredientDTO = _cache.GetItem<CalledIngredientDTO>($"called_ingredient_{request.Id}");
            if (calledIngredientDTO == null)
            {
                var calledIngredientEntity = _repository.CalledIngredients.Set.FirstOrDefault(co => co.Id == request.Id);

                if (calledIngredientEntity == null)
                {
                    throw new NotFoundException($"No CalledIngredient found for the Id {request.Id}");
                }

                calledIngredientDTO = _mapper.Map<CalledIngredientDTO>(calledIngredientEntity);

                _cache.SetItem($"called_ingredient_{request.Id}", calledIngredientDTO);
            }

            var calledIngredientDetailsDTO = _mapper.Map<CalledIngredientDetailsDTO>(calledIngredientDTO);

            var query = from ps in _repository.KitchenProducts.Set
                        where EF.Functions.Like(ps.Name, string.Format("%{0}%", calledIngredientDTO.Name))
                        select ps;

            calledIngredientDetailsDTO.KitchenProductSearchItems = _mapper.Map<List<KitchenProductDTO>>(query.ToList());

            return calledIngredientDetailsDTO;
        }
    }
}