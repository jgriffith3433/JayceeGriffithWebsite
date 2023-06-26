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
    public class SearchCookedRecipeCalledIngredientProductNameQuery : IRequest<CookedRecipeCalledIngredientDetailsDTO>
    {
        public int Id { get; init; }
        public string Search { get; init; }
    }

    public class SearchCookedRecipeCalledIngredientProductNameQueryHandler : IRequestHandler<SearchCookedRecipeCalledIngredientProductNameQuery, CookedRecipeCalledIngredientDetailsDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public SearchCookedRecipeCalledIngredientProductNameQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<CookedRecipeCalledIngredientDetailsDTO> Handle(SearchCookedRecipeCalledIngredientProductNameQuery request, CancellationToken cancellationToken)
        {
            var cookedRecipeCalledIngredientDTO = _cache.GetItem<CookedRecipeCalledIngredientDTO>($"cooked_recipe_called_ingredient_{request.Id}");
            if (cookedRecipeCalledIngredientDTO == null)
            {
                var cookedRecipeCalledIngredientEntity = _repository.CookedRecipeCalledIngredients.Set.FirstOrDefault(co => co.Id == request.Id);

                if (cookedRecipeCalledIngredientEntity == null)
                {
                    throw new NotFoundException($"No CookedRecipeCalledIngredient found for the Id {request.Id}");
                }

                cookedRecipeCalledIngredientDTO = _mapper.Map<CookedRecipeCalledIngredientDTO>(cookedRecipeCalledIngredientEntity);
                _cache.SetItem($"cooked_recipe_called_ingredient_{request.Id}", cookedRecipeCalledIngredientDTO);
            }

            var cookedRecipeCalledIngredientDetailsDTO = _mapper.Map<CookedRecipeCalledIngredientDetailsDTO>(cookedRecipeCalledIngredientDTO);
            var query = from ps in _repository.KitchenProducts.Set where EF.Functions.Like(ps.Name, string.Format("%{0}%", request.Search)) select ps;

            cookedRecipeCalledIngredientDetailsDTO.KitchenProductSearchItems = _mapper.Map<List<KitchenProductDTO>>(query.ToList());

            return cookedRecipeCalledIngredientDetailsDTO;
        }
    }
}