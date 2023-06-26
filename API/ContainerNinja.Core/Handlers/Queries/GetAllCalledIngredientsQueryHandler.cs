using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetAllCalledIngredientsQuery : IRequest<GetAllCalledIngredientsVM>
    {
    }

    public class GetAllCalledIngredientsQueryHandler : IRequestHandler<GetAllCalledIngredientsQuery, GetAllCalledIngredientsVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetAllCalledIngredientsQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<GetAllCalledIngredientsVM> Handle(GetAllCalledIngredientsQuery request, CancellationToken cancellationToken)
        {
            var cachedEntities = _cache.GetItem<GetAllCalledIngredientsVM>("called_ingredients");

            if (cachedEntities == null)
            {
                var entities = await Task.FromResult(_repository.CalledIngredients.Set.Include(ci => ci.Recipe).AsEnumerable());
                var result = new GetAllCalledIngredientsVM
                {
                    CalledIngredients = _mapper.Map<List<CalledIngredientDTO>>(entities),
                };

                _cache.SetItem("called_ingredients", result);
                return result;
            }
            else
            {
                return cachedEntities;
            }
        }
    }
}