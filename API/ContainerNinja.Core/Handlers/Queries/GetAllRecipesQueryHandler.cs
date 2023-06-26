using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using System.Text.Json;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Contracts.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetAllRecipesQuery : IRequest<GetAllRecipesVM>
    {
    }

    public class GetAllRecipesQueryHandler : IRequestHandler<GetAllRecipesQuery, GetAllRecipesVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetAllRecipesQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<GetAllRecipesVM> Handle(GetAllRecipesQuery request, CancellationToken cancellationToken)
        {
            var cachedEntities = _cache.GetItem<GetAllRecipesVM>("recipes");

            if (cachedEntities == null)
            {
                var entities = await Task.FromResult(_repository.Recipes.Set.AsEnumerable());
                var result = new GetAllRecipesVM
                {
                    Recipes = _mapper.Map<List<RecipeDTO>>(entities),
                    KitchenUnitTypes = Enum.GetValues(typeof(KitchenUnitType))
                    .Cast<KitchenUnitType>()
                    .Select(p => new KitchenUnitTypeDTO { Value = (int)p, Name = p.ToString() })
                    .ToList(),
                };

                _cache.SetItem("recipes", result);
                return result;
            }
            else
            {
                return cachedEntities;
            }
        }
    }
}