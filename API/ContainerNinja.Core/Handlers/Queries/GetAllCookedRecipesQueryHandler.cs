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
    public class GetAllCookedRecipesQuery : IRequest<GetAllCookedRecipesVM>
    {
    }

    public class GetAllCookedRecipesQueryHandler : IRequestHandler<GetAllCookedRecipesQuery, GetAllCookedRecipesVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetAllCookedRecipesQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<GetAllCookedRecipesVM> Handle(GetAllCookedRecipesQuery request, CancellationToken cancellationToken)
        {
            var cachedEntities = _cache.GetItem<GetAllCookedRecipesVM>("cooked_recipes");

            if (cachedEntities == null)
            {
                var entities = await Task.FromResult(_repository.CookedRecipes.Set.AsEnumerable());

                var result = new GetAllCookedRecipesVM
                {
                    CookedRecipes = _mapper.Map<List<CookedRecipeDTO>>(entities),

                    KitchenUnitTypes = Enum.GetValues(typeof(KitchenUnitType))
                    .Cast<KitchenUnitType>()
                    .Select(p => new KitchenUnitTypeDTO { Value = (int)p, Name = p.ToString() })
                    .ToList(),

                    RecipesOptions = _repository.Recipes.GetAll().Select(r => new RecipesOptionDTO { Value = r.Id, Name = r.Name }).ToList(),
                };

                _cache.SetItem("cooked_recipes", result);
                return result;
            }
            else
            {
                return cachedEntities;
            }
        }
    }
}