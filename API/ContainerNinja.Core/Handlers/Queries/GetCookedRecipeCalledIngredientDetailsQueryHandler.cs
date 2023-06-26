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
    public class GetCookedRecipeCalledIngredientDetailsQuery : IRequest<CookedRecipeCalledIngredientDetailsDTO>
    {
        public int Id { get; init; }
    }

    public class GetCookedRecipeCalledIngredientDetailsQueryHandler : IRequestHandler<GetCookedRecipeCalledIngredientDetailsQuery, CookedRecipeCalledIngredientDetailsDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetCookedRecipeCalledIngredientDetailsQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<CookedRecipeCalledIngredientDetailsDTO> Handle(GetCookedRecipeCalledIngredientDetailsQuery request, CancellationToken cancellationToken)
        {
            var cookedRecipeCalledIngredientDetailsDTO = _cache.GetItem<CookedRecipeCalledIngredientDetailsDTO>($"cooked_recipe_called_ingredient_{request.Id}");
            if (cookedRecipeCalledIngredientDetailsDTO == null)
            {
                var cookedRecipeCalledIngredientEntity = _repository.CookedRecipeCalledIngredients.Set.Include(crci => crci.KitchenProduct).FirstOrDefault(co => co.Id == request.Id);

                if (cookedRecipeCalledIngredientEntity == null)
                {
                    throw new NotFoundException($"No CookedRecipeCalledIngredient found for the Id {request.Id}");
                }

                cookedRecipeCalledIngredientDetailsDTO = _mapper.Map<CookedRecipeCalledIngredientDetailsDTO>(cookedRecipeCalledIngredientEntity);
                _cache.SetItem($"cooked_recipe_called_ingredient_{request.Id}", cookedRecipeCalledIngredientDetailsDTO);
            }

            var query = from ps in _repository.KitchenProducts.Set where EF.Functions.Like(ps.Name, string.Format("%{0}%", cookedRecipeCalledIngredientDetailsDTO.Name)) select ps;

            cookedRecipeCalledIngredientDetailsDTO.KitchenProductSearchItems = _mapper.Map<List<KitchenProductDTO>>(query.ToList());

            return cookedRecipeCalledIngredientDetailsDTO;
        }
    }
}